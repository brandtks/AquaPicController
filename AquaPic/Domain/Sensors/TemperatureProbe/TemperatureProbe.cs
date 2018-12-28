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
using AquaPic.PubSub;

namespace AquaPic.Sensors.TemperatureProbe
{
    public class TemperatureProbe : GenericSensor
    {
        public float temperature { get; protected set; }

        public float zeroScaleActual { get; set; }
        public float zeroScaleValue { get; set; }
        public float fullScaleActual { get; set; }
        public float fullScaleValue { get; set; }
        public int probeDisconnectedAlarmIndex { get; private set; }
        public bool connected {
            get {
                return !Alarm.CheckAlarming (probeDisconnectedAlarmIndex);
            }
        }

        public TemperatureProbe (
            string name,
            IndividualControl channel,
            float zeroScaleActual,
            float zeroScaleValue,
            float fullScaleActual,
            float fullScaleValue)
            : base (name, channel)
        {
            this.zeroScaleActual = zeroScaleActual;
            this.zeroScaleValue = zeroScaleValue;
            this.fullScaleActual = fullScaleActual;
            this.fullScaleValue = fullScaleValue;
            temperature = this.zeroScaleActual;
            probeDisconnectedAlarmIndex = -1;
        }

        public override void OnCreate () {
            AquaPicDrivers.AnalogInput.AddChannel (channel, string.Format ("{0}, Temperature Probe", name));
            AquaPicDrivers.AnalogInput.SubscribeConsumer (channel, this);
            probeDisconnectedAlarmIndex = Alarm.Subscribe ("Temperature probe disconnected, " + name);
        }

        public void Remove () {
            AquaPicDrivers.AnalogInput.RemoveChannel (channel);
            AquaPicDrivers.AnalogInput.UnsubscribeConsumer (channel, this);
            Alarm.Clear (probeDisconnectedAlarmIndex);
        }

        public override ValueType GetValue () {
            return temperature;
        }

        public override void OnValueChangedAction (object parm) {
            var args = parm as ValueChangedEvent;
            var oldTemperature = temperature;
            temperature = ScaleRawLevel (Convert.ToSingle (args.newValue));

            if (temperature < zeroScaleActual) {
                if (!Alarm.CheckAlarming (probeDisconnectedAlarmIndex)) {
                    Alarm.Post (probeDisconnectedAlarmIndex);
                }
            } else {
                if (Alarm.CheckAlarming (probeDisconnectedAlarmIndex)) {
                    Alarm.Clear (probeDisconnectedAlarmIndex);
                }
            }

            NotifyValueChanged (temperature, oldTemperature);
        }


        protected float ScaleRawLevel (float rawValue) {
            return rawValue.Map (zeroScaleValue, fullScaleValue, zeroScaleActual, fullScaleActual);
        }
    }
}

