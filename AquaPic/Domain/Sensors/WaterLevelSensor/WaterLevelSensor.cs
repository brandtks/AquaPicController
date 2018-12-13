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
using GoodtimeDevelopment.Utilites;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Globals;

namespace AquaPic.Sensors
{
    public class WaterLevelSensor : ISensor<float>
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

        int _sensorDisconnectedAlarmIndex;
        public int sensorDisconnectedAlarmIndex {
            get {
                return _sensorDisconnectedAlarmIndex;
            }
        }

        public bool connected {
            get {
                return !Alarm.CheckAlarming (_sensorDisconnectedAlarmIndex);
            }
        }

        public string waterLevelGroupName;

        public WaterLevelSensor (
            string name,
            IndividualControl ic,
            string waterLevelGroupName
        ) {
            _channel = ic;
            _name = name;
            _level = 0.0f;

            zeroScaleValue = 819.2f;
            fullScaleActual = 15f;
            fullScaleValue = 3003.73f;

            _sensorDisconnectedAlarmIndex = Alarm.Subscribe ("Analog level probe disconnected, " + name);
            Add (ic);

            this.waterLevelGroupName = waterLevelGroupName;
        }

        public WaterLevelSensor (string name, IndividualControl ic)
            : this (name, ic, string.Empty) { }

        public void Add (IndividualControl channel) {
            if (!_channel.Equals (channel)) {
                Remove ();
            }

            _channel = channel;

            if (_channel.IsNotEmpty ()) {
                AquaPicDrivers.AnalogInput.AddChannel (_channel, string.Format ("{0}, Water Level Sensor", name));
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
            AquaPicDrivers.AnalogInput.SetChannelName (_channel, string.Format ("{0}, Water Level Sensor", name));
        }
    }
}

