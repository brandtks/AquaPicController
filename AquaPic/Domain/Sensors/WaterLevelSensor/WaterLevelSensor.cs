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
using AquaPic.PubSub;

namespace AquaPic.Sensors
{
    public class WaterLevelSensor : GenericAnalogSensor
    {
        public WaterLevelSensor (
            string name,
            IndividualControl channel,
            float zeroScaleValue,
            float fullScaleActual,
            float fullScaleValue,
            int lowPassFilterFactor)
        : base (name,
            channel,
            0,
            zeroScaleValue,
            fullScaleActual,
            fullScaleValue,
            lowPassFilterFactor) { }

        public override void OnCreate () {
            AquaPicDrivers.AnalogInput.AddChannel (channel, string.Format ("{0}, Water Level Sensor", name));
            AquaPicDrivers.AnalogInput.SubscribeConsumer (channel, this);
            probeDisconnectedAlarmIndex = Alarm.Subscribe ("Water level sensor disconnected, " + name);
        }

        public override void OnRemove () {
            AquaPicDrivers.AnalogInput.RemoveChannel (channel);
            AquaPicDrivers.AnalogInput.UnsubscribeConsumer (channel, this);
            Alarm.Clear (probeDisconnectedAlarmIndex);
        }
    }
}

