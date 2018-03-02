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
using AquaPic.Globals;
using AquaPic.Runtime;

namespace AquaPic.Sensors
{
    public class TemperatureProbe : ISensor<float>
    {
        protected IndividualControl _channel;
        public IndividualControl channel {
            get {
                return _channel;
            }
        }

        protected string _name;
        public string name {
            get {
                return _name;
            }
        }

        protected float _temperature;
        public float temperature {
            get {
                return _temperature;
            }
        }

        public string temperatureGroupName;

        public float zeroActual;
        public float zeroValue;
        public float fullScaleActual;
        public float fullScaleValue;

        public int probeDisconnectedAlarmIndex;

        public TemperatureProbe (
            string name,
            IndividualControl channel,
            float zeroActual,
            float zeroValue,
            float fullScaleActual,
            float fullScaleValue, 
            string temperatureGroupName) 
        {
            _name = name;
            _channel = channel;
            this.zeroActual = zeroActual;
            this.zeroValue = zeroValue;
            this.fullScaleActual = fullScaleActual;
            this.fullScaleValue = fullScaleValue;
            this.temperatureGroupName = temperatureGroupName;
            _temperature = this.zeroActual;
            Add (_channel);
            probeDisconnectedAlarmIndex = Alarm.Subscribe ("Temperature probe disconnected, " + name);
        }

        public void Add (IndividualControl channel) {
            if (_channel.IsNotEmpty ()) {
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
            _temperature = AquaPicDrivers.AnalogInput.GetChannelValue (_channel);
            _temperature = temperature.Map (zeroValue, fullScaleValue, zeroActual, fullScaleActual);

            if (temperature < zeroActual) {
                if (!Alarm.CheckAlarming (probeDisconnectedAlarmIndex)) {
                    Alarm.Post (probeDisconnectedAlarmIndex);
                }
            } else {
                if (Alarm.CheckAlarming (probeDisconnectedAlarmIndex)) {
                    Alarm.Clear (probeDisconnectedAlarmIndex);
                }
            }

            return _temperature;
        }

        public void SetName (string name) {
            _name = name;
            AquaPicDrivers.AnalogInput.SetChannelName (_channel, _name);
        }
    }
}

