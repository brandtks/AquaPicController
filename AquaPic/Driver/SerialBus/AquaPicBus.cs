#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.Threading;             // for Thread, AutoResetEvent
using System.Diagnostics;           // for Stopwatch
using System.Collections;           // for Queue
using System.Collections.Generic;   // for List
using System.IO.Ports;              // for SerialPort
using Gtk;                          // for Application.Invoke
using AquaPic.Runtime;              // for Logger, Alarm

namespace AquaPic.SerialBus
{
    public partial class AquaPicBus
    {
        static SerialPort uart;
        static Queue messageBuffer;
        static Thread txRxThread;
        static Thread responseThread;
        static bool enableTxRx;
        static bool enableResponse;
        static AutoResetEvent getInput, gotInput;
        static Stopwatch stopwatch;
        static List<Slave> slaves;
        static ReceiveBuffer receiveBuffer;

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
                    names[i++] = s.slaveName;
                return names;
            }
        }

        public static int[] slaveAdresses {
            get {
                int[] address = new int[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    address[i++] = s.address;
                return address;
            }
        }

        public static AquaPicBusStatus[] slaveStatus {
            get {
                var status = new AquaPicBusStatus[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    status[i++] = s.status;
                return status;
            }
        }

        public static int[] slaveResponseTimes {
            get {
                int[] time = new int[slaves.Count];
                int i = 0;
                foreach (var s in slaves)
                    time[i++] = s.responeTime;
                return time;
            }
        }

        public static bool isOpen {
            get {
                if (uart != null)
                    return uart.IsOpen;

                return false;
            }
        }

        public static string portName {
            get {
                if (uart != null)
                    return uart.PortName;

                return string.Empty;
            }
        }

        static AquaPicBus () {
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
        public static void Open (string port, int baudRate = 57600) {
            try {
                uart = new SerialPort (port, baudRate, Parity.None, 8, StopBits.One);
                uart.Handshake = Handshake.None;
                uart.Open ();

                if (uart.IsOpen) {
                    for (int i = 0; i < slaves.Count; ++i)
                        slaves[i].UpdateStatus (AquaPicBusStatus.Open, 0);
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

        public static bool SlaveAddressOk (int address) {
            var compareAddress = (byte)address;
            for (int i = 0; i < slaves.Count; ++i) {
                var slaveAddress = slaves[i].address;
                if (slaveAddress == compareAddress)
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
            bool queueDuringPortClosed) {
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

                    if (uart.IsOpen) {
                        m.slave.UpdateStatus (AquaPicBusStatus.CommunicationStart, 0);

                        try {
                            for (int i = 0; i < retryCount; ++i) {
                                uart.DiscardOutBuffer ();
                                uart.DiscardInBuffer ();

#if DEBUG_SERIAL
                                Console.WriteLine ("Sent Message");
                                foreach (var w in m.writeBuffer) {
                                    Console.WriteLine ("{0:X}", w);
                                }
#endif

                                //Write message
                                stopwatch.Stop ();
                                int framingDelay = 12 - (int)stopwatch.ElapsedMilliseconds;
                                if (framingDelay > 0) {
                                    Thread.Sleep (framingDelay);
                                }
                                uart.Write (m.writeBuffer, 0, m.writeBuffer.Length);

                                // wait for response
                                stopwatch.Restart (); // resets stopwatch for response time, getResponse(ref byte[]) stops it

                                lock (receiveBuffer.SyncLock) {
                                    receiveBuffer.responseLength = m.responseLength;
                                    receiveBuffer.buffer.Clear ();
                                    receiveBuffer.waitForResponse = true;
                                }

                                try {
                                    getResponse (ref m.readBuffer);
                                } catch (TimeoutException) {
                                    m.slave.UpdateStatus (AquaPicBusStatus.Timeout, readTimeout);
                                    Gtk.Application.Invoke ((sender, e) => {
                                        Logger.AddWarning ("APB {0} timeout on function number {1}", m.slave.address, m.writeBuffer[1]);
                                    });

#if DEBUG_SERIAL
                                    Console.WriteLine ("<ERROR> APB {0} timeout on function number {1}", m.slave.Address, m.writeBuffer [1]);
#endif
                                }

                                if (m.readBuffer.Length >= m.responseLength) { // check response
                                    if (checkResponse (ref m.readBuffer)) { // response is good
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
                                        break; // exit for loop
                                    } else { // Crc error
                                        m.slave.UpdateStatus (AquaPicBusStatus.CrcError, readTimeout);
                                        Gtk.Application.Invoke ((sender, e) => {
                                            Logger.AddWarning ("APB {0} crc error on function number {1}", m.slave.address, m.writeBuffer[1]);
                                        });

#if DEBUG_SERIAL
                                        Console.WriteLine ("<ERROR> APB {0} crc error on function number {1}", m.slave.Address, m.writeBuffer [1]);
#endif
                                    }
                                } else { // message length error
                                    m.slave.UpdateStatus (AquaPicBusStatus.LengthError, readTimeout);
                                    Gtk.Application.Invoke ((sender, e) => {
                                        Logger.AddWarning ("APB {0} response length error on function number {1}", m.slave.address, m.writeBuffer[1]);
                                    });

#if DEBUG_SERIAL
                                    Console.WriteLine ("<ERROR> APB {0} response length error on function number {1}", m.slave.Address, m.writeBuffer [1]);
#endif
                                }


                                lock (receiveBuffer.SyncLock) {
                                    receiveBuffer.waitForResponse = false;
                                }
                            }

                            //all retry attempts have failed, post alarm
                            if ((m.slave.status == AquaPicBusStatus.CrcError) || (m.slave.status == AquaPicBusStatus.LengthError) || (m.slave.status == AquaPicBusStatus.Timeout)) {
                                Gtk.Application.Invoke ((sender, e) => {
                                    Alarm.Post (m.slave.alarmIdx);
                                });
                            }
                        } catch (Exception ex) { // exception error
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

                    stopwatch.Restart ();
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
                    int size = uart.BytesToRead;

                    lock (receiveBuffer.SyncLock) {
                        if (size == receiveBuffer.responseLength) {
                            byte[] b = new byte[size];
                            uart.Read (b, 0, size);
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

        private class ReceiveBuffer
        {
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

