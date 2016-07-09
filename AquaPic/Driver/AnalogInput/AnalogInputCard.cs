using System;
using AquaPic.Utilites;
using AquaPic.SerialBus;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
    public partial class AnalogInputBase
    {
        protected class AnalogInputCard<T> : GenericCard<T>
        {
            public AnalogInputCard (string name, int cardId, int address)
                : base (name, 
                    CardType.AnalogInputCard, 
                    cardId,
                    address,
                    4) { }

            protected override GenericChannel<T> ChannelCreater (int index) {
                return new AnalogInputChannel<T> (GetDefualtName (index));
            }

            public override void GetValueCommunication (int channel) {
                CheckChannelRange (channel);

                if (channels [channel].mode == Mode.Auto) {
                    slave.ReadWrite (10, (byte)channel, 3, GetValueCommunicationCallback);
                }
            }

            protected void GetValueCommunicationCallback (CallbackArgs args) {
                byte ch = args.GetDataFromReadBuffer<byte> (0);
                short value = args.GetDataFromReadBuffer<short> (1);
                channels [ch].SetValue (value);
            }

            public override void GetAllValuesCommunication () {
                slave.Read (20, sizeof(short) * 4, GetAllValuesCommunicationCallback);
            }

            protected void GetAllValuesCommunicationCallback (CallbackArgs args) {
                short[] values = new short[4];

                for (int i = 0; i < values.Length; ++i) {
                    values [i] = args.GetDataFromReadBuffer<short> (i * 2);
                }

                for (int i = 0; i < channels.Length; ++i) {
                    if (channels [i].mode == Mode.Auto)
                        channels [i].SetValue (values [i]);
                }
            }

            public override void SetChannelValue (int channel, T value) {
                CheckChannelRange (channel);

                if (channels [channel].mode == Mode.Manual) {
                    channels [channel].SetValue (value);
                } else {
                    throw new Exception ("Can only modify analong input value with channel forced");
                }
            }

            public override void SetAllChannelValues (T[] values) {
                if (values.Length != channels.Length)
                    throw new ArgumentOutOfRangeException ("values length");

                for (int i = 0; i < channels.Length; ++i) {
                    if (channels [i].mode == Mode.Manual) {
                        channels [i].SetValue (values [i]);
                    }
                }
            }
        }
    }
}

