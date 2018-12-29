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

        public float zeroScaleCalibrationActual { get; set; }
        public float zeroScaleCalibrationValue { get; set; }
        public float fullScaleCalibrationActual { get; set; }
        public float fullScaleCalibrationValue { get; set; }

        public int lowPassFilterFactor { get; set; }

        public int sensorDisconnectedAlarmIndex { get; protected set; }
        public bool connected {
            get {
                return !Alarm.CheckAlarming (sensorDisconnectedAlarmIndex);
            }
        }

        public GenericAnalogSensor (
            string name,
            IndividualControl channel,
            float zeroScaleCalibrationActual,
            float zeroScaleCalibrationValue,
            float fullScaleCalibrationActual,
            float fullScaleCalibrationValue,
            int lowPassFilterFactor)
            : base (name, channel) 
        {
            this.zeroScaleCalibrationActual = zeroScaleCalibrationActual;
            this.zeroScaleCalibrationValue = zeroScaleCalibrationValue;
            this.fullScaleCalibrationActual = fullScaleCalibrationActual;
            this.fullScaleCalibrationValue = fullScaleCalibrationValue;
            this.lowPassFilterFactor = lowPassFilterFactor;
            value = this.zeroScaleCalibrationActual;
            sensorDisconnectedAlarmIndex = -1;
        }

        public override ValueType GetValue () {
            return value;
        }

        public override void OnValueChangedAction (object parm) {
            var args = parm as ValueChangedEvent;
            var oldValue = value;
            value = ScaleRawLevel (Convert.ToSingle (args.newValue));

            if (value < zeroScaleCalibrationActual) {
                Alarm.Post (sensorDisconnectedAlarmIndex);
            } else {
                Alarm.Clear (sensorDisconnectedAlarmIndex);
            }

            NotifyValueChanged (value, oldValue);
        }


        protected float ScaleRawLevel (float rawValue) {
            return rawValue.Map (zeroScaleCalibrationValue, fullScaleCalibrationValue, zeroScaleCalibrationActual, fullScaleCalibrationActual);
        }
    }
}
