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

namespace AquaPic.PubSub
{
    public class SensorPublisher : ValueConsumer
    {
        public event EventHandler<SensorUpdatedEventArgs> SensorUpdatedEvent;
        public event EventHandler<SensorRemovedEventArgs> SensorRemovedEvent;
        public event EventHandler<ValueChangedEventArgs> ValueChangedEvent;
        public event EventHandler<ValueUpdatedEventArgs> ValueUpdatedEvent;

        public void NotifySensorUpdated (string name, GenericSensorSettings settings) {
            SensorUpdatedEvent?.Invoke (this, new SensorUpdatedEventArgs (name, settings));
        }

        public void NotifySensorRemoved (string name) {
            SensorRemovedEvent?.Invoke (this, new SensorRemovedEventArgs (name));
        }

        public void NotifyValueChanged (ValueType newValue, ValueType oldValue) {
            ValueChangedEvent?.Invoke (this, new ValueChangedEventArgs (newValue, oldValue));
        }

        public void NotifyValueUpdated (ValueType value) {
            ValueUpdatedEvent?.Invoke (this, new ValueUpdatedEventArgs (value));
        }

        public void SubscribeConsumer (SensorConsumer consumer) {
            var consumerType = consumer.GetType ();

            var methodInfo = consumerType.GetMethod (nameof (consumer.OnValueChangedEvent));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                ValueChangedEvent += consumer.OnValueChangedEvent;
            }

            methodInfo = consumerType.GetMethod (nameof (consumer.OnValueUpdatedEvent));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                ValueUpdatedEvent += consumer.OnValueUpdatedEvent;
            }

            methodInfo = consumerType.GetMethod (nameof (consumer.OnSensorUpdatedEvent));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                SensorUpdatedEvent += consumer.OnSensorUpdatedEvent;
            }

            methodInfo = consumerType.GetMethod (nameof (consumer.OnSensorRemovedEvent));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                SensorRemovedEvent += consumer.OnSensorRemovedEvent;
            }
        }

        public void UnsubscribeConsumer (SensorConsumer consumer) {
            ValueChangedEvent -= consumer.OnValueChangedEvent;
            ValueUpdatedEvent -= consumer.OnValueUpdatedEvent;
            SensorUpdatedEvent -= consumer.OnSensorUpdatedEvent;
            SensorRemovedEvent -= consumer.OnSensorRemovedEvent;
        }
    }
}
