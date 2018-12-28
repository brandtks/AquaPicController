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
    public class SensorPublisher : ValueConsumer
    {
        string key;

        public SensorPublisher (string key) {
            this.key = key;
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

        public void NotifyValueChanged (ValueType newValue, ValueType oldValue) {
            MessageHub.Instance.Publish (key, new ValueChangedEvent (key, newValue, oldValue));
        }

        public void NotifyValueUpdated (ValueType value) {
            MessageHub.Instance.Publish (key, new ValueUpdatedEvent (key, value));
        }

        public void SubscribeConsumer (SensorConsumer consumer) {
            var consumerType = consumer.GetType ();
            var messageHub = MessageHub.Instance;

            Guid valChangedGuid, valUpdatedGuid, sensorUpdatedGuid, sensorRemovedGuid;
            var methodInfo = consumerType.GetMethod (nameof (consumer.OnValueChangedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                valChangedGuid = messageHub.Subscribe<ValueChangedEvent> (key, consumer.OnValueChangedAction);
            }

            methodInfo = consumerType.GetMethod (nameof (consumer.OnValueUpdatedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                valUpdatedGuid = messageHub.Subscribe<ValueUpdatedEvent> (key, consumer.OnValueUpdatedAction);
            }

            methodInfo = consumerType.GetMethod (nameof (consumer.OnSensorUpdatedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                sensorUpdatedGuid = messageHub.Subscribe<SensorUpdatedEvent> (key, consumer.OnSensorUpdatedAction);
            }

            methodInfo = consumerType.GetMethod (nameof (consumer.OnSensorRemovedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                sensorRemovedGuid = messageHub.Subscribe<SensorRemovedEvent> (key, consumer.OnSensorRemovedAction);
            }

            consumer.SetGuids (valChangedGuid, valUpdatedGuid, sensorUpdatedGuid, sensorRemovedGuid);
        }

        public void UnsubscribeConsumer (SensorConsumer consumer) {
            var messageHub = MessageHub.Instance;

            if (consumer.valueChangedGuid != Guid.Empty) {
                messageHub.Unsubscribe (key, consumer.valueChangedGuid);
            }

            if (consumer.valueUpdatedGuid != Guid.Empty) {
                messageHub.Unsubscribe (key, consumer.valueUpdatedGuid);
            }

            if (consumer.sensorUpdatedGuid != Guid.Empty) {
                messageHub.Unsubscribe (key, consumer.sensorUpdatedGuid);
            }

            if (consumer.sensorRemovedGuid != Guid.Empty) {
                messageHub.Unsubscribe (key, consumer.sensorUpdatedGuid);
            }
        }
    }
}
