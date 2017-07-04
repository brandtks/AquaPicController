#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using AquaPic.Utilites;
using AquaPic.Runtime;
using AquaPic.Drivers;

namespace AquaPic.Sensors
{
    public enum SwitchType {
        [Description("Normally Opened")]
        NormallyOpened,
        [Description("Normally Closed")]
        NormallyClosed
    }

    public enum SwitchFunction {
        LowLevel,
        HighLevel,
        ATO,
        Other
    }

    public class FloatSwitch : ISensor<bool>
    {
        protected string _name;
        public string name {
            get {
                return _name;
            }
        }

        protected OnDelayTimer _onDelayTimer;
        public OnDelayTimer onDelayTimer {
            get {
                return _onDelayTimer;
            }
        }

        protected IndividualControl _channel;
        public IndividualControl channel {
            get {
                return _channel;
            }
        }

        protected bool _activated;
        public bool activated {
            get {
                return _activated;
            }
        }

        protected SwitchType _type;
        public SwitchType type {
            get {
                return _type;
            }
            set {
                if (value != _type) { // when switching type activation reverses
                    _activated = !_activated;
                }
                _type = value;
            }
        }

        public SwitchFunction function;
        public float physicalLevel;
        public string waterLevelGroupName;

        public FloatSwitch (
            string name,
            SwitchType type,
            SwitchFunction function,
            float physicalLevel,
            IndividualControl channel,
            uint timeOffset
        ) {
            _activated = false;
            _name = name;
            _type = type;
            this.function = function;
            this.physicalLevel = physicalLevel;
            _channel = channel;
            _onDelayTimer = new OnDelayTimer (timeOffset);

            if (_channel.IsNotEmpty ()) {
                Add (channel);
            }
        }

        public void Add (IndividualControl channel) {
            if (!_channel.Equals (channel))
                Remove ();

            _channel = channel;

            if (_channel.IsNotEmpty ()) {
                AquaPicDrivers.DigitalInput.AddChannel (_channel, _name);
            }
        }

        public void Remove () {
            if (_channel.IsNotEmpty ()) {
                AquaPicDrivers.DigitalInput.RemoveChannel (_channel);
            }
        }

        public bool Get () {
            var state = AquaPicDrivers.DigitalInput.GetChannelValue (_channel);
            bool timerFinished;

            if (_type == SwitchType.NormallyClosed)
                state = !state; //normally closed switches are reversed

            timerFinished = _onDelayTimer.Evaluate (_activated != state); // if current state and switch activation do not match start timer
            if (timerFinished) // once timer has finished, toggle switch activation
                _activated = !_activated;

            return _activated;
        }

        public void SetName (string name) {
            _name = name;
            AquaPicDrivers.DigitalInput.SetChannelName (_channel, _name);
        }
    }
}

