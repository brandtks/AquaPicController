#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2018 Goodtime Development

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

namespace AquaPic.Drivers
{
    public class GenericAnalogInputCard : GenericInputCard
    {
        public GenericAnalogInputCard (string name, int address, int numberChannels)
            : base (name, address, numberChannels) { }

        public int GetChannelLowPassFilterFactor (int channel) {
            CheckChannelRange (channel);
            var analogInputChannel = channels[channel] as GenericAnalogInputChannel;
            return analogInputChannel.lowPassFilterFactor;
        }

        public int[] GetAllChannelLowPassFilterFactors () {
            int[] lowPassFilterFactors = new int[channelCount];
            for (int i = 0; i < channelCount; ++i) {
                var analogInputCard = channels[i] as GenericAnalogInputChannel;
                lowPassFilterFactors[i] = analogInputCard.lowPassFilterFactor;
            }
            return lowPassFilterFactors;
        }

        public virtual void SetupChannelCommunication (int channel) {
            CheckChannelRange (channel);

            var analogInputChannel = channels[channel] as GenericAnalogInputChannel;

            var message = new byte[2];
            message[0] = (byte)channel;
            message[1] = (byte)analogInputChannel.lowPassFilterFactor;

            Write (2, message);
        }
    }
}
