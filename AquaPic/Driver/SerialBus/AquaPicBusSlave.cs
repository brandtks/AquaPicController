using System;
using Gtk; // for Application.Invoke

namespace AquaPic.SerialBus
{
    public partial class AquaPicBus
    {
        public class Slave
        {
            public event StatusUpdateHandler OnStatusUpdate;

            private AquaPicBus _bus;
            private byte _address;
            private int _responeTime;
            private int[] _timeQue;
            private int _queIdx;
            private AquaPicBusStatus _status;

            public AquaPicBusStatus status { 
                get { return _status; }
            }
            public byte address {
                get { return _address; }
            }
            public int responeTime {
                get { return _responeTime; }
            }
            public string name { get; set; }

            public Slave (AquaPicBus bus, byte address) {
                if (!bus.IsAddressOk (address))
                    throw new Exception ("Address already in use");

                this._bus = bus;
                this._address = address;
                this._responeTime = 0;
                this._timeQue = new int[10];
                this._queIdx = 0;
                this._status = AquaPicBusStatus.notOpen;
                this.name = null;

                this._bus.slaves.Add (this);
            }

            public Slave (AquaPicBus bus, byte address, string name ) : this(bus, address) {
                this.name = name;
            }

            public unsafe void Read (byte func, int readSize, ResponseCallback callback) {
                _bus.queueMessage (this, func, null, 0, readSize, callback);
            }

            public unsafe void Write (byte func, void* writeData, int writeSize) {
                _bus.queueMessage (this, func, writeData, writeSize, 0, null);
            }

            public unsafe void ReadWrite (byte func, void* writeData, int writeSize, int readSize, ResponseCallback callback) {
                _bus.queueMessage (this, func, writeData, writeSize, readSize, callback);
            }

            public void updateStatus (AquaPicBusStatus stat, int time) {
                if (time != 0) {
                    long sum = 0;
                    int sumCount = 0;

                    _timeQue [_queIdx] = time;
                    for (int i = 0; i < _timeQue.Length; ++i) {
                        if (_timeQue [i] != 0) {
                            sum += _timeQue [i];
                            ++sumCount;
                        }
                    }

                    _responeTime = (int)(sum / sumCount);
                    _queIdx = ++_queIdx % _timeQue.Length;
                }

                _status = stat;

                if (OnStatusUpdate != null)
                    Gtk.Application.Invoke (delegate {
                        OnStatusUpdate (this);
                    });
            }
        }
    }
}

