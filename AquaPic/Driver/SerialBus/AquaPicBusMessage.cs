using System;
using System.Collections.Generic;

namespace AquaPic.SerialBus
{
    public partial class AquaPicBus
    {
        private class InternalMessage
        {
            public Slave slave;
            public byte[] writeBuffer;
            public byte[] readBuffer;
            public int responseLength;
            public ResponseCallback callback;

            public unsafe InternalMessage (Slave slave, byte func, void* writeData, int writeSize, int readSize, ResponseCallback callback) {
                byte[] crc = new byte[2];

                this.slave = slave;
                this.writeBuffer = new byte[5 + writeSize];
                this.responseLength = 5 + readSize;
                this.readBuffer = new byte[responseLength];
                this.callback = callback;

                this.writeBuffer [0] = slave.Address;
                this.writeBuffer [1] = func;
                this.writeBuffer [2] = (byte)this.writeBuffer.Length;

                if (writeData != null) {
                    byte* data = (byte*)writeData;
                    for (int i = 0; i < writeSize; ++i)
                        this.writeBuffer [3 + i] = *data++;
                }

                crc16 (ref writeBuffer, ref crc);
                this.writeBuffer [this.writeBuffer.Length - 2] = crc [0];
                this.writeBuffer [this.writeBuffer.Length - 1] = crc [1];
            }
        }

        /*
        public class WriteBuffer
        {
            private List<byte> _buffer;

            public byte[] buffer {
                get {
                    return _buffer.ToArray ();
                }
            }

            public int size {
                get {
                    return _buffer.Count;
                }
            }

            public WriteBuffer () {
                _buffer = new List<byte> ();
            }

            public void Add<T> (T data) {
                int size = sizeof(T);
                byte[] d = new byte[size];
                unsafe {
                    fixed (byte* ptr = d) {
                        Utilities.MemoryCopy (&data, ptr, size);
                    }
                }

                _buffer.AddRange (d);
            }
        }
        */
    }
}

