using System;

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

            #if UNSAFE_COMMS
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
            #endif

            public InternalMessage (Slave slave, byte func, byte[] writeBuffer, int writeSize, int readSize, ResponseCallback callback) {
                byte[] crc = new byte[2];

                this.slave = slave;
                if (writeBuffer != null) {
                    this.writeBuffer = new byte[5 + writeSize];
                } else {
                    this.writeBuffer = new byte[5];
                }
                this.responseLength = 5 + readSize;
                this.readBuffer = new byte[responseLength];
                this.callback = callback;

                this.writeBuffer [0] = slave.Address;
                this.writeBuffer [1] = func;
                this.writeBuffer [2] = (byte)this.writeBuffer.Length;

                if (writeBuffer != null) {
                    Array.Copy (writeBuffer, 0, this.writeBuffer, 3, writeSize);
                }

                crc16 (ref this.writeBuffer, ref crc);
                this.writeBuffer [this.writeBuffer.Length - 2] = crc [0];
                this.writeBuffer [this.writeBuffer.Length - 1] = crc [1];
            }
        }
    }
}

