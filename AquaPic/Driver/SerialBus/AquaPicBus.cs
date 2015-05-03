/* Message Structure
 * 
 * SENDING FROM MASTER
 *   0      : Address
 *   1      : Function Number
 *   2      : Number of Bytes to be sent
 *   3-n    : Message - if Any
 *   last 2 : CRC
 * 
 *  RECEIVEING FROM SLAVE
 *   0      : Address
 *   1      : Function Number
 *   2      : Number of Bytes to be received
 *   3-n    : Message - if Any
 *   last 2 : CRC
 * */

using System;
using System.IO.Ports; // for SerialPort
using System.Threading; // for Thread, AutoResetEvent
using System.Diagnostics; // for Stopwatch
using System.Collections; // for Queue
using System.Collections.Generic; // for List
using Gtk; // for Application.Invoke

#if SIMULATION
using System.IO;
using System.Text;
#endif

namespace AquaPic.SerialBus
{
    public partial class AquaPicBus
    {
        #if SIMULATION
        public string textFilePath; 
        #endif

        public static AquaPicBus Bus1 = new AquaPicBus (2, 1000);

        #if !SIMULATION
        private SerialPort uart;
        #endif
        private Queue messageBuffer;
        private Thread txRxThread;
        private Thread responseThread;
        private AutoResetEvent getInput, gotInput;
        private bool responseReceived;
        private Stopwatch stopwatch;
        private List<Slave> slaves;
        public int retryCount, readTimeout;

        private AquaPicBus (int retryCount, int responseTimeout) {
            this.messageBuffer = new Queue ();
            this.txRxThread = new Thread (this.txRx);
            this.txRxThread.IsBackground = true;
            this.responseThread = new Thread (this.responseTimeout);
            this.responseThread.IsBackground = true;
            this.getInput = new AutoResetEvent (false);
            this.gotInput = new AutoResetEvent (false);
            this.responseReceived = false;
            this.stopwatch = new Stopwatch ();
            this.slaves = new List<Slave> ();
            this.retryCount = retryCount;
            this.readTimeout = responseTimeout;
        }

        #if SIMULATION
        public void Start () {
            const string FILENAME = @"\AquaPic\Simulation.txt";
            string path = Environment.GetEnvironmentVariable ("AquaPic");
            textFilePath = string.Format ("{0}{1}", path, FILENAME);
            File.WriteAllText (textFilePath, string.Empty);
            txRxThread.Start ();
        }
        #else
        public void Open (string port, int baudRate) {
            try {
                //<TODO> Determine port for Radxa Rock "/dev/ttyAMA0"
                uart = new SerialPort (port, baudRate, Parity.Space, 8);
                uart.StopBits = StopBits.One;
                uart.Handshake = Handshake.None;
                uart.DataReceived += new SerialDataReceivedEventHandler (uartDataReceived);
                uart.Open ();

                if (uart.IsOpen) {
                    for (int i = 0; i < slaves.Count; ++i)
                        slaves [i].updateStatus (AquaPicBusStatus.open, 0);
                }

                txRxThread.Start ();
                responseThread.Start ();
            } catch (Exception ex) {
                Console.WriteLine (ex.ToString ());
                Console.WriteLine (ex.Message);
            }
            
        }
        #endif

        private bool IsAddressOk (byte a) {
            for (int i = 0; i < slaves.Count; ++i) {
                if (slaves [i].Address == a)
                    return false;
            }
            return true;
        }

        #if SIMULATION
        private void queueMessage (Slave slave, int func, string[] writeMessage, int writeSize, int readSize, ResponseCallback callback) {
            messageBuffer.Enqueue (new Message (slave, func, writeMessage, writeSize, readSize, callback));
        }
        #else
        private unsafe void queueMessage (Slave slave, byte func, void* writeData, int writeSize, int readSize, ResponseCallback callback) {
            messageBuffer.Enqueue (new Message (slave, func, writeData, writeSize, readSize, callback));
        }
        #endif

        // background thread to dequeue any messages and send to slave
        // waits for response and calls callback if required
        private void txRx () {
            while (true) {
                int count;
                lock (messageBuffer.SyncRoot) {
                    count = messageBuffer.Count;
                }

                if (count > 0) {
                    if (count > 8) {
                        Console.WriteLine ("Message queue count is {0}", count);
                    }

                    Message m;

                    lock (messageBuffer.SyncRoot) {
                        m = (Message)messageBuffer.Dequeue ();
                    }

                    #if SIMULATION
                    bool fileOpen = false;
                    do {
                        try {
                            using (FileStream fs = File.Open (textFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
                                StringBuilder sb = new StringBuilder ();
                                sb.AppendLine ("master");
                                foreach (string line in m.writeData)
                                    sb.AppendLine (line);

                                fs.SetLength (0);
                                byte[] wb = Encoding.UTF8.GetBytes (sb.ToString ());
                                fs.Write (wb, 0, wb.Length);
                            }
                            fileOpen = true;
                        } catch {
                            Console.WriteLine ("File is most likely being read");
                            //Console.WriteLine (ex.ToString ());
                            Thread.Sleep (1000);
                        }
                    } while (!fileOpen);

                    Thread.Sleep (500);

                    fileOpen = false;
                    do {
                        try {
                            using (FileStream fs = File.Open (textFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
                                char[] splitChars = { '\n' };
                                byte[] b = new byte[fs.Length];
                                fs.Read (b, 0, (int)fs.Length);
                                string stringMessage = Encoding.UTF8.GetString (b);
                                string[] lines = stringMessage.Split (splitChars);
                                if (lines[0].StartsWith ("slave", StringComparison.InvariantCultureIgnoreCase)) {
                                    for (int i = 0; i < m.responseLength; ++i)
                                        m.readData [i] = lines [1 + i];

                                    if (m.callback != null) {
                                        Gtk.Application.Invoke ( delegate {
                                            m.callback (new CallbackArgs (m.readData));
                                        });
                                    }

                                    fileOpen = true;
                                }
                                else {
                                    Console.WriteLine ("the slave has not responded");
                                }
                            }

                            if (!fileOpen)
                                Thread.Sleep (1000);
                        } catch {
                            Console.WriteLine ("File is most likely being read");
                            //Console.WriteLine (ex.ToString ());
                            Thread.Sleep (1000);
                        }
                    } while (!fileOpen);
                    #else
                    if (uart.IsOpen) {
                        m.slave.updateStatus (AquaPicBusStatus.communicationStart, 0);
                        uart.ReceivedBytesThreshold = m.responseLength;

                        try {
                            for (int i = 0; i < retryCount; ++i) {
                                uart.DiscardInBuffer ();
                                uart.DiscardOutBuffer ();

                                uart.Parity = Parity.Mark;
                                uart.Write (m.writeBuffer, 0, 1);
                                Thread.Sleep (50); // wait 50msecs for slave to wake up
                                uart.Parity = Parity.Space;

                                uart.Write (m.writeBuffer, 1, m.writeBuffer.Length - 1);

                                stopwatch.Restart (); // resets stopwatch for response time, getResponse(ref byte[]) stops it

                                try {
                                    getResponse (ref m.readBuffer);

                                    if (checkResponse (ref m.readBuffer)) {
                                        if (m.callback != null) {
                                            Gtk.Application.Invoke ( delegate {
                                                m.callback (new CallbackArgs (m.readBuffer));
                                            });
                                        }
                                        m.slave.updateStatus (AquaPicBusStatus.communicationSuccess, (int)stopwatch.ElapsedMilliseconds);
                                        break;
                                    } else {
                                        m.slave.updateStatus (AquaPicBusStatus.crcError, 0);
                                    }
                                } catch (TimeoutException) {
                                    m.slave.updateStatus (AquaPicBusStatus.timeout, readTimeout);
                                }
                            }
                        } catch (Exception ex) {
                            Console.WriteLine (ex.ToString ());
                            m.slave.updateStatus (AquaPicBusStatus.exception, 0);
                        }
                    } else {
                        m.slave.updateStatus (AquaPicBusStatus.notOpen, 0);
                    }
                    #endif
                }
            }
        }

        private void responseTimeout () {
            while (true) {
                getInput.WaitOne (); // never returns until getInput.Set() is called 
                while (!responseReceived) // waits until responseReceived is true, set by SerialPort Received Event
                    continue;
                gotInput.Set (); // sets gotInput Event if response
            }
        }

        #if !SIMULATION
        private void getResponse (ref byte[] response) {
            responseReceived = false;
            getInput.Set ();
            // waits readTimeout for respone and returns true if gotInput.Set() was called or false if not
            bool success = gotInput.WaitOne (readTimeout);
            stopwatch.Stop (); // stops stopwatch to determine latency of slave device
            if (success) {
                uart.Read (response, 0, response.Length);
            } else
                throw new TimeoutException("UART response timeout");
        }
        #endif

        private void uartDataReceived (object sender, SerialDataReceivedEventArgs e) {
            responseReceived = true;
        }

        private static bool checkResponse (ref byte[] response) {
            byte[] crc = new byte[2];
            crc16 (ref response, ref crc); 
            if ((crc[0] == response[response.Length - 2]) && (crc[1] == response[response.Length - 1]))
                return true;
            else
                return false;
        }

        //From distantcity on CodeProject - Simple Modbus Protocol in C# / .NET 2.0
        //url: http://www.codeproject.com/Articles/20929/Simple-Modbus-Protocol-in-C-NET
        private static void crc16 (ref byte[] message, ref byte[] crc) {
            ushort CRCFull = 0xFFFF;
            char CRCLSB;

            for (int i = 0; i < (message.Length) - 2; ++i) {
                CRCFull = (ushort)(CRCFull ^ message[i]);

                for (int j = 0; j < 8; j++) {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }

            crc[1] = (byte)((CRCFull >> 8) & 0xFF);
            crc[0] = (byte)(CRCFull & 0xFF);
        }
    }
}

