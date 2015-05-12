using System;
using Gtk; // for Application.Invoke

namespace AquaPic.SerialBus
{
    public partial class AquaPicBus
    {
        public class Slave
        {
            public event StatusUpdateHandler OnStatusUpdate;

            private AquaPicBus bus;
            private byte address;
            private int responeTime;
            private int[] timeQue;
            private int queIdx;
            private AquaPicBusStatus status;

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

            public Slave (AquaPicBus bus, byte address) {
                if (!bus.IsAddressOk (address))
                    throw new Exception ("Address already in use");

                this.bus = bus;
                this.address = address;
                this.responeTime = 0;
                this.timeQue = new int[10];
                this.queIdx = 0;
                this.status = AquaPicBusStatus.notOpen;
                this.Name = null;

                this.bus.slaves.Add (this);
            }

            public Slave (AquaPicBus bus, byte address, string name ) : this(bus, address) {
                this.Name = name;
            }

            #if SIMULATION
            public void Read (int func, int readSize, ResponseCallback callback) {
                bus.queueMessage (this, func, null, 0, readSize, callback);
            }

            public void Write (int func, string[] writeMessage, int writeSize) {
                bus.queueMessage (this, func, writeMessage, writeSize, 0, null);
            }

            public void ReadWrite (int func, string[] writeMessage, int writeSize, int readSize, ResponseCallback callback) {
                bus.queueMessage (this, func, writeMessage, writeSize, readSize, callback);
            }
            #else
            public unsafe void Read (byte func, int readSize, ResponseCallback callback) {
                bus.queueMessage (this, func, null, 0, readSize, callback);
            }

            public unsafe void Write (byte func, void* writeData, int writeSize) {
                bus.queueMessage (this, func, writeData, writeSize, 0, null);
            }

            public unsafe void ReadWrite (byte func, void* writeData, int writeSize, int readSize, ResponseCallback callback) {
                bus.queueMessage (this, func, writeData, writeSize, readSize, callback);
            }
            #endif

            public void updateStatus (AquaPicBusStatus stat, int time) {
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

