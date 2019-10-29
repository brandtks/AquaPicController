#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

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
using AquaPic.Drivers;

namespace AquaPic.Gadgets.Sensor
{
    public class FloatSwitch : GenericSensor
    {
        public bool activated { get; protected set; }
        public float physicalLevel { get; protected set; }
        public SwitchType switchType { get; protected set; }
        public SwitchFunction switchFuntion { get; protected set; }

        protected OnDelayTimer delayTimer;
        public uint timeOffset { 
            get {
                return delayTimer.timerInterval;
            }
            protected set {
                delayTimer = new OnDelayTimer (value);
            }
        }

        public FloatSwitch (FloatSwitchSettings settings) : base (settings) {
            activated = false;
            switchType = settings.switchType;
            switchFuntion = settings.switchFuntion;
            physicalLevel = settings.physicalLevel;
            delayTimer = new OnDelayTimer (settings.timeOffset);
            delayTimer.TimerElapsedEvent += OnDelayTimerTimerElapsedEvent;
            var channelName = string.Format ("{0}, Float Switch", name);
            Driver.DigitalInput.AddChannel (channel, channelName);
            Subscribe (Driver.DigitalInput.GetChannelEventPublisherKey (channelName));
        }

        public override void Dispose () {
            Driver.DigitalInput.RemoveChannel (channel);
            Unsubscribe ();
        }

        protected override ValueType GetValue () {
            return activated;
        }

        protected void OnDelayTimerTimerElapsedEvent (object sender, TimerElapsedEventArgs args) {
            // Once the timer elapses, the activation state of the float switch can be toggled.
            activated = !activated;
            NotifyValueChanged (name, activated, !activated);
        }

        public override void OnValueChangedAction (object parm) {
            var args = parm as ValueChangedEvent;
            var state = Convert.ToBoolean (args.newValue);

            if (switchType == SwitchType.NormallyClosed)
                state = !state; //normally closed switches are reversed

            delayTimer.Evaluate (activated != state); // if current state and switch activation do not match start timer
        }
    }
}

