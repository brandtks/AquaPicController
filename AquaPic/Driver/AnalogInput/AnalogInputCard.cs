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
using AquaPic.Globals;
using AquaPic.SerialBus;

namespace AquaPic.Drivers
{
    public partial class AnalogInputBase
    {
        protected class AnalogInputCard : GenericAnalogInputCard
        {
            public AnalogInputCard (string name, int address)
                : base (
                    name,
                    address,
                    4) { }

            protected override GenericChannel ChannelCreater (int index) {
                return new AnalogInputChannel (GetDefualtName (index));
            }

            public override string GetChannelPrefix () {
                return "i";
            }

            public override void GetValueCommunication (int channel) {
                CheckChannelRange (channel);

                if (channels[channel].mode == Mode.Auto) {
                    ReadWrite (10, (byte)channel, 3, GetValueCommunicationCallback);
                }
            }

            protected void GetValueCommunicationCallback (CallbackArgs args) {
                byte ch = args.GetDataFromReadBuffer<byte> (0);
                short value = args.GetDataFromReadBuffer<short> (1);
                if (channels[ch].mode == Mode.Auto) {
                    UpdateChannelValue (channels[ch], value);
                }
            }

            public override void GetAllValuesCommunication () {
                Read (20, sizeof (short) * 4, GetAllValuesCommunicationCallback);
            }

            protected void GetAllValuesCommunicationCallback (CallbackArgs args) {
                var values = new ValueType[4];

                for (int i = 0; i < values.Length; ++i) {
                    values[i] = args.GetDataFromReadBuffer<short> (i * 2);
                }

                for (int i = 0; i < channelCount; ++i) {
                    if (channels[i].mode == Mode.Auto) {
                        UpdateChannelValue (channels[i], values[i]);
                    }
                }
            }
        }
    }
}

