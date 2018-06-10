#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using AquaPic.Operands;
using AquaPic.Globals;
using AquaPic.SerialBus;

namespace AquaPic.Drivers
{
    public partial class AnalogOutputBase
    {
        protected class AnalogOutputCard : GenericCard
        {
            public AnalogOutputCard (string name, int address)
                : base (
                    name,
                    address,
                    4) { }

            protected override GenericChannel ChannelCreater (int index) {
                return new AnalogOutputChannel (GetDefualtName (index));
            }

            public override void GetValueCommunication (int channel) {
                CheckChannelRange (channel);
                ReadWrite (10, (byte)channel, 3, GetValueCommunicationCallback); // byte channel id and float value, 5 bytes
            }

            protected void GetValueCommunicationCallback (CallbackArgs args) {
                var ch = args.GetDataFromReadBuffer<byte> (0);
                var value = args.GetDataFromReadBuffer<float> (1);
                channels[ch].SetValue (value);
            }

            public override void SetValueCommunication<CommunicationType> (int channel, CommunicationType value) {
                CheckChannelRange (channel);
                channels[channel].SetValue (value);
                var valueToSend = Convert.ToSingle (value);
                var buf = new WriteBuffer ();
                buf.Add ((byte)channel, sizeof (byte));
                buf.Add (valueToSend, sizeof (float));
                Write (31, buf);
            }

            public override void SetAllValuesCommunication<CommunicationType> (CommunicationType[] values) {
                if (values.Length < channelCount)
                    throw new ArgumentOutOfRangeException (nameof (values), "values length");

                var valuesToSend = new float[channelCount];
                for (int i = 0; i < 4; ++i) {
                    channels[i].SetValue (values[i]);
                    valuesToSend[i] = Convert.ToSingle (values[i]);
                }

                var buf = new WriteBuffer ();
                foreach (var val in valuesToSend) {
                    buf.Add (val, sizeof (float));
                }
                Write (30, buf);
            }

            public override void GetAllValuesCommunication () {
                Read (20, sizeof (float) * 4, GetAllValuesCommunicationCallback);
            }

            protected void GetAllValuesCommunicationCallback (CallbackArgs args) {
                var values = new float[4];

                for (int i = 0; i < values.Length; ++i) {
                    values[i] = args.GetDataFromReadBuffer<float> (i * 4);
                }

                for (int i = 0; i < channels.Length; ++i) {
                    channels[i].SetValue (values[i]);
                }
            }

            public void SetChannelType (int channel, AnalogType type) {
                CheckChannelRange (channel);
                var analogOutputChannel = channels[channel] as AnalogOutputChannel;
                analogOutputChannel.type = type;

                var message = new byte[2];
                message[0] = (byte)channel;
                message[1] = (byte)analogOutputChannel.type;

                Write (2, message);
            }

            public AnalogType GetChannelType (int channel) {
                CheckChannelRange (channel);
                var analogOutputChannel = channels[channel] as AnalogOutputChannel;
                return analogOutputChannel.type;
            }

            public AnalogType[] GetAllChannelTypes () {
                AnalogType[] types = new AnalogType[channelCount];
                for (int i = 0; i < channelCount; ++i) {
                    var analogOutputChannel = channels[i] as AnalogOutputChannel;
                    types[i] = analogOutputChannel.type;
                }
                return types;
            }

            public Value GetChannelValueControl (int channel) {
                CheckChannelRange (channel);
                var analogOutputChannel = channels[channel] as AnalogOutputChannel;
                return analogOutputChannel.valueControl;
            }
        }
    }
}

