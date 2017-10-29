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
using GoodtimeDevelopment.Utilites;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Globals;

namespace AquaPic.Sensors
{
    public class AnalogLevelSensor : ISensor<float>
    {
        protected float _level;
        public float level {
            get {
                return _level;
            }
        }

        protected string _name;
        public string name {
            get {
                return _name;
            }
        }

        public IndividualControl _channel;
        public IndividualControl channel {
            get {
                return _channel;
            }
        }

        public float zeroScaleValue;
        public float fullScaleActual;
        public float fullScaleValue;

        private int _sensorDisconnectedAlarmIndex;
        public int disconnectedAlarmIndex {
            get {
                return _sensorDisconnectedAlarmIndex;
            }
        }

        public AnalogLevelSensor (string name, IndividualControl ic) {
            _channel = ic;
            _name = name;
            _level = 0.0f;

            zeroScaleValue = 819.2f;
            fullScaleActual = 15.0f;
            fullScaleValue = 4096.0f;

            _sensorDisconnectedAlarmIndex = Alarm.Subscribe ("Analog level probe disconnected, " + name);
            Add (ic);
        }

        public void Add (IndividualControl channel) {
            if (!_channel.Equals (channel)) {
                Remove ();
            }
            
            _channel = channel;

            if (_channel.IsNotEmpty ()) {
                AquaPicDrivers.AnalogInput.AddChannel (_channel, name);
            }
        }

        public void Remove () {
            if (_channel.IsNotEmpty ()) {
                AquaPicDrivers.AnalogInput.RemoveChannel (_channel);
            }
        }

        public float Get () {
            _level = AquaPicDrivers.AnalogInput.GetChannelValue (_channel);
            _level = _level.Map (zeroScaleValue, fullScaleValue, 0.0f, fullScaleActual);

            if (_level < 0.0f) {
                if (!Alarm.CheckAlarming (_sensorDisconnectedAlarmIndex)) {
                    Alarm.Post (_sensorDisconnectedAlarmIndex);
                }
            } else {
                if (Alarm.CheckAlarming (_sensorDisconnectedAlarmIndex)) {
                    Alarm.Clear (_sensorDisconnectedAlarmIndex);
                }
            }

            return level;
        }

        public void SetName (string name) {
            _name = name;
            AquaPicDrivers.AnalogInput.SetChannelName (_channel, name);
        }
    }
}

