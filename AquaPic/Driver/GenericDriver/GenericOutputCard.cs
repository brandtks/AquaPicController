#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2019 Goodtime Development

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

namespace AquaPic.Drivers
{
    public class GenericOutputCard : GenericCard
    {
        public GenericOutputCard (string name, int address, int numberChannels)
            : base (name, address, numberChannels) {
            foreach (var channel in channels) {
                var sub = new OutputChannelValueSubscriber (OnValueChanged);
                sub.Subscribe (channel.key);
            }
        }

        protected virtual void OnValueChanged (string name, ValueType value) {
            var index = GetChannelIndex (name);
            SetValueCommunication (index, value);
        }

        public void SubscribeChannel (int channel, Guid key) {
            CheckChannelRange (channel);
            var outputChannel = channels[channel] as GenericOutputChannel;
            outputChannel.Subscribe (key);
        }

        public void UnsubscribeChannel (int channel) {
            CheckChannelRange (channel);
            var outputChannel = channels[channel] as GenericOutputChannel;
            outputChannel.Unsubscribe ();
        }

        public Guid GetSubscriptionKey (int channel) {
            CheckChannelRange (channel);
            var outputChannel = channels[channel] as GenericOutputChannel;
            return outputChannel.subscriptionKey;
        }

        public Guid[] GetAllSubscriptionKeys () {
            Guid[] subscriptionKeys = new Guid[channelCount];
            for (int i = 0; i < channelCount; ++i) {
                var outputChannel = channels[i] as GenericOutputChannel;
                subscriptionKeys[i] = outputChannel.subscriptionKey;
            }
            return subscriptionKeys;
        }
    }
}
