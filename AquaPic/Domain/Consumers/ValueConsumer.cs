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

namespace AquaPic.Consumers
{
    // This isn't abstract because we don't want to force override the methods
    public class ValueConsumer
    {
        public virtual void OnValueChangedEvent (object sender, ValueChangedEventArgs args) => throw new NotImplementedException ();
        public virtual void OnValueUpdatedEvent (object sender, ValueUpdatedEventArgs args) => throw new NotImplementedException ();
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public ValueType newValue;
        public ValueType oldValue;

        public ValueChangedEventArgs (ValueType newValue, ValueType oldValue) {
            this.newValue = newValue;
            this.oldValue = oldValue;
        }
    }

    public class ValueUpdatedEventArgs : EventArgs
    {
        public ValueType value;

        public ValueUpdatedEventArgs (ValueType value) {
            this.value = value;
        }
    }
}
