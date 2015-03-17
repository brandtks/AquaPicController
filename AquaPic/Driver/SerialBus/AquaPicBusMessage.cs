using System;

namespace AquaPic.SerialBus
{
    public partial class AquaPicBus
    {
        #if SIMULATION
        private class Message
        {
            public Slave slave;
            public string[] writeData;
            public string[] readData;
            public int responseLength;
            public ResponseCallback callback;

            public Message (Slave slave, int func, string[] writeMessage, int writeSize, int readSize, ResponseCallback callback) {
                byte[] crc = new byte[2];

                this.slave = slave;
                this.writeData = new string[5 + writeSize];
                this.responseLength = 5 + readSize;
                this.readData = new string[responseLength];
                this.callback = callback;

                this.writeData[0] = slave.Address.ToString();
                this.writeData[1] = func.ToString();
                this.writeData[2] = this.writeData.Length.ToString();

                if (writeMessage != null) {
                    for (int i = 0; i < writeMessage.Length; ++i)
                        this.writeData[3 + i] = writeMessage[i];
                }

                this.writeData[this.writeData.Length - 2] = "255";
                this.writeData[this.writeData.Length - 1] = "255";
            }
        }
        #else
        private class Message
        {
            public Slave slave;
            public byte[] writeBuffer;
            public byte[] readBuffer;
            public int responseLength;
            public ResponseCallback callback;

            public unsafe Message (Slave slave, byte func, void* writeData, int writeSize, int readSize, ResponseCallback callback) {
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
        #endif
    }
}

