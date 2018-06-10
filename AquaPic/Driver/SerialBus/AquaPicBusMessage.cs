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

                this.writeBuffer[0] = slave.address;
                this.writeBuffer[1] = func;
                this.writeBuffer[2] = (byte)this.writeBuffer.Length;

                if (writeBuffer != null) {
                    Array.Copy (writeBuffer, 0, this.writeBuffer, 3, writeSize);
                }

                crc16 (ref this.writeBuffer, ref crc);
                this.writeBuffer[this.writeBuffer.Length - 2] = crc[0];
                this.writeBuffer[this.writeBuffer.Length - 1] = crc[1];
            }
        }
    }
}

