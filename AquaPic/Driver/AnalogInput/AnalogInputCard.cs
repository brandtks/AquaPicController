using System;
using AquaPic.Globals;
using AquaPic.SerialBus;
using AquaPic.AlarmRuntime;

namespace AquaPic.AnalogInputDriver
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
                this.communicationAlarmIndex = Alarm.Subscribe (address.ToString () + " communication fault", "Analog Input card at address " + this.slave.Address.ToString ());
                this.channels = new AnalogInputChannel[4];
                for (int i = 0; i < this.channels.Length; ++i) {
                    this.channels [i] = new AnalogInputChannel (); 
                }
                this.updating = false;
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((slave.Status != AquaPicBusStatus.communicationSuccess) || (slave.Status != AquaPicBusStatus.communicationStart))
                    Alarm.Post (communicationAlarmIndex);
            }

            #if SIMULATION
            public void AddChannel (int ch, AnalogType type, string name) {
                channels [ch].type = type;
                channels [ch].name = name;
            }
            #else
            public void AddChannel (int ch, AnalogType type, string name) {
                channels [ch].type = type;
                channels [ch].name = name;

                byte[] arr = new byte[2];
                arr [0] = (byte)ch;
                arr [1] = (byte)channels [ch].type;

                unsafe {
                    fixed (byte* ptr = &arr [0]) {
                        slave.Write (2, ptr, sizeof(byte) * 2);
                    }
                }
            }
            #endif

            #if SIMULATION
            public void GetValues () {
                updating = true;
                slave.Read (20, 4, GetValuesCallback); 
            }
            #else
            public unsafe void GetValues () {
                updating = true;
                slave.Read (20, sizeof(float) * 4, GetValuesCallback); 
            }
            #endif

            #if SIMULATION
            protected void GetValuesCallback (CallbackArgs args) {
                for (int i = 0; i < channels.Length; ++i) {
                    channels [i].value = Convert.ToSingle(args.readMessage[3 + i]);
                }
                updating = false;
            }
            #else
            protected void GetValuesCallback (CallbackArgs args) {
                float[] values = new float[4];

                unsafe {
                    fixed (float* ptr = values) {
                        args.copyBuffer (ptr, sizeof(float) * 4);
                    }
                }

                for (int i = 0; i < channels.Length; ++i) {
                    channels [i].value = values [i];
                }
                updating = false;
            }
            #endif

            #if SIMULATION
            public unsafe void GetValue (byte ch) {
                const int messageLength = 1;
                string[] message = new string[messageLength];
                message [0] = ch.ToString ();
                updating = true;
                slave.ReadWrite (10, message, messageLength, 2, GetValueCallback);
            }
            #else
            public unsafe void GetValue (byte ch) {
                byte message = ch;
                updating = true;
                slave.ReadWrite (10, &message, sizeof(byte), sizeof(CommValueFloat), GetValueCallback);
            }
            #endif

            #if SIMULATION
            protected void GetValueCallback (CallbackArgs args) {
                int ch = Convert.ToInt32 (args.readMessage [3]);
                channels [ch].value = Convert.ToSingle (args.readMessage [4]);
                updating = false;
            }
            #else
            protected void GetValueCallback (CallbackArgs args) {
                CommValueFloat vg;

                unsafe {
                    args.copyBuffer (&vg, sizeof(CommValueFloat));
                }

                channels [vg.channel].value = vg.value;
                updating = false;
            }
            #endif
        }
    }
}

