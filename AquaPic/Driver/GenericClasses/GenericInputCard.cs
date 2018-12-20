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
    public class GenericInputCard : GenericCard
    {
        public GenericInputCard (string name, int address, int numberChannels)
            : base (name, address, numberChannels) { }

        public void AddHandlerOnInputChannelValueChangedEvent (int channel, EventHandler<InputChannelValueChangedEventArgs> handler) {
            CheckChannelRange (channel);
            var inputChannel = channels[channel] as GenericInputChannel;
            inputChannel.InputChannelValueChangedEvent += handler;
        }

        public void AddHandlerOnInputChannelValueUpdatedEvent (int channel, EventHandler<InputChannelValueUpdatedEventArgs> handler) {
            CheckChannelRange (channel);
            var inputChannel = channels[channel] as GenericInputChannel;
            inputChannel.InputChannelValueUpdatedEvent += handler;
        }

        public void RemoveHandlerOnInputChannelValueChangedEvent (int channel, EventHandler<InputChannelValueChangedEventArgs> handler) {
            CheckChannelRange (channel);
            var inputChannel = channels[channel] as GenericInputChannel;
            inputChannel.InputChannelValueChangedEvent -= handler;
        }

        public void RemoveHandlerOnInputChannelValueUpdatedEvent (int channel, EventHandler<InputChannelValueUpdatedEventArgs> handler) {
            CheckChannelRange (channel);
            var inputChannel = channels[channel] as GenericInputChannel;
            inputChannel.InputChannelValueUpdatedEvent -= handler;
        }

        protected virtual void UpdateChannelValue (GenericChannel channel, ValueType value) {
            if (channel.mode == Mode.Auto) {
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

        protected virtual void UpdateAllChannelValues (ValueType[] values) {
            if (values.Length < channels.Length) {
                throw new ArgumentOutOfRangeException (nameof (values));
            }

            for (int i = 0; i < channels.Length; ++i) {
                var inputChannel = channels[i] as GenericInputChannel;
                UpdateChannelValue (inputChannel, values[i]);
            }
        }
    }
}
