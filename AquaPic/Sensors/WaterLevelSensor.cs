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
using AquaPic.Sensors;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Sensors
{
    public class WaterLevelSensor : AnalogLevelSensor
    {
        private bool _enable;
        public bool enable {
            get {
                return _enable;
            }
            set {
                _enable = value;
                if (!_enable) {
                    if (lowAlarmIndex != -1) {
                        Alarm.Clear (highAlarmIndex);
                    }

                    if (lowAlarmIndex != -1) {
                        Alarm.Clear (lowAlarmIndex);
                    }

                    if (disconnectedAlarmIndex != -1) {
                        Alarm.Clear (disconnectedAlarmIndex);
                    }
                }
            }
        }

        public bool connected {
            get {
                return !Alarm.CheckAlarming (disconnectedAlarmIndex);
            }
        }

        public float highAlarmSetpoint;
        private int _highAlarmIndex;
        public int highAlarmIndex {
            get {
                return _highAlarmIndex;
            }
        }
        public bool enableHighAlarm {
            get {
                return highAlarmIndex != -1;
            }
            set {
                if (value) {
                    _highAlarmIndex = Alarm.Subscribe ("High water level, " + name);
                } else {
                    _highAlarmIndex = -1;
                }
            }
        }

        public float lowAlarmSetpoint;
        private int _lowAlarmIndex;
        public int lowAlarmIndex {
            get {
                return _lowAlarmIndex;
            }
        }
        public bool enableLowAlarm {
            get {
                return lowAlarmIndex != -1;
            }
            set {
                if (value) {
                    _lowAlarmIndex = Alarm.Subscribe ("Low water level, " + name);
                } else {
                    _lowAlarmIndex = -1;
                }
            }
        }

        public WaterLevelSensor (
            string name,
            IndividualControl ic,
            float highAlarmSetpoint,
            float lowAlarmSetpoint,
            bool enable,
            bool enableHighAlarm,
            bool enableLowAlarm)
            : base (name, ic) 
        {
            this.enable = enable;

            if (enableHighAlarm && enable) {
                _highAlarmIndex = Alarm.Subscribe ("High water level, " + name);
            } else {
                _highAlarmIndex = -1;
            }

            if (enableLowAlarm && enable) {
                _lowAlarmIndex = Alarm.Subscribe ("Low water level, " + name);
            } else {
                _lowAlarmIndex = -1;
            }

            this.highAlarmSetpoint = highAlarmSetpoint;
            this.lowAlarmSetpoint = lowAlarmSetpoint;
        }

        public WaterLevelSensor (string name, IndividualControl ic)
        : this (name, ic, 0.0f, 0.0f, true, false, false) { }

        public WaterLevelSensor (string name, IndividualControl ic, float highAlarmSetpoint, float lowAlarmSetpoint)
        : this (name, ic, highAlarmSetpoint, lowAlarmSetpoint, true, true, true) { }

        public WaterLevelSensor (string name, IndividualControl ic, float highAlarmSetpoint, float lowAlarmSetpoint, bool enable)
        : this (name, ic, highAlarmSetpoint, lowAlarmSetpoint, enable, true, true) { }

        public float UpdateWaterLevel () {
            if (enable) {
                Get ();

                if ((_lowAlarmIndex != -1)  && connected) {
                    if (level <= lowAlarmSetpoint) {
                        Alarm.Post (_lowAlarmIndex);
                    } else {
                        Alarm.Clear (_lowAlarmIndex);
                    }
                }

                if ((_highAlarmIndex != -1) && connected) {
                    if (level >= highAlarmSetpoint) {
                        Alarm.Post (_highAlarmIndex);
                    } else {
                        Alarm.Clear (_highAlarmIndex);
                    }
                }
            } else {
                _level = 0.0f;
            }

            return level;
        }
    }
}
