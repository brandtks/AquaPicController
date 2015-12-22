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
        public static AquaPicBus Bus1 = new AquaPicBus (2, 1000);

        private SerialPort uart;
        private Queue messageBuffer;
        private Thread txRxThread;
        private Thread responseThread;
        private AutoResetEvent getInput, gotInput;
        private bool responseReceived;
        private Stopwatch stopwatch;
        private List<Slave> slaves;
        private ReceiveBuffer receiveBuffer;
        public int retryCount, readTimeout;

        public int slaveCount {
            get {
                return slaves.Count;
            }
        }

        public string[] slaveNames {
            get {
                string[] names = new string[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    names [i++] = s.Name;
                return names;
            }
        }

        public int[] slaveAdresses {
            get {
                int[] address = new int[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    address [i++] = s.Address;
                return address;
            }
        }

        public AquaPicBusStatus[] slaveStatus {
            get {
                AquaPicBusStatus[] status = new AquaPicBusStatus[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    status [i++] = s.Status;
                return status;
            }
        }

        public int[] slaveResponseTimes {
            get {
                int[] time = new int[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    time [i++] = s.ResponeTime;
                return time;
            }
        }

        public bool isOpen {
            get {
                if (uart != null)
                    return uart.IsOpen;
                else
                    return false;
            }
        }

        public string portName {
            get {
                if (uart != null)
                    return uart.PortName;
                else
                    return string.Empty;
            }
        }

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
            receiveBuffer = new ReceiveBuffer ();
        }

        //57600
        public void Open (string port, int baudRate = 57600) {
            try {
                if (Utils.RunningPlatform == Platform.Windows)
                    uart = new SerialPort (port, baudRate, Parity.Space, 8);
                else
                    uart = new MonoSerialPort (port, baudRate, Parity.Space, 8);

                uart.StopBits = StopBits.One;
                uart.Handshake = Handshake.None;
                uart.ReadTimeout = 1000;
                uart.DataReceived += new SerialDataReceivedEventHandler (uartDataReceived);

                if (Utils.RunningPlatform == Platform.Windows)
                    uart.Open ();
                else {
                    MonoSerialPort msp = uart as MonoSerialPort;
                    if (msp != null)
                        msp.Open ();
                    else
                        throw new Exception ("Mono serial port not initialized correctly");
                }

                if (uart.IsOpen) {
                    for (int i = 0; i < slaves.Count; ++i)
                        slaves [i].UpdateStatus (AquaPicBusStatus.open, 0);
                }

                txRxThread.Start ();
                responseThread.Start ();
            } catch (Exception ex) {
                Logger.AddError (ex.ToString ());
            }
        }

        public void Close () {
            if (uart.IsOpen) {
                txRxThread.Abort ();
                responseThread.Abort ();
                uart.Close ();
                uart.Dispose ();
            }
        }

        private bool IsAddressOk (byte a) {
            for (int i = 0; i < slaves.Count; ++i) {
                if (slaves [i].Address == a)
                    return false;
            }
            return true;
        }

        #if UNSAFE_COMMS
        private unsafe void QueueMessage (Slave slave, byte func, void* writeData, int writeSize, int readSize, ResponseCallback callback) {
            // <TEST> if uart is null the port isn't open so don't queue the message
            if (uart != null)
                messageBuffer.Enqueue (new InternalMessage (slave, func, writeData, writeSize, readSize, callback));
        }
        #else
        private void QueueMessage (Slave slave, byte func, byte[] writeData, int writeSize, int readSize, ResponseCallback callback) {
            // <TEST> if uart is null the port isn't open so don't queue the message
            if (uart != null)
                messageBuffer.Enqueue (new InternalMessage (slave, func, writeData, writeSize, readSize, callback));
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
                    if (count > 8)
                        Logger.AddWarning (string.Format ("Message queue count is {0}", count));

                    InternalMessage m;

                    lock (messageBuffer.SyncRoot) {
                        m = (InternalMessage)messageBuffer.Dequeue ();
                    }

                    if (uart.IsOpen) {
                        m.slave.UpdateStatus (AquaPicBusStatus.communicationStart, 0);
                        //uart.ReceivedBytesThreshold = m.responseLength;

                        try {
                            for (int i = 0; i < retryCount; ++i) {
                                uart.DiscardInBuffer ();
                                uart.DiscardOutBuffer ();

                                lock (receiveBuffer.SyncLock) {
                                    receiveBuffer.length = m.responseLength;
                                    receiveBuffer.buffer.Clear ();
                                }

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
                                    getResponse (ref m.readBuffer);

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
                                        Logger.AddWarning ("APB {0} crc error", m.slave.Address);
                                        Alarm.Post (m.slave.alarmIdx);
                                    }
                                } catch (TimeoutException) {
                                    m.slave.UpdateStatus (AquaPicBusStatus.timeout, readTimeout);
                                    Logger.AddWarning ("APB {0} timeout", m.slave.Address);
                                }
                            }
                        } catch (Exception ex) {
                            m.slave.UpdateStatus (AquaPicBusStatus.exception, 1000);
                            Logger.AddError (ex.ToString ());
                            Alarm.Post (m.slave.alarmIdx);
                        }
                    } else {
                        m.slave.UpdateStatus (AquaPicBusStatus.notOpen, 0);
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
                lock (receiveBuffer.SyncLock) {
                    response = receiveBuffer.buffer.ToArray ();
                }
            } else
                throw new TimeoutException("UART response timeout");
        }

        private void uartDataReceived (object sender, SerialDataReceivedEventArgs e) {
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

                if (receiveBuffer.buffer.Count == receiveBuffer.length)
                    responseReceived = true;
            }
        }

        private void WriteWithParity (byte data, Parity p = Parity.Space) {
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
            public int length;
            public List<byte> buffer;

            public ReceiveBuffer () {
                SyncLock = new object ();
                buffer = new List<byte> ();
                length = 0;
            }
        }
    }
}

