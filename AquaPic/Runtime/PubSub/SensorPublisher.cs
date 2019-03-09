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
using AquaPic.Sensors;
using AquaPic.Runtime;

namespace AquaPic.PubSub
{
    public class SensorPublisher : ValueSubscriber
    {
        public string key { get; protected set; }

        public SensorPublisher (string key) {
            this.key = key;
        }

        public void NotifyValueChanged (ValueType newValue, ValueType oldValue) {
            MessageHub.Instance.Publish (key, new ValueChangedEvent (key, newValue, oldValue));
        }

        public void NotifyValueUpdated (ValueType value) {
            MessageHub.Instance.Publish (key, new ValueUpdatedEvent (key, value));
        }

        public void NotifySensorUpdated (string name, GenericSensorSettings settings) {
            // If the sensor is renamed, the local key variable is the new name (settings.name)
            // But the Consumers are subscribed to the MessageHub at the old name
            if (name != settings.name) {
                MessageHub.Instance.ChangeKey (name, settings.name);
            }
            MessageHub.Instance.Publish (key, new SensorUpdatedEvent (name, settings));
        }

        public void NotifySensorRemoved (string name) {
            if (name != key) {
                Logger.AddError ("Trying to publish remove event on key: {0}, with name {1}", key, name);
            }
            MessageHub.Instance.Publish (key, new SensorRemovedEvent (name));
        }
    }
}
