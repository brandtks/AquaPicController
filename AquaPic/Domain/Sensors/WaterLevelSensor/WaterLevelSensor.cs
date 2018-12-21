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
    public class WaterLevelSensor : GenericSensor
    {
        public float level { get; protected set; }

        public float zeroScaleValue { get; set; }
        public float fullScaleActual { get; set; }
        public float fullScaleValue { get; set; }
        public int sensorDisconnectedAlarmIndex { get; private set; }
        public bool connected {
            get {
                return !Alarm.CheckAlarming (sensorDisconnectedAlarmIndex);
            }
        }

        public WaterLevelSensor (
            string name,
            IndividualControl channel,
            float zeroScaleValue,
            float fullScaleActual,
            float fullScaleValue) 
            : base (name, channel)
        {
            level = 0.0f;
            this.zeroScaleValue = zeroScaleValue;
            this.fullScaleActual = fullScaleActual;
            this.fullScaleValue = fullScaleValue;
            sensorDisconnectedAlarmIndex = -1;
        }


        public override void OnCreate () {
            AquaPicDrivers.AnalogInput.AddChannel (channel, string.Format ("{0}, Water Level Sensor", name));
            AquaPicDrivers.AnalogInput.AddHandlerOnInputChannelValueChangedEvent (channel, OnInputChannelValueChangedEvent);
            sensorDisconnectedAlarmIndex = Alarm.Subscribe ("Analog level probe disconnected, " + name);
        }

        public override void OnRemove () {
            AquaPicDrivers.AnalogInput.RemoveChannel (channel);
            AquaPicDrivers.AnalogInput.RemoveHandlerOnInputChannelValueChangedEvent (channel, OnInputChannelValueChangedEvent);
            Alarm.Clear (sensorDisconnectedAlarmIndex);
        }

        protected void OnInputChannelValueChangedEvent (object sender, InputChannelValueChangedEventArgs args) {
            var newLevel = Convert.ToSingle (args.newValue);
            level = newLevel.Map (zeroScaleValue, fullScaleValue, 0.0f, fullScaleActual);

            if (level < 0.0f) {
                if (!Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)) {
                    Alarm.Post (sensorDisconnectedAlarmIndex);
                }
            } else {
                if (Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)) {
                    Alarm.Clear (sensorDisconnectedAlarmIndex);
                }
            }
        }
    }
}

