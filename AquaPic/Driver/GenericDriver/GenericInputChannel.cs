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
using AquaPic.Consumers;

namespace AquaPic.Drivers
{
    public class GenericInputChannel : GenericChannel
    {
        public event EventHandler<ValueChangedEventArgs> InputChannelValueChangedEvent;
        public event EventHandler<ValueUpdatedEventArgs> InputChannelValueUpdatedEvent;

        public GenericInputChannel (string name, Type valueType) : base (name, valueType) { }

        public void OnValueChanged (ValueType newValue, ValueType oldValue) {
            InputChannelValueChangedEvent?.Invoke (this, new ValueChangedEventArgs (newValue, oldValue));
        }

        public void OnValueUpdated (ValueType newValue) {
            InputChannelValueUpdatedEvent?.Invoke (this, new ValueUpdatedEventArgs (newValue));
        }
    }
}
