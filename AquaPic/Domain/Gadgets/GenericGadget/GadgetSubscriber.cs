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
using AquaPic.Service;

namespace AquaPic.Gadgets
{
    public class GadgetSubscriber : ValueSubscriber
    {
        public Guid gadgetUpdatedGuid { get; protected set; }
        public Guid gadgetRemovedGuid { get; protected set; }

        public virtual void OnGadgetUpdatedAction (object parm) => throw new NotImplementedException ();
        public virtual void OnGadgetRemovedAction (object parm) => throw new NotImplementedException ();

        public override void Subscribe (Guid key) {
            subscriptionKey = key;
            var consumerType = GetType ();
            var messageHub = MessageHub.Instance;

            base.Subscribe (subscriptionKey);
            var methodInfo = consumerType.GetMethod (nameof (OnGadgetUpdatedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                gadgetUpdatedGuid = messageHub.Subscribe<GadgetUpdatedEvent> (subscriptionKey, OnGadgetUpdatedAction);
            }

            methodInfo = consumerType.GetMethod (nameof (OnGadgetRemovedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                gadgetRemovedGuid = messageHub.Subscribe<GadgetRemovedEvent> (subscriptionKey, OnGadgetRemovedAction);
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

            if (gadgetUpdatedGuid != Guid.Empty) {
                messageHub.Unsubscribe (subscriptionKey, gadgetUpdatedGuid);
            }

            if (gadgetRemovedGuid != Guid.Empty) {
                messageHub.Unsubscribe (subscriptionKey, gadgetUpdatedGuid);
            }

            subscriptionKey = Guid.Empty;
        }
    }
}
