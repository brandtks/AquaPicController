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
 *   0      : Address of slave
 *   1      : Function Number
 *   2      : Number of Bytes to be received
 *   3-n    : Message - if Any
 *   last 2 : CRC
 * */

using System;
using System.IO;
using System.IO.Ports; // for SerialPort
using System.Threading; // for Thread, AutoResetEvent
using System.Diagnostics; // for Stopwatch
using System.Collections; // for Queue
using System.Collections.Generic; // for List
using Gtk; // for Application.Invoke
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.SerialBus
{
    public partial class AquaPicBus
    {
        private static SerialPort uart;
        private static Queue messageBuffer;
        private static Thread txRxThread;
        private static Thread responseThread;
        private static bool enableTxRx;
        private static bool enableResponse;
        private static AutoResetEvent getInput, gotInput;
        private static Stopwatch stopwatch;
        private static List<Slave> slaves;
        private static ReceiveBuffer receiveBuffer;
        public static int retryCount, readTimeout;

        #if USE_SERIAL_RECEIVE_EVENT
        private static bool responseReceived;
        #endif

        public static int slaveCount {
            get {
                return slaves.Count;
            }
        }

        public static string[] slaveNames {
            get {
                string[] names = new string[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    names [i++] = s.Name;
                return names;
            }
        }

        public static int[] slaveAdresses {
            get {
                int[] address = new int[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    address [i++] = s.Address;
                return address;
            }
        }

        public static AquaPicBusStatus[] slaveStatus {
            get {
                AquaPicBusStatus[] status = new AquaPicBusStatus[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    status [i++] = s.Status;
                return status;
            }
        }

        public static int[] slaveResponseTimes {
            get {
                int[] time = new int[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    time [i++] = s.ResponeTime;
                return time;
            }
        }

        public static bool isOpen {
            get {
                if (uart != null)
                    return uart.IsOpen;
                else
                    return false;
            }
        }

        public static string portName {
            get {
                if (uart != null)
                    return uart.PortName;
                else
                    return string.Empty;
            }
        }

        static AquaPicBus () {
            messageBuffer = new Queue ();

            txRxThread = new Thread (txRx);
            txRxThread.IsBackground = true;
            enableTxRx = false;

            responseThread = new Thread (responseTimeout);
            responseThread.IsBackground = true;
            enableResponse = false;

            getInput = new AutoResetEvent (false);
            gotInput = new AutoResetEvent (false);

            #if USE_SERIAL_RECEIVE_EVENT
            responseReceived = false;
            #endif

            stopwatch = new Stopwatch ();
            slaves = new List<Slave> ();

            retryCount = 2;
            readTimeout = 1000;

            receiveBuffer = new ReceiveBuffer ();
        }

        //57600
        public static void Open (string port, int baudRate = 57600) {
            try {
                #if USE_SERIAL_RECEIVE_EVENT
                if (Utils.RunningPlatform == Platform.Windows)
                #endif
                    uart = new SerialPort (port, baudRate, Parity.Space, 8);
                #if USE_SERIAL_RECEIVE_EVENT
                else
                    uart = new MonoSerialPort (port, baudRate, Parity.Space, 8);
                #endif

                uart.StopBits = StopBits.One;
                uart.Handshake = Handshake.None;
                //uart.ReadTimeout = readTimeout;

                #if USE_SERIAL_RECEIVE_EVENT
                uart.DataReceived += new SerialDataReceivedEventHandler (uartDataReceived);

                if (Utils.RunningPlatform == Platform.Windows)
                #endif
                    uart.Open ();
                #if USE_SERIAL_RECEIVE_EVENT
                else {
                    MonoSerialPort msp = uart as MonoSerialPort;
                    if (msp != null)
                        msp.Open ();
                    else
                        throw new Exception ("Mono serial port not initialized correctly");
                }
                #endif

                if (uart.IsOpen) {
                    for (int i = 0; i < slaves.Count; ++i)
                        slaves [i].UpdateStatus (AquaPicBusStatus.open, 0);
                }

                enableTxRx = true;
                txRxThread.Start ();
                enableResponse = true;
                responseThread.Start ();
            } catch (Exception ex) {
                Logger.AddError (ex.ToString ());
            }
        }

        public static void Close () {
            if (uart.IsOpen) {
                enableTxRx = false;
                txRxThread.Join (1000);
                enableResponse = false;
                responseThread.Join (1000);
                uart.Close ();
                uart.Dispose ();
            }
        }

        private static bool IsAddressOk (byte a) {
            for (int i = 0; i < slaves.Count; ++i) {
                if (slaves [i].Address == a)
                    return false;
            }
            return true;
        }

        #if UNSAFE_COMMS
        private unsafe static void QueueMessage (Slave slave, byte func, void* writeData, int writeSize, int readSize, ResponseCallback callback) {
            // <TEST> if uart is null the port isn't open so don't queue the message
            if (uart != null) {
                lock (messageBuffer.SyncRoot) {
                    messageBuffer.Enqueue (new InternalMessage (slave, func, writeData, writeSize, readSize, callback));
                }
            }
        }
        #else
        private static void QueueMessage (Slave slave, byte func, byte[] writeData, int writeSize, int readSize, ResponseCallback callback) {
            // <TEST> if uart is null the port isn't open so don't queue the message
            if (uart != null) {
                lock (messageBuffer.SyncRoot) {
                    messageBuffer.Enqueue (new InternalMessage (slave, func, writeData, writeSize, readSize, callback));
                }
            }
        }
        #endif

        // background thread to dequeue any messages and send to slave
        // waits for response and calls callback if required
        private static void txRx () {
            while (enableTxRx) {
                int count;
                lock (messageBuffer.SyncRoot) {
                    count = messageBuffer.Count;
                }

                if (count > 0) {
                    if (count > 8) {
                        Gtk.Application.Invoke ((sender, e) => {
                            Logger.AddWarning (string.Format ("Message queue count is {0}", count));
                        });
                    }

                    InternalMessage m;

                    lock (messageBuffer.SyncRoot) {
                        m = (InternalMessage)messageBuffer.Dequeue ();
                    }

                    if (uart.IsOpen) {
                        m.slave.UpdateStatus (AquaPicBusStatus.communicationStart, 0);
                        //uart.ReceivedBytesThreshold = m.responseLength;

                        try {
                            for (int i = 0; i < retryCount; ++i) {
                                uart.DiscardOutBuffer ();
                                uart.DiscardInBuffer ();

                                #if DEBUG_SERIAL
                                Console.WriteLine ();
                                Console.WriteLine ("Start Message");
                                foreach (var w in m.writeBuffer)
                                    Console.WriteLine ("{0:X}", w);
                                #endif

                                if (Utils.RunningPlatform == Platform.Windows) {
                                    uart.Parity = Parity.Mark;
                                    uart.Write (m.writeBuffer, 0, 1);
                                    uart.Parity = Parity.Space;

                                    uart.Write (m.writeBuffer, 1, m.writeBuffer.Length - 1);
                                } else {
                                    WriteWithParity (m.writeBuffer [0], Parity.Mark);

                                    for (int index = 1; index < m.writeBuffer.Length; ++index) {
                                        WriteWithParity (m.writeBuffer [index]);
                                    }
                                }

                                stopwatch.Restart (); // resets stopwatch for response time, getResponse(ref byte[]) stops it

                                try {
                                    lock (receiveBuffer.SyncLock) {
                                        receiveBuffer.responseLength = m.responseLength;
                                        receiveBuffer.buffer.Clear ();
                                    }

                                    getResponse (ref m.readBuffer);

                                    if (m.readBuffer.Length >= m.responseLength) {
                                        if (checkResponse (ref m.readBuffer)) {
                                            if (m.callback != null) {
                                                Gtk.Application.Invoke ((sender, e) => m.callback (new CallbackArgs (m.readBuffer)));
                                            }

                                            #if DEBUG_SERIAL
                                            Console.WriteLine ("Message ok");
                                            #endif

                                            if (Alarm.CheckAlarming (m.slave.alarmIdx))
                                                Alarm.Clear (m.slave.alarmIdx);

                                            m.slave.UpdateStatus (AquaPicBusStatus.communicationSuccess, (int)stopwatch.ElapsedMilliseconds);
                                            break;
                                        } else {
                                            m.slave.UpdateStatus (AquaPicBusStatus.crcError, 1000);
                                            Gtk.Application.Invoke ((sender, e) => {
                                                Logger.AddWarning ("APB {0} crc error on function number {1}", m.slave.Address, m.writeBuffer [1]);
                                                Alarm.Post (m.slave.alarmIdx);
                                            });
                                        }
                                    } else {
                                        m.slave.UpdateStatus (AquaPicBusStatus.crcError, 1000);
                                        Gtk.Application.Invoke ((sender, e) => {
                                            Logger.AddWarning ("APB {0} response length error on function number {1}", m.slave.Address, m.writeBuffer [1]);
                                        });
                                    }
                                } catch (TimeoutException) {
                                    m.slave.UpdateStatus (AquaPicBusStatus.timeout, readTimeout);
                                    Gtk.Application.Invoke ((sender, e) => {
                                        Logger.AddWarning ("APB {0} timeout on function number {1}", m.slave.Address, m.writeBuffer [1]);
                                    });
                                }
                            }
                        } catch (Exception ex) {
                            m.slave.UpdateStatus (AquaPicBusStatus.exception, 1000);
                            Gtk.Application.Invoke ((sender, e) => {
                                Logger.AddError (ex.ToString ());
                                Alarm.Post (m.slave.alarmIdx);
                            });
                        }
                    } else {
                        m.slave.UpdateStatus (AquaPicBusStatus.notOpen, 0);
                    }
                }
            }
        }

        #if USE_SERIAL_RECEIVE_EVENT
        private static void responseTimeout () {
            while (enableResponse) {
                getInput.WaitOne (); // never returns until getInput.Set() is called 
                while (!responseReceived) // waits until responseReceived is true, set by SerialPort Received Event
                    continue;
                gotInput.Set (); // sets gotInput Event if response
            }
        }
        #else
        private static void responseTimeout () {
            bool loopResponse;

            while (enableResponse) {
                getInput.WaitOne (); // never returns until getInput.Set() is called 

                lock (receiveBuffer.SyncLock) {
                    loopResponse = receiveBuffer.waitForResponse;
                }

                while (loopResponse) {
                    int size = uart.BytesToRead;

                    lock (receiveBuffer.SyncLock) {
                        if (size == receiveBuffer.responseLength) {
                            byte[] b = new byte[size];
                            uart.Read (b, 0, size);
                            receiveBuffer.buffer.AddRange (b);
                            receiveBuffer.waitForResponse = false;
                        }

                        loopResponse = receiveBuffer.waitForResponse;
                    }
                }

                gotInput.Set (); // sets gotInput Event if response
            }
        }
        #endif

        private static void getResponse (ref byte[] response) {
            #if USE_SERIAL_RECEIVE_EVENT
            responseReceived = false;
            #else
            lock (receiveBuffer.SyncLock) {
                receiveBuffer.waitForResponse = true;
            }
            #endif
            getInput.Set ();
            // waits readTimeout for respone and returns true if gotInput.Set() was called or false if not
            bool success = gotInput.WaitOne (readTimeout);
            stopwatch.Stop (); // stops stopwatch to determine latency of slave device
            if (success) {
                lock (receiveBuffer.SyncLock) {
                    response = receiveBuffer.buffer.ToArray ();
                }
            } else {
                #if !USE_SERIAL_RECEIVE_EVENT
                lock (receiveBuffer.SyncLock) {
                    receiveBuffer.waitForResponse = false;
                }
                #endif
                throw new TimeoutException ("UART response timeout");
            }
        }

        #if USE_SERIAL_RECEIVE_EVENT
        private static void uartDataReceived (object sender, SerialDataReceivedEventArgs e) {
            lock (receiveBuffer.SyncLock) {
                int size = uart.BytesToRead;

                byte[] b = new byte[size];
                uart.Read (b, 0, size);

                #if DEBUG_SERIAL
                Console.WriteLine ("BytesToRead: {0}. Response...", size);
                foreach (var bb in b)
                    Console.WriteLine ("{0:X}", bb);
                #endif

                receiveBuffer.buffer.AddRange (b);

                if (receiveBuffer.buffer.Count == receiveBuffer.responseLength)
                    responseReceived = true;
            }
        }
        #endif

        private static void WriteWithParity (byte data, Parity p = Parity.Space) {
            int count = 0;

            for (int i = 0; i < 8; ++i) {
                if (((data >> i) & 0x01) == 1)
                    ++count;
            }

            if ((count % 2) == 0) { // even number
                if (p == Parity.Mark)
                    uart.Parity = Parity.Even;
                else
                    uart.Parity = Parity.Odd;
            } else { // odd number
                if (p == Parity.Space)
                    uart.Parity = Parity.Odd;
                else
                    uart.Parity = Parity.Even;
            }

            uart.Write (new byte[] { data }, 0, 1);
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

        private class ReceiveBuffer {
            public object SyncLock;
            public int responseLength;
            public List<byte> buffer;
            public bool waitForResponse;

            public ReceiveBuffer () {
                SyncLock = new object ();
                buffer = new List<byte> ();
                responseLength = 0;
                waitForResponse = false;
            }
        }
    }
}

