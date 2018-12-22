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
using AquaPic.Globals;
using AquaPic.Runtime;
using AquaPic.Drivers;
using AquaPic.Consumers;

namespace AquaPic.Sensors
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

        public FloatSwitch (
            string name,
            SwitchType switchType,
            SwitchFunction switchFuntion,
            float physicalLevel,
            IndividualControl channel,
            uint timeOffset) 
            : base (name, channel)
        {
            activated = false;
            this.switchType = switchType;
            this.switchFuntion = switchFuntion;
            this.physicalLevel = physicalLevel;
            delayTimer = new OnDelayTimer (timeOffset);
            delayTimer.TimerElapsedEvent += OnDelayTimerTimerElapsedEvent;
        }

        public override void OnCreate () {
            AquaPicDrivers.DigitalInput.AddChannel (channel, name);
            AquaPicDrivers.DigitalInput.SubscribeConsumer (channel, this);
        }

        public override void OnRemove () {
            AquaPicDrivers.DigitalInput.RemoveChannel (channel);
            AquaPicDrivers.DigitalInput.UnsubscribeConsumer (channel, this);
        }

        public override ValueType GetValue () {
            return activated;
        }

        protected void OnDelayTimerTimerElapsedEvent (object sender, TimerElapsedEventArgs args) {
            // Once the timer elapses, the activation state of the float switch can be toggled.
            activated = !activated;
        }

        public override void OnValueChangedEvent (object sender, ValueChangedEventArgs args) {
            var state = Convert.ToBoolean (args.newValue);

            if (switchType == SwitchType.NormallyClosed)
                state = !state; //normally closed switches are reversed

            delayTimer.Evaluate (activated != state); // if current state and switch activation do not match start timer
        }
    }
}

