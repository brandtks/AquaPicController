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
    public class SensorSubscriber : ValueSubscriber
    {
        public Guid sensorUpdatedGuid { get; protected set; }
        public Guid sensorRemovedGuid { get; protected set; }

        public virtual void OnSensorUpdatedAction (object parm) => throw new NotImplementedException ();
        public virtual void OnSensorRemovedAction (object parm) => throw new NotImplementedException ();

        public override void Subscribe (string key) {
            subscriptionKey = key;
            var consumerType = GetType ();
            var messageHub = MessageHub.Instance;

            base.Subscribe (subscriptionKey);
            var methodInfo = consumerType.GetMethod (nameof (OnSensorUpdatedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                sensorUpdatedGuid = messageHub.Subscribe<SensorUpdatedEvent> (subscriptionKey, OnSensorUpdatedAction);
            }

            methodInfo = consumerType.GetMethod (nameof (OnSensorRemovedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                sensorRemovedGuid = messageHub.Subscribe<SensorRemovedEvent> (subscriptionKey, OnSensorRemovedAction);
            }
        }

        public override void Unsubscribe () {
            var messageHub = MessageHub.Instance;

            if (valueChangedGuid != Guid.Empty) {
                messageHub.Unsubscribe (subscriptionKey, valueChangedGuid);
            }

            if (valueUpdatedGuid != Guid.Empty) {
                messageHub.Unsubscribe (subscriptionKey, valueUpdatedGuid);
            }

            if (sensorUpdatedGuid != Guid.Empty) {
                messageHub.Unsubscribe (subscriptionKey, sensorUpdatedGuid);
            }

            if (sensorRemovedGuid != Guid.Empty) {
                messageHub.Unsubscribe (subscriptionKey, sensorUpdatedGuid);
            }
        }
    }

    public class SensorUpdatedEvent
    {
        public string name;
        public GenericSensorSettings settings;

        public SensorUpdatedEvent (string name, GenericSensorSettings settings) {
            this.name = name;
            this.settings = settings;
        }
    }

    public class SensorRemovedEvent
    {
        public string name;

        public SensorRemovedEvent (string name) {
            this.name = name;
        }
    }
}
