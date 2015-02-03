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

namespace AquaPic.SerialBus
{
    public partial class AquaPicBus
    {
        static public AquaPicBus Bus1 = new AquaPicBus (2, 1000);

        private SerialPort uart;
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

        private bool IsAddressOk (byte a) {
            for (int i = 0; i < slaves.Count; ++i) {
                if (slaves [i].address == a)
                    return false;
            }
            return true;
        }

        private unsafe void queueMessage (Slave slave, byte func, void* writeData, int writeSize, int readSize, ResponseCallback callback) {
            messageBuffer.Enqueue (new Message (slave, func, writeData, writeSize, readSize, callback));
        }

        // background thread to dequeue any messages and send to slave
        // waits for response and calls callback if required
        private void txRx () {
            while (true) {
                int count;
                lock (messageBuffer.SyncRoot) {
                    count = messageBuffer.Count;
                }

                if (count > 0) {
                    Message m;

                    lock (messageBuffer.SyncRoot) {
                        m = (Message)messageBuffer.Dequeue ();
                    }

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

