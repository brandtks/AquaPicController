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

//#define DEBUG_SERIAL

using System;
using System.Threading;             // for Thread, AutoResetEvent
using System.Diagnostics;           // for Stopwatch
using System.Collections;           // for Queue
using System.Collections.Generic;   // for List
using Gtk;                          // for Application.Invoke
using AquaPic.Runtime;              // for Logger, Alarm

namespace AquaPic.SerialBus
{
    public partial class AquaPicBus
    {
        private static AquaPicBusSerialPort apbPort;
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
                if (apbPort.uart != null)
                    return apbPort.uart.IsOpen;
                else
                    return false;
            }
        }

        public static string portName {
            get {
                if (apbPort.uart != null)
                    return apbPort.uart.PortName;
                else
                    return string.Empty;
            }
        }

        static AquaPicBus () {
            apbPort = new AquaPicBusSerialPort ();
            messageBuffer = new Queue ();

            retryCount = 2;
            readTimeout = 50;

            txRxThread = new Thread (txRx);
            txRxThread.IsBackground = true;
            enableTxRx = false;

            responseThread = new Thread (responseTimeout);
            responseThread.IsBackground = true;
            enableResponse = false;
            receiveBuffer = new ReceiveBuffer ();

            getInput = new AutoResetEvent (false);
            gotInput = new AutoResetEvent (false);

            stopwatch = new Stopwatch ();
            slaves = new List<Slave> ();
        }

        //57600
        //RPi gpio uart port is /dev/ttyAMA0
        public static void Open (string port, int baudRate = 57600) {
            try {
                apbPort.Open (port, baudRate);

                if (apbPort.uart.IsOpen) {
                    for (int i = 0; i < slaves.Count; ++i)
                        slaves [i].UpdateStatus (AquaPicBusStatus.Open, 0);
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
            if (apbPort.uart.IsOpen) {
                enableTxRx = false;
                txRxThread.Join (1000);
                enableResponse = false;
                responseThread.Join (1000);
                apbPort.uart.Close ();
                apbPort.uart.Dispose ();
            }
        }

        private static bool IsAddressOk (byte a) {
            for (int i = 0; i < slaves.Count; ++i) {
                if (slaves [i].Address == a)
                    return false;
            }
            return true;
        }

        private static void QueueMessage (
            Slave slave, 
            byte func, 
            byte[] writeData, 
            int writeSize, 
            int readSize, 
            ResponseCallback callback, 
            bool queueDuringPortClosed) 
        {
            if (isOpen || queueDuringPortClosed) {
                lock (messageBuffer.SyncRoot) {
                    messageBuffer.Enqueue (new InternalMessage (slave, func, writeData, writeSize, readSize, callback));
                }
            }
        }

        // background thread to dequeue any messages and send to slave
        // waits for response and calls callback if required
        private static void txRx () {
            while (enableTxRx) {
                int count;
                lock (messageBuffer.SyncRoot) {
                    count = messageBuffer.Count;
                }

                if (count > 0) { //We've got messages, lets send them
                    if (count > 8) {
                        Gtk.Application.Invoke ((sender, e) => {
                            Logger.AddWarning (string.Format ("Message queue count is {0}", count));
                        });
                    }

                    #if DEBUG_SERIAL
                    Console.WriteLine ();
                    Console.WriteLine ("*****************Start Message*****************");
                    #endif

                    InternalMessage m;

                    lock (messageBuffer.SyncRoot) {
                        m = (InternalMessage)messageBuffer.Dequeue ();
                    }

                    if (apbPort.uart.IsOpen) {
                        m.slave.UpdateStatus (AquaPicBusStatus.CommunicationStart, 0);

                        try {
                            for (int i = 0; i < retryCount; ++i) {
                                apbPort.uart.DiscardOutBuffer ();
                                apbPort.uart.DiscardInBuffer ();

                                #if DEBUG_SERIAL
                                Console.WriteLine ("Sent Message");
                                foreach (var w in m.writeBuffer) {
                                    Console.WriteLine ("{0:X}", w);
                                }
                                #endif

                                // write message
                                apbPort.Write (m.writeBuffer);

                                // wait for response
                                stopwatch.Restart (); // resets stopwatch for response time, getResponse(ref byte[]) stops it

                                lock (receiveBuffer.SyncLock) {
                                    receiveBuffer.responseLength = m.responseLength;
                                    receiveBuffer.buffer.Clear ();
                                }

                                try {
                                    getResponse (ref m.readBuffer);
                                } catch (TimeoutException) {
                                    m.slave.UpdateStatus (AquaPicBusStatus.Timeout, readTimeout);
                                    Gtk.Application.Invoke ((sender, e) => {
                                        Logger.AddWarning ("APB {0} timeout on function number {1}", m.slave.Address, m.writeBuffer [1]);
                                    });

                                    #if DEBUG_SERIAL
                                    Console.WriteLine ("<ERROR> APB {0} timeout on function number {1}", m.slave.Address, m.writeBuffer [1]);
                                    #endif
                                }

                                // check response
                                if (m.readBuffer.Length >= m.responseLength) {
                                    if (checkResponse (ref m.readBuffer)) {
                                        // response is all good
                                        #if DEBUG_SERIAL
                                        Console.WriteLine ("Received Message in main thread");
                                        Console.WriteLine ("Message length is {0}, expected {1}", m.readBuffer.Length, m.responseLength);
                                        foreach (var w in m.readBuffer) {
                                            Console.WriteLine ("{0:X}", w);
                                        }
                                        #endif

                                        if (m.callback != null) {
                                            Gtk.Application.Invoke ((sender, e) => m.callback (new CallbackArgs (m.readBuffer)));
                                        }

                                        if (Alarm.CheckAlarming (m.slave.alarmIdx)) {
                                            Alarm.Clear (m.slave.alarmIdx);
                                        }

                                        m.slave.UpdateStatus (AquaPicBusStatus.CommunicationSuccess, (int)stopwatch.ElapsedMilliseconds);
                                        break;
                                    } else {
                                        m.slave.UpdateStatus (AquaPicBusStatus.CrcError, readTimeout);
                                        Gtk.Application.Invoke ((sender, e) => {
                                            Logger.AddWarning ("APB {0} crc error on function number {1}", m.slave.Address, m.writeBuffer[1]);
                                        });

                                        #if DEBUG_SERIAL
                                        Console.WriteLine ("<ERROR> APB {0} crc error on function number {1}", m.slave.Address, m.writeBuffer [1]);
                                        #endif
                                    }
                                } else {
                                    m.slave.UpdateStatus (AquaPicBusStatus.LengthError, readTimeout);
                                    Gtk.Application.Invoke ((sender, e) => {
                                        Logger.AddWarning ("APB {0} response length error on function number {1}", m.slave.Address, m.writeBuffer[1]);
                                    });

                                    #if DEBUG_SERIAL
                                    Console.WriteLine ("<ERROR> APB {0} response length error on function number {1}", m.slave.Address, m.writeBuffer [1]);
                                    #endif
                                }

                                lock (receiveBuffer.SyncLock) {
                                    receiveBuffer.waitForResponse = false;
                                }

                                // Thread.Sleep (100);
                            }

                            //all retry attempts have failed, post alarm
                            if ((m.slave.Status == AquaPicBusStatus.CrcError) || (m.slave.Status == AquaPicBusStatus.LengthError) || (m.slave.Status == AquaPicBusStatus.Timeout)) {
                                Gtk.Application.Invoke ((sender, e) => {
                                    Alarm.Post (m.slave.alarmIdx);
                                });
                            }
                        } catch (Exception ex) {
                            m.slave.UpdateStatus (AquaPicBusStatus.Exception, 1000);
                            Gtk.Application.Invoke ((sender, e) => {
                                Logger.AddError (ex.ToString ());
                                Alarm.Post (m.slave.alarmIdx);
                            });
                        }
                    } else {
                        m.slave.UpdateStatus (AquaPicBusStatus.NotOpen, 0);
                    }

                    #if DEBUG_SERIAL
                    Console.WriteLine ("******************End Message******************");
                    #endif
                }
            }
        }
           
        private static void responseTimeout () {
            bool loopResponse, responseSuccessful;

            while (enableResponse) {
                getInput.WaitOne (); // never returns until getInput.Set() is called 

                lock (receiveBuffer.SyncLock) {
                    loopResponse = receiveBuffer.waitForResponse;
                }
                responseSuccessful = false;

                while (loopResponse) {
                    int size = apbPort.uart.BytesToRead;

                    lock (receiveBuffer.SyncLock) {
                        if (size == receiveBuffer.responseLength) {
                            byte[] b = new byte[size];
                            apbPort.uart.Read (b, 0, size);
                            receiveBuffer.buffer.AddRange (b); 
                            receiveBuffer.waitForResponse = false;
                            responseSuccessful = true;

                            #if DEBUG_SERIAL
                            Console.WriteLine ("Received message in response thread");
                            #endif
                        }

                        loopResponse = receiveBuffer.waitForResponse;
                    }
                }

                if (responseSuccessful)
                    gotInput.Set (); // sets gotInput Event if response
            }
        }

        private static void getResponse (ref byte[] response) {
            lock (receiveBuffer.SyncLock) {
                receiveBuffer.waitForResponse = true;
            }
            getInput.Set ();
            // waits readTimeout for respone and returns true if gotInput.Set() was called or false if not
            bool success = gotInput.WaitOne (readTimeout);
            stopwatch.Stop (); // stops stopwatch to determine latency of slave device
            if (success) {
                #if DEBUG_SERIAL
                Console.WriteLine ("Received message successfully");
                #endif
                lock (receiveBuffer.SyncLock) {
                    #if DEBUG_SERIAL
                    if (receiveBuffer.buffer.Count != receiveBuffer.responseLength) {
                        Console.WriteLine ("Incorrect response success");
                    }
                    #endif
                    response = receiveBuffer.buffer.ToArray ();
                }
            } else {
                #if DEBUG_SERIAL
                Console.WriteLine ("Throwing timeout exception");
                #endif
                throw new TimeoutException ("ApuaPicBus response timeout");
            }
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

