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

            //private AquaPicBus bus;
            private byte address;
            private int responeTime;
            private int[] timeQue;
            private int queIdx;
            private AquaPicBusStatus status;
            private int _alarmIdx;

            public AquaPicBusStatus Status { 
                get { return status; }
            }
            public byte Address {
                get { return address; }
            }
            public int ResponeTime {
                get { return responeTime; }
            }
            public string Name { get; set; }
            public int alarmIdx {
                get { return _alarmIdx; }
            }

            //public Slave (AquaPicBus bus, byte address, string name) {
            public Slave (byte address, string name) {
                if (!IsAddressOk (address))
                    throw new Exception ("Address already in use");

                //this.bus = bus;
                this.address = address;
                this.responeTime = 0;
                this.timeQue = new int[10];
                this.queIdx = 0;
                this.status = AquaPicBusStatus.notOpen;
                this.Name = name;

                slaves.Add (this);

                _alarmIdx = Alarm.Subscribe (address.ToString () + " communication fault");
            }

            #if UNSAFE_COMMS
            public unsafe void Read (byte func, int readSize, ResponseCallback callback) {
                bus.QueueMessage (this, func, null, 0, readSize, callback);
            }
            #else
            public void Read (byte func, int readSize, ResponseCallback callback) {
                QueueMessage (this, func, null, 0, readSize, callback);
            }
            #endif

            #if UNSAFE_COMMS
            public unsafe void Write (byte func, void* writeData, int writeSize) {
                bus.queueMessage (this, func, writeData, writeSize, 0, null);
            }
            #else
            public void Write (int func, WriteBuffer writeBuffer) {
                byte[] array = writeBuffer.buffer;
                Write (func, array);
            }

            public void Write (int func, byte[] writeBuffer) {
                QueueMessage (this, (byte)func, writeBuffer, writeBuffer.Length, 0, null);
            }

            public void Write (int func, byte writeBuffer) {
                Write (func, new byte[] { writeBuffer });
            }
            #endif

            #if UNSAFE_COMMS
            public unsafe void ReadWrite (byte func, void* writeData, int writeSize, int readSize, ResponseCallback callback) {
                bus.queueMessage (this, func, writeData, writeSize, readSize, callback);
            }
            #else
            public void ReadWrite (int func, WriteBuffer writeBuffer, int readSize, ResponseCallback callback) {
                byte[] array = writeBuffer.buffer;
                ReadWrite (func, array, readSize, callback);
            }

            public void ReadWrite (int func, byte[] writeBuffer, int readSize, ResponseCallback callback) {
                QueueMessage (this, (byte)func, writeBuffer, writeBuffer.Length, readSize, callback);
            }

            public void ReadWrite (int func, byte writeBuffer, int readSize, ResponseCallback callback) {
                ReadWrite (func, new byte[] { writeBuffer }, readSize, callback);
            }
            #endif

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

                    responeTime = (int)(sum / sumCount);
                    queIdx = ++queIdx % timeQue.Length;
                }

                status = stat;

                if (OnStatusUpdate != null)
                    Gtk.Application.Invoke ((sender, e) => OnStatusUpdate (this));
            }
        }
    }
}

