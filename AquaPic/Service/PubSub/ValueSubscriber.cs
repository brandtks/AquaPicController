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

namespace AquaPic.PubSub
{
    public class ValueSubscriber
    {
        public Guid subscriptionKey { get; protected set; }
        public Guid valueChangedGuid { get; protected set; }
        public Guid valueUpdatedGuid { get; protected set; }

        public virtual void OnValueChangedAction (object parm) => throw new NotImplementedException ();
        public virtual void OnValueUpdatedAction (object parm) => throw new NotImplementedException ();

        public virtual void Subscribe (Guid key) {
            subscriptionKey = key;
            var consumerType = GetType ();
            var messageHub = MessageHub.Instance;

            var methodInfo = consumerType.GetMethod (nameof (OnValueChangedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                valueChangedGuid = messageHub.Subscribe<ValueChangedEvent> (subscriptionKey, OnValueChangedAction);
            }

            methodInfo = consumerType.GetMethod (nameof (OnValueUpdatedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                valueUpdatedGuid = messageHub.Subscribe<ValueUpdatedEvent> (subscriptionKey, OnValueUpdatedAction);
            }
        }

        public virtual void Unsubscribe () {
            var messageHub = MessageHub.Instance;

            if (valueChangedGuid != Guid.Empty) {
                messageHub.Unsubscribe (subscriptionKey, valueChangedGuid);
            }

            if (valueUpdatedGuid != Guid.Empty) {
                messageHub.Unsubscribe (subscriptionKey, valueUpdatedGuid);
            }

            subscriptionKey = Guid.Empty;
        }
    }
}
