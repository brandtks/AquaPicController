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
using AquaPic.PubSub;

namespace AquaPic.Drivers
{
    public class GenericInputCard : GenericCard
    {
        public GenericInputCard (string name, int address, int numberChannels)
            : base (name, address, numberChannels) { }
            
        public override void SetChannelValue (int channel, ValueType value) {
            CheckChannelRange (channel);

            if (channels[channel].mode == Mode.Manual) {
                UpdateChannelValue (channels[channel], value);
            } else {
                throw new Exception ("Can only modify input value with channel forced");
            }
        }

        public override void SetAllChannelValues (ValueType[] values) {
            if (values.Length < channels.Length) {
                throw new ArgumentOutOfRangeException (nameof (values));
            }

            for (int i = 0; i < channels.Length; ++i) {
                var inputChannel = channels[i] as GenericInputChannel;
                UpdateChannelValue (inputChannel, values[i]);
            }
        }

        public void SubscribeConsumer (int channel, ValueConsumer consumer) {
            CheckChannelRange (channel);
            var key = channels[channel].name;
            var consumerType = consumer.GetType ();
            var messageHub = MessageHub.Instance;

            Guid valueChangedGuid, valueUpdatedGuid;
            var methodInfo = consumerType.GetMethod (nameof (consumer.OnValueChangedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                valueChangedGuid = messageHub.Subscribe<ValueChangedEvent> (key, consumer.OnValueChangedAction);
            }

            methodInfo = consumerType.GetMethod (nameof (consumer.OnValueUpdatedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                valueUpdatedGuid = messageHub.Subscribe<ValueUpdatedEvent> (key, consumer.OnValueUpdatedAction);
            }
            consumer.SetGuids (valueChangedGuid, valueUpdatedGuid);
        }

        public void UnsubscribeConsumer (int channel, ValueConsumer consumer) {
            CheckChannelRange (channel);
            var key = channels[channel].name;
            var messageHub = MessageHub.Instance;

            if (consumer.valueChangedGuid != Guid.Empty) {
                messageHub.Unsubscribe (key, consumer.valueChangedGuid);
            }

            if (consumer.valueUpdatedGuid != Guid.Empty) {
                messageHub.Unsubscribe (key, consumer.valueUpdatedGuid);
            }
        }

        protected virtual void UpdateChannelValue (GenericChannel channel, ValueType value) {
            var oldValue = channel.value;
            channel.SetValue (value);
            var newValue = channel.value;

            var inputChannel = channel as GenericInputChannel;
            if (inputChannel != null) {
                inputChannel.OnValueUpdated (newValue);
                if (!oldValue.Equals (newValue)) {
                    inputChannel.OnValueChanged (newValue, oldValue);
                }
            }
        }
    }
}
