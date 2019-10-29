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
using AquaPic.DataLogging;
using AquaPic.Drivers;
using AquaPic.Service;

namespace AquaPic.Gadgets.Sensor
{
    public class PhProbe : GenericAnalogSensor
    {
        public IDataLogger dataLogger;

        public PhProbe (GenericAnalogSensorSettings settings) : base (settings) {
            dataLogger = Factory.GetDataLogger (string.Format ("{0}PhProbe", name.RemoveWhitespace ()));
            var channelName = string.Format ("{0}, pH Probe", name);
            Driver.PhOrp.AddChannel (channel, channelName, lowPassFilterFactor);
            sensorDisconnectedAlarmIndex = Alarm.Subscribe ("pH probe disconnected, " + name);
            Subscribe (Driver.PhOrp.GetChannelEventPublisherKey (channelName));
        }

        public override void Dispose () {
            Driver.PhOrp.RemoveChannel (channel);
            Alarm.Clear (sensorDisconnectedAlarmIndex);
            Unsubscribe ();
        }

        public override void OnValueUpdatedAction (object parm) {
            var args = parm as ValueUpdatedEvent;
            var val = ScaleRawLevel (Convert.ToSingle (args.value));
            if (val > zeroScaleCalibrationActual) {
                dataLogger.AddEntry (val);
            } else {
                dataLogger.AddEntry ("probe disconnected");
            }
        }
    }
}

