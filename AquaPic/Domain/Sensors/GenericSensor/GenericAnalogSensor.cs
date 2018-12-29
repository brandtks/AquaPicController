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
using GoodtimeDevelopment.Utilites;
using AquaPic.Globals;
using AquaPic.Runtime;
using AquaPic.PubSub;

namespace AquaPic.Sensors
{
    public class GenericAnalogSensor : GenericSensor
    {
        public float value { get; protected set; }

        public float zeroScaleActual { get; set; }
        public float zeroScaleValue { get; set; }
        public float fullScaleActual { get; set; }
        public float fullScaleValue { get; set; }

        public int lowPassFilterFactor { get; set; }

        public int probeDisconnectedAlarmIndex { get; protected set; }
        public bool connected {
            get {
                return !Alarm.CheckAlarming (probeDisconnectedAlarmIndex);
            }
        }

        public GenericAnalogSensor (
            string name,
            IndividualControl channel,
            float zeroScaleActual,
            float zeroScaleValue,
            float fullScaleActual,
            float fullScaleValue,
            int lowPassFilterFactor)
            : base (name, channel) 
        {
            this.zeroScaleActual = zeroScaleActual;
            this.zeroScaleValue = zeroScaleValue;
            this.fullScaleActual = fullScaleActual;
            this.fullScaleValue = fullScaleValue;
            this.lowPassFilterFactor = lowPassFilterFactor;
            value = this.zeroScaleActual;
            probeDisconnectedAlarmIndex = -1;
        }

        public override ValueType GetValue () {
            return value;
        }

        public override void OnValueChangedAction (object parm) {
            var args = parm as ValueChangedEvent;
            var oldValue = value;
            value = ScaleRawLevel (Convert.ToSingle (args.newValue));

            if (value < zeroScaleActual) {
                Alarm.Post (probeDisconnectedAlarmIndex);
            } else {
                Alarm.Clear (probeDisconnectedAlarmIndex);
            }

            NotifyValueChanged (value, oldValue);
        }


        protected float ScaleRawLevel (float rawValue) {
            return rawValue.Map (zeroScaleValue, fullScaleValue, zeroScaleActual, fullScaleActual);
        }
    }
}
