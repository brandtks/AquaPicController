using System;
using AquaPic.Utilites;
using AquaPic.SerialBus;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
    public partial class AnalogInput
    {
        private class AnalogInputCard
        {
            public AquaPicBus.Slave slave;
            public byte cardID;
            public string name;
            public int communicationAlarmIndex;
            public AnalogInputChannel[] channels;
            public bool updating;

            /* <FUTURE>
            public analogInputCard (byte address, byte cardID, AnalogType[] types, string[] names) {
                this._cardID = cardID;
                this._alarmIdx = alarm.subscribe ("APB communication error", "Temperature channel at address " + this._address.ToString ());
                this.channels = new analogInputChannel[4];
                for (int i = 0; i < channels.Length; ++i) {
                    channels [i] = new analogInputChannel (types [i], names [i]); 
                }
            }
            */

            public AnalogInputCard (byte address, byte cardID, string name) {
                this.slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address);
                this.slave.OnStatusUpdate += OnSlaveStatusUpdate;

                this.cardID = cardID;
                this.name = name;
                this.communicationAlarmIndex = Alarm.Subscribe (address.ToString () + " communication fault");
                this.channels = new AnalogInputChannel[4];
                for (int i = 0; i < this.channels.Length; ++i) {
                    this.channels [i] = new AnalogInputChannel (this.name + ".i" + i.ToString ()); 
                }
                this.updating = false;
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((slave.Status != AquaPicBusStatus.communicationSuccess) || (slave.Status != AquaPicBusStatus.communicationStart))
                    Alarm.Post (communicationAlarmIndex);
            }

            public void AddChannel (int ch, string name) {
                channels [ch].name = name;

//                byte[] arr = new byte[2];
//                arr [0] = (byte)ch;
//                arr [1] = (byte)channels [ch].type;
//
//                unsafe {
//                    fixed (byte* ptr = &arr [0]) {
//                        slave.Write (2, ptr, sizeof(byte) * 2);
//                    }
//                }
            }

            public unsafe void GetValues () {
                updating = true;
                slave.Read (20, sizeof(Int16) * 4, GetValuesCallback); 
            }

            protected void GetValuesCallback (CallbackArgs args) {
                Int16[] values = new Int16[4];

                unsafe {
                    fixed (Int16* ptr = values) {
                        args.copyBuffer (ptr, sizeof(Int16) * 4);
                    }
                }

                for (int i = 0; i < channels.Length; ++i) {
                    if (channels [i].mode == Mode.Auto)
                        channels [i].value = (float)values [i];
                }

                updating = false;
            }

            public unsafe void GetValue (byte ch) {
                if (channels [ch].mode == Mode.Auto) {
                    byte message = ch;
                    updating = true;
                    slave.ReadWrite (10, &message, sizeof(byte), sizeof(CommValueInt), GetValueCallback);
                }
            }

            protected void GetValueCallback (CallbackArgs args) {
                CommValueInt vg;

                unsafe {
                    args.copyBuffer (&vg, sizeof(CommValueInt));
                }
                   
                channels [vg.channel].value = (float)vg.value;
                updating = false;
            }
        }
    }
}

