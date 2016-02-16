using System;
using AquaPic.SerialBus;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public partial class DigitalInputBase
    {
        private class DigitalInputCard<T> : GenericCard<T>
        {
            public DigitalInputCard (string name, int cardId, int address)
                : base (name, 
                    CardType.DigitalInputCard, 
                    cardId,
                    address,
                    6) { }

            protected override GenericChannel<T> ChannelCreater (int index) {
                return new DigitalInputChannel<T> (GetDefualtName (index));
            }

            public override void GetAllValuesCommunication () {
                slave.Read (20, sizeof(byte), GetInputsCallback);
            }

            protected void GetInputsCallback (CallbackArgs args) {
                byte stateMask;

                stateMask = args.GetDataFromReadBuffer<byte> (0);

                for (int i = 0; i < channelCount; ++i)
                    channels [i].SetValue (Utils.MaskToBoolean (stateMask, i));
            }

            public override void GetValueCommunication (int channel) {
                CheckChannelRange (channel);
                slave.ReadWrite (10, (byte)channel, 2, GetInputCallback);
            }

            protected void GetInputCallback (CallbackArgs args) {
                byte ch = args.GetDataFromReadBuffer<byte> (0);
                bool value = args.GetDataFromReadBuffer<bool> (1);

                channels [ch].SetValue(value);
            }
        }
    }
}

