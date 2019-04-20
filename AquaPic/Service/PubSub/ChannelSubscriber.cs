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

namespace AquaPic.Service
{
    public class ChannelSubscriber : ValueSubscriber
    {
        public Guid modeChangeGuid { get; protected set; }

        public virtual void OnModeChangedAction (object parm) => throw new NotImplementedException ();

        public override void Subscribe (Guid key) {
            base.Subscribe (key);

            subscriptionKey = key;
            var consumerType = GetType ();
            var messageHub = MessageHub.Instance;

            var methodInfo = consumerType.GetMethod (nameof (OnModeChangedAction));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                modeChangeGuid = messageHub.Subscribe<ModeChangedEvent> (subscriptionKey, OnModeChangedAction);
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

            if (modeChangeGuid != Guid.Empty) {
                messageHub.Unsubscribe (subscriptionKey, modeChangeGuid);
            }

            subscriptionKey = Guid.Empty;
        }
    }
}
