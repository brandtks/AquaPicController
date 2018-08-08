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
    public class PhProbe : ISensor<float>
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

        protected float _level;
        public float level {
            get {
                return _level;
            }
        }

        public float zeroActual;
        public float zeroValue;
        public float fullScaleActual;
        public float fullScaleValue;

        public int probeDisconnectedAlarmIndex;

        public PhProbe (
            string name,
            IndividualControl channel,
            float zeroActual,
            float zeroValue,
            float fullScaleActual,
            float fullScaleValue
        ) {
            _name = name;
            _channel = channel;
            this.zeroActual = zeroActual;
            this.zeroValue = zeroValue;
            this.fullScaleActual = fullScaleActual;
            this.fullScaleValue = fullScaleValue;
            _level = this.zeroActual;
            Add (_channel);
            probeDisconnectedAlarmIndex = Alarm.Subscribe ("pH probe disconnected, " + name);
        }

        public void Add (IndividualControl channel) {
            if (_channel.IsNotEmpty ()) {
                Remove ();
            }

            _channel = channel;

            if (_channel.IsNotEmpty ()) {
                AquaPicDrivers.PhOrp.AddChannel (_channel, name);
            }
        }

        public void Remove () {
            if (_channel.IsNotEmpty ()) {
                AquaPicDrivers.PhOrp.RemoveChannel (_channel);
            }
        }

        public float Get () {
            _level = AquaPicDrivers.PhOrp.GetChannelValue (_channel);
            _level = _level.Map (zeroValue, fullScaleValue, zeroActual, fullScaleActual);

            if (_level < zeroActual) {
                Alarm.Post (probeDisconnectedAlarmIndex);
            } else {
                Alarm.Clear (probeDisconnectedAlarmIndex);
            }

            return _level;
        }

        public void SetName (string name) {
            _name = name;
            AquaPicDrivers.PhOrp.SetChannelName (_channel, _name);
        }
    }
}

