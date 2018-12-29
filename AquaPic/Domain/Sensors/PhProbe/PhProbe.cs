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

namespace AquaPic.Sensors.PhProbe
{
    public class PhProbe : GenericSensor
    {
        public float level { get; protected set; }

        public float zeroScaleActual;
        public float zeroScaleValue;
        public float fullScaleActual;
        public float fullScaleValue;

        public int probeDisconnectedAlarmIndex;

        public PhProbe (
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
            level = this.zeroScaleActual;
            probeDisconnectedAlarmIndex = -1;
        }

        public override void OnCreate () {
            AquaPicDrivers.AnalogInput.AddChannel (channel, string.Format ("{0}, pH Probe", name));
            AquaPicDrivers.AnalogInput.SubscribeConsumer (channel, this);
            probeDisconnectedAlarmIndex = Alarm.Subscribe ("pH probe disconnected, " + name);
        }

        public override void OnRemove () {
            AquaPicDrivers.AnalogInput.RemoveChannel (channel);
            AquaPicDrivers.AnalogInput.UnsubscribeConsumer (channel, this);
            Alarm.Clear (probeDisconnectedAlarmIndex);
        }

        public override ValueType GetValue () {
            return level;
        }

        public override void OnValueChangedAction (object parm) {
            var args = parm as ValueChangedEvent;
            var oldLevel = level;
            level = ScaleRawLevel (Convert.ToSingle (args.newValue));

            if (level < zeroScaleActual) {
                Alarm.Post (probeDisconnectedAlarmIndex);
            } else {
                Alarm.Clear (probeDisconnectedAlarmIndex);
            }

            NotifyValueChanged (level, oldLevel);
        }


        protected float ScaleRawLevel (float rawValue) {
            return rawValue.Map (zeroScaleValue, fullScaleValue, zeroScaleActual, fullScaleActual);
        }
    }
}

