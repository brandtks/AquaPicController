using System;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.SerialBus;

namespace AquaPic.Drivers
{
    public partial class AnalogOutput
    {
        private class AnalogOutputCard
        {
            public AquaPicBus.Slave slave;
            public byte cardID;
            public AnalogOutputChannel[] channels;
            public string name;

            public AnalogOutputCard (byte address, byte cardID, string name) {
                slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address, name + " (Analog Output)");
                this.cardID = cardID;
                this.name = name;
                channels = new AnalogOutputChannel[4];
                for (int i = 0; i < channels.Length; ++i) {
                    int chId = i;
                    this.channels [chId] = new AnalogOutputChannel (
                        this.name + ".q" + i.ToString (),
                        //(float value) => SetAnalogValue ((byte)chId, value.ToInt ())
                        (float value) => channels [chId].value = value.ToInt ()
                    );
                }
            }

            public void AddChannel (int ch, AnalogType type, string name) {
                channels [ch].name = name;
                channels [ch].type = type;

                byte[] arr = new byte[2];
                arr [0] = (byte)ch;
                //<NOTE> CHANGE PWM to be 255 and 0-10 to be 0
                arr [1] = (byte)channels [ch].type;

                unsafe {
                    fixed (byte* ptr = &arr [0]) {
                        slave.Write (2, ptr, sizeof(byte) * 2);
                    }
                }

                SetChannelType (ch, type);
            }

            public void SetChannelType (int ch, AnalogType type) {
                throw new Exception ("Dimming card only does 0-10V");

                /*
                channels [ch].type = type;

                byte[] arr = new byte[2];
                arr [0] = (byte)ch;
                //<NOTE> CHANGE PWM to be 255 and 0-10 to be 0
                arr [1] = (byte)channels [ch].type;

                unsafe {
                    fixed (byte* ptr = &arr [0]) {
                        slave.Write (2, ptr, sizeof(byte) * 2);
                    }
                }
                */
            }

            public void SetAnalogValue (byte channelId, int value) {
                channels [channelId].value = value;

                CommValueInt vs;
                vs.channel = channelId;
                vs.value = (short)value;

                unsafe {
                    slave.Write (31, &vs, sizeof(CommValueInt));
                }

                //AquaPicBus.WriteBuffer buffer = new AquaPicBus.WriteBuffer ();
                //buffer.Add<byte> (channelId);
                //buffer.Add<short> ((short)value);

                //slave.Write (31, buffer); 
            }

            public void SetAllAnalogValues (short[] values) {
                if (values.Length < 4)
                    return;

                for (int i = 0; i < 4; ++i) {
                    channels [i].value = values [i];
                }

                unsafe {
                    fixed (short* ptr = &values[0]) {
                        slave.Write (30, ptr, sizeof(short) * 4);
                    }
                }
            }

            public void GetValues () {
                unsafe {
                    slave.Read (20, sizeof(short) * 4, GetValuesCallback); 
                }
            }

            protected void GetValuesCallback (CallbackArgs args) {
                short[] values = new short[4];

                unsafe {
                    fixed (short* ptr = values) {
                        args.copyBuffer (ptr, sizeof(short) * 4);
                    }
                }

                for (int i = 0; i < channels.Length; ++i) {
                    channels [i].value = values [i];
                }
            }

            public void GetValue (byte ch) {
                byte message = ch;

                unsafe {
                    slave.ReadWrite (10, &message, sizeof(byte), sizeof(CommValueInt), GetValueCallback);
                }
            }

            protected void GetValueCallback (CallbackArgs args) {
                CommValueInt vg;

                unsafe {
                    args.copyBuffer (&vg, sizeof(CommValueInt));
                }

                channels [vg.channel].value = vg.value;
            }
        }
    }
}

