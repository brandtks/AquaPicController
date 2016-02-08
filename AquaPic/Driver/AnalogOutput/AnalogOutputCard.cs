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

                slave.Write (2, arr);
                #endif //MULTI_TYPE_AO_CARD
            }

            public void SetAnalogValue (int channelId, int value) {
                channels [channelId].value = value;

                WriteBuffer buf = new WriteBuffer ();
                buf.Add ((byte)channelId, sizeof(byte));
                buf.Add ((short)value, sizeof(short));

                slave.Write (31, buf);
            }

            public void SetAllAnalogValues (short[] values) {
                if (values.Length < 4)
                    return;

                for (int i = 0; i < 4; ++i) {
                    channels [i].value = values [i];
                }
                    
                WriteBuffer buf = new WriteBuffer ();
                foreach (var val in values) {
                    buf.Add (val, sizeof(short));
                }
                slave.Write (30, buf);
            }

            public void GetValues () {
                slave.Read (20, sizeof(short) * 4, GetValuesCallback);
            }

            protected void GetValuesCallback (CallbackArgs args) {
                short[] values = new short[4];

                for (int i = 0; i < values.Length; ++i) {
                    values [i] = args.GetDataFromReadBuffer<short> (i * 2);
                }

                for (int i = 0; i < channels.Length; ++i) {
                    channels [i].value = values [i];
                }
            }

            public void GetValue (byte ch) {
                slave.ReadWrite (10, ch, 3, GetValueCallback); // byte channel id and int16 value, 3 bytes
            }

            protected void GetValueCallback (CallbackArgs args) {
                byte ch = args.GetDataFromReadBuffer<byte> (0);
                short value = args.GetDataFromReadBuffer<short> (1);
                channels [ch].value = value;
            }
        }
    }
}

