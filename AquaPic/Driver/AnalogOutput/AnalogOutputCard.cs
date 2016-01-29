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
                slave = new AquaPicBus.Slave (address, name + " (Analog Output)");
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
                SetChannelType (ch, type);
            }

            public void SetChannelType (int ch, AnalogType type) {
                if (type != AnalogType.ZeroTen)
                    throw new Exception ("Dimming card only does 0-10V");

                channels [ch].type = type;

                #if MULTI_TYPE_AO_CARD
                byte[] arr = new byte[2];
                arr [0] = (byte)ch;
                //<NOTE> CHANGE PWM to be 255 and 0-10 to be 0
                arr [1] = (byte)channels [ch].type;

                #if UNSAFE_COMMS
                unsafe {
                    fixed (byte* ptr = &arr [0]) {
                        slave.Write (2, ptr, sizeof(byte) * 2);
                    }
                }
                #else
                slave.Write (2, arr);
                #endif //UNSAFE_COMMS
                #endif //MULTI_TYPE_AO_CARD
            }

            public void SetAnalogValue (int channelId, int value) {
                channels [channelId].value = value;

                #if UNSAFE_COMMS
                CommValueInt vs;
                vs.channel = (byte)channelId;
                vs.value = (short)value;

                unsafe {
                    slave.Write (31, &vs, sizeof(CommValueInt));
                }
                #else
                WriteBuffer buf = new WriteBuffer ();
                buf.Add ((byte)channelId, sizeof(byte));
                buf.Add ((short)value, sizeof(short));

                slave.Write (31, buf);
                #endif
            }

            public void SetAllAnalogValues (short[] values) {
                if (values.Length < 4)
                    return;

                for (int i = 0; i < 4; ++i) {
                    channels [i].value = values [i];
                }

                #if UNSAFE_COMMS
                unsafe {
                    fixed (short* ptr = &values[0]) {
                        slave.Write (30, ptr, sizeof(short) * 4);
                    }
                }
                #else
                WriteBuffer buf = new WriteBuffer ();
                foreach (var val in values) {
                    buf.Add (val, sizeof(short));
                }
                slave.Write (30, buf);
                #endif
            }

            public void GetValues () {
                #if UNSAFE_COMMS
                unsafe {
                    slave.Read (20, sizeof(short) * 4, GetValuesCallback); 
                }
                #else
                slave.Read (20, sizeof(short) * 4, GetValuesCallback);
                #endif
            }

            protected void GetValuesCallback (CallbackArgs args) {
                short[] values = new short[4];

                #if UNSAFE_COMMS
                unsafe {
                    fixed (short* ptr = values) {
                        args.CopyBuffer (ptr, sizeof(short) * 4);
                    }
                }
                #else
                for (int i = 0; i < values.Length; ++i) {
                    values [i] = args.GetDataFromReadBuffer<short> (i * 2);
                }
                #endif

                for (int i = 0; i < channels.Length; ++i) {
                    channels [i].value = values [i];
                }
            }

            public void GetValue (byte ch) {
                #if UNSAFE_COMMS
                byte message = ch;

                unsafe {
                    slave.ReadWrite (10, &message, sizeof(byte), sizeof(CommValueInt), GetValueCallback);
                }
                #else
                slave.ReadWrite (10, ch, 3, GetValueCallback); // byte channel id and int16 value, 3 bytes
                #endif
            }

            protected void GetValueCallback (CallbackArgs args) {
                #if UNSAFE_COMMS
                CommValueInt vg;

                unsafe {
                    args.CopyBuffer (&vg, sizeof(CommValueInt));
                }

                channels [vg.channel].value = vg.value;
                #else
                byte ch = args.GetDataFromReadBuffer<byte> (0);
                short value = args.GetDataFromReadBuffer<short> (1);
                channels [ch].value = value;
                #endif
            }
        }
    }
}

