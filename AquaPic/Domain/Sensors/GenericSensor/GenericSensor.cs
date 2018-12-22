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
using AquaPic.Consumers;

namespace AquaPic.Sensors
{
    public class GenericSensor : ValueConsumer
    {
        public event EventHandler<SensorUpdatedEventArgs> SensorUpdatedEvent;
        public event EventHandler<SensorRemovedEventArgs> SensorRemovedEvent;
        public event EventHandler<ValueChangedEventArgs> ValueChangedEvent;
        public event EventHandler<ValueUpdatedEventArgs> ValueUpdatedEvent;

        public string name { get; protected set; }
        public IndividualControl channel { get; protected set; }

        public GenericSensor (string name, IndividualControl channel) {
            this.name = name;
            this.channel = channel;
        }

        public virtual void OnCreate () => throw new NotImplementedException ();
        public virtual void OnRemove () => throw new NotImplementedException ();
        public virtual ValueType GetValue () => throw new NotImplementedException ();

        public virtual GenericSensor Clone () {
            return (GenericSensor)MemberwiseClone ();
        }

        public void Updated (string name, GenericSensorSettings settings) {
            SensorUpdatedEvent?.Invoke (this, new SensorUpdatedEventArgs (name, settings));
        }

        public void Removed () {
            SensorRemovedEvent?.Invoke (this, new SensorRemovedEventArgs (name));
        }

        public void ValueChanged (ValueType newValue, ValueType oldValue) {
            ValueChangedEvent?.Invoke (Clone (), new ValueChangedEventArgs (newValue, oldValue));
        }

        public void ValueUpdated (ValueType value) {
            ValueUpdatedEvent?.Invoke (Clone (), new ValueUpdatedEventArgs (value));
        }
    }
}
