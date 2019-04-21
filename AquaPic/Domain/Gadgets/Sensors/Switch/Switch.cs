#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2019 Goodtime Development

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
using AquaPic.Drivers;
using AquaPic.Service;

namespace AquaPic.Gadgets.Sensor.Switch
{
    public class Switch : GenericSensor
    {
        public bool activated { get; protected set; }

        public Switch (SwitchSettings settings) : base (settings) {
            activated = false;
            var channelName = string.Format ("{0}, Switch", name);
            Driver.DigitalInput.AddChannel (channel, channelName);
            Subscribe (Driver.DigitalInput.GetChannelEventPublisherKey (channelName));
            Bit.Instance.Set (name, activated);
        }

        public override void Dispose () {
            Driver.DigitalInput.RemoveChannel (channel);
            Unsubscribe ();
        }

        protected override ValueType GetValue () {
            return activated;
        }

        public override void OnValueChangedAction (object parm) {
            var args = parm as ValueChangedEvent;
            activated = Convert.ToBoolean (args.newValue);
            Bit.Instance.Set (name, activated);
        }
    }
}
