using System;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.SerialBus;

namespace AquaPic.Drivers
{
    public partial class AnalogOutputBase
    {
        protected class AnalogOutputCard<T> : GenericCard<T>
        {
            public AnalogOutputCard (string name, int cardId, int address)
                : base (name, 
                    CardType.AnalogOutputCard, 
                    cardId,
                    address,
                    4) { }

            protected override GenericChannel<T> ChannelCreater (int index) {
                return new AnalogOutputChannel<T> (GetDefualtName (index));
            }

            public override void GetValueCommunication (int channel) {
                CheckChannelRange (channel);
                slave.ReadWrite (10, (byte)channel, 3, GetValueCommunicationCallback); // byte channel id and int16 value, 3 bytes
            }

            protected void GetValueCommunicationCallback (CallbackArgs args) {
                byte ch = args.GetDataFromReadBuffer<byte> (0);
                short value = args.GetDataFromReadBuffer<short> (1);
                channels [ch].SetValue (value);
            }

            public override void SetValueCommunication<CommunicationType> (int channel, CommunicationType value) {
                CheckChannelRange (channel);

                channels [channel].SetValue (value);

                short valueToSend = Convert.ToInt16 (value);
                WriteBuffer buf = new WriteBuffer ();
                buf.Add ((byte)channel, sizeof(byte));
                buf.Add ((short)valueToSend, sizeof(short));

                slave.Write (31, buf);
            }

            public override void SetAllValuesCommunication<CommunicationType> (CommunicationType[] values) {
                if (values.Length < channelCount)
                    throw new ArgumentOutOfRangeException ("values length");

                short[] valuesToSend = new short[channelCount];
                for (int i = 0; i < 4; ++i) {
                    channels [i].SetValue (values [i]);
                    valuesToSend [i] = Convert.ToInt16 (values [i]);
                }

                WriteBuffer buf = new WriteBuffer ();
                foreach (var val in valuesToSend) {
                    buf.Add (val, sizeof(short));
                }
                slave.Write (30, buf);
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
                    channels [i].SetValue (values [i]);
                }
            }

            public void SetChannelType (int channel, AnalogType type) {
                CheckChannelRange (channel);
                #if SELECTABLE_ANALOG_OUTPUT_TYPE
                channels [ch].type = type;

                byte[] arr = new byte[2];
                arr [0] = (byte)ch;
                arr [1] = (byte)channels [ch].type;

                slave.Write (2, arr);
                #else
                if (type != AnalogType.ZeroTen)
                    throw new Exception ("Dimming card only does 0-10V");
                #endif
            }

            public AnalogType GetChannelType (int channel) {
                CheckChannelRange (channel);
                var analogOutputChannel = channels [channel] as AnalogOutputChannel<T>;
                return analogOutputChannel.type;
            }

            public AnalogType[] GetAllChannelTypes () {
                AnalogType[] types = new AnalogType[channelCount];
                for (int i = 0; i < channelCount; ++i) {
                    var analogOutputChannel = channels [i] as AnalogOutputChannel<T>;
                    types [i] = analogOutputChannel.type;
                }
                return types;
            }

            public Value GetChannelValueControl (int channel) {
                CheckChannelRange (channel);
                var analogOutputChannel = channels [channel] as AnalogOutputChannel<T>;
                return analogOutputChannel.ValueControl;
            }
        }
    }
}

