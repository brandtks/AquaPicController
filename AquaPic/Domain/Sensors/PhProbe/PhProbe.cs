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
    public class PhProbe : GenericAnalogSensor
    {
        public PhProbe (
            string name,
            IndividualControl channel,
            float zeroScaleActual,
            float zeroScaleValue,
            float fullScaleActual,
            float fullScaleValue,
            int lowPassFilterFactor)
        : base (name,
            channel,
            zeroScaleActual,
            zeroScaleValue,
            fullScaleActual,
            fullScaleValue,
            lowPassFilterFactor) { }

        public override void OnCreate () {
            AquaPicDrivers.PhOrp.AddChannel (channel, string.Format ("{0}, pH Probe", name), lowPassFilterFactor);
            AquaPicDrivers.PhOrp.SubscribeConsumer (channel, this);
            probeDisconnectedAlarmIndex = Alarm.Subscribe ("pH probe disconnected, " + name);
        }

        public override void OnRemove () {
            AquaPicDrivers.PhOrp.RemoveChannel (channel);
            AquaPicDrivers.PhOrp.UnsubscribeConsumer (channel, this);
            Alarm.Clear (probeDisconnectedAlarmIndex);
        }
    }
}

