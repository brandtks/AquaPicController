// /*
// Copyright 2013 Antanas Veiverys www.veiverys.com
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// */
//

using System;
using System.IO.Ports;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.SerialBus
{
    public class MonoSerialPort : SerialPort
    {
        [DllImport ("MonoPosixHelper", SetLastError = true)]
        static extern bool poll_serial (int fd, out int error, int timeout);

        [DllImport ("libc")]
        static extern IntPtr strerror (int errnum);

        public MonoSerialPort () : 
            base() { }

        public MonoSerialPort (IContainer container) : 
                base (container) { }

        public MonoSerialPort (string portName) : 
            base(portName) { }

        public MonoSerialPort (string portName, int baudRate) : 
            base(portName, baudRate) { }

        public MonoSerialPort (string portName, int baudRate, Parity parity) : 
            base(portName, baudRate, parity) { }

        public MonoSerialPort (string portName, int baudRate, Parity parity, int dataBits) : 
            base(portName, baudRate, parity, dataBits) { }

        public MonoSerialPort (string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) :
            base(portName, baudRate, parity, dataBits,stopBits) { }

        // private member access via reflection
        int fd;
        FieldInfo disposedFieldInfo;
        object data_received;

        public new void Open () {
            base.Open();

            if (Utils.GetRunningPlatform () != Platform.Windows) {
                Logger.Add ("Linux runtime: implementing work around for AquaPicBus");

                FieldInfo fieldInfo = BaseStream.GetType().GetField("fd", BindingFlags.Instance | BindingFlags.NonPublic);
                fd = (int)fieldInfo.GetValue(BaseStream);

                disposedFieldInfo = BaseStream.GetType().GetField("disposed", BindingFlags.Instance | BindingFlags.NonPublic);

                fieldInfo = typeof(SerialPort).GetField("data_received", BindingFlags.Instance | BindingFlags.NonPublic);
                data_received = fieldInfo.GetValue(this);

                new System.Threading.Thread (new System.Threading.ThreadStart (this.EventThreadFunction)).Start ();
            }
        }

        private void EventThreadFunction ( ) {
            do {
                try {
                    var _stream = BaseStream;

                    if (_stream == null)
                        return;

                    if (Poll (_stream, ReadTimeout))
                        OnDataReceived(null);

                } catch {
                    return;
                }
            } while (IsOpen);
        }

        void OnDataReceived (SerialDataReceivedEventArgs args) {
            SerialDataReceivedEventHandler handler = (SerialDataReceivedEventHandler) Events [data_received];

            if (handler != null)
                handler (this, args);
        }

        private bool Poll (Stream stream, int timeout) {
            CheckDisposed (stream);

            if (IsOpen == false)
                throw new Exception("port is closed");

            int error;

            bool poll_result = poll_serial (fd, out error, ReadTimeout);
            if (error == -1)
                ThrowIOException ();
            
            return poll_result;
        }

        static void ThrowIOException () {
            int errnum = Marshal.GetLastWin32Error ();
            string error_message = Marshal.PtrToStringAnsi (strerror (errnum));

            throw new IOException (error_message);
        }

        void CheckDisposed (Stream stream) {
            bool disposed = (bool)disposedFieldInfo.GetValue(stream);
            if (disposed)
                throw new ObjectDisposedException (stream.GetType().FullName);
        }
    }

}

