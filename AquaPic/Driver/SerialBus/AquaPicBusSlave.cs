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
using Gtk; // for Application.Invoke
using AquaPic.Runtime;

namespace AquaPic.SerialBus
{
    public partial class AquaPicBus
    {
        public class Slave
        {
            public event StatusUpdateHandler OnStatusUpdate;

			string _name;
			public string slaveName {
				get {
					return _name;
				}
			}

			byte _address;
			public byte address {
                get { return _address; }
            }

			int _responeTime;
			public int responeTime {
                get { return _responeTime; }
            }

			AquaPicBusStatus _status;
            public AquaPicBusStatus status {
                get { return _status; }
            }

            int _alarmIdx;
            public int alarmIdx {
                get { return _alarmIdx; }
            }
            
            int[] timeQue;
            int queIdx;

            public Slave (int address, string name) {
				if (!SlaveAddressOk ((byte)address))
                    throw new Exception ("Address already in use");
                
                _address = (byte)address;
                _responeTime = 0;
                timeQue = new int[10];
                queIdx = 0;
                _status = AquaPicBusStatus.NotOpen;
                _name = name;

                slaves.Add (this);

                _alarmIdx = Alarm.Subscribe (address.ToString () + " communication fault");
            }

			public void RemoveSlave () {
				slaves.Remove (this);
            }
         
            public void Read (byte func, int readSize, ResponseCallback callback, bool queueDuringPortClosed = false) {
                QueueMessage (this, func, null, 0, readSize, callback, queueDuringPortClosed);
            }

            public void Write (int func, WriteBuffer writeBuffer, bool queueDuringPortClosed = false) {
                byte[] array = writeBuffer.buffer;
                Write (func, array, queueDuringPortClosed);
            }

            public void Write (int func, byte[] writeBuffer, bool queueDuringPortClosed = false) {
                QueueMessage (this, (byte)func, writeBuffer, writeBuffer.Length, 0, null, queueDuringPortClosed);
            }

            public void Write (int func, byte writeBuffer, bool queueDuringPortClosed = false) {
                Write (func, new byte[] { writeBuffer }, queueDuringPortClosed);
            }

            public void ReadWrite (int func, WriteBuffer writeBuffer, int readSize, ResponseCallback callback, bool queueDuringPortClosed = false) {
                byte[] array = writeBuffer.buffer;
                ReadWrite (func, array, readSize, callback, queueDuringPortClosed);
            }

            public void ReadWrite (int func, byte[] writeBuffer, int readSize, ResponseCallback callback, bool queueDuringPortClosed = false) {
                QueueMessage (this, (byte)func, writeBuffer, writeBuffer.Length, readSize, callback, queueDuringPortClosed);
            }

            public void ReadWrite (int func, byte writeBuffer, int readSize, ResponseCallback callback, bool queueDuringPortClosed = false) {
                ReadWrite (func, new byte[] { writeBuffer }, readSize, callback, queueDuringPortClosed);
            }

            public void UpdateStatus (AquaPicBusStatus stat, int time) {
                if (time != 0) {
                    long sum = 0;
                    int sumCount = 0;

                    timeQue [queIdx] = time;
                    for (int i = 0; i < timeQue.Length; ++i) {
                        if (timeQue [i] != 0) {
                            sum += timeQue [i];
                            ++sumCount;
                        }
                    }

                    _responeTime = (int)(sum / sumCount);
                    queIdx = ++queIdx % timeQue.Length;
                }

                _status = stat;

                if (OnStatusUpdate != null)
                    Application.Invoke ((sender, e) => OnStatusUpdate (this));
            }
        }
    }
}

