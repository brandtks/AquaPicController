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
using Gtk;
using GoodtimeDevelopment.Utilites;
using GoodtimeDevelopment.TouchWidget;
using AquaPic.Sensors;
using AquaPic.Sensors.PhProbe;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class PhProbeWidget : SensorWidget
    {
        public PhProbeWidget () : base ("pH Probes", AquaPicSensors.PhProbes, AquaPicDrivers.PhOrp) { }

        public override void GetSensorData () {
            if (sensorName.IsNotEmpty ()) {
                var probe = (PhProbe)AquaPicSensors.PhProbes.GetSensor (sensorName);
                if (probe.connected) {
                    sensorStateTextbox.text = probe.value.ToString ("F2");
                    sensorLabel.Visible = true;
                } else {
                    sensorStateTextbox.text = "Probe disconnected";
                    sensorLabel.Visible = false;
                }
            } else {
                sensorStateTextbox.text = "Probe not available";
                sensorLabel.Visible = false;
            }

            sensorStateTextbox.QueueDraw ();
        }

        protected override SensorSettingsDialog GetSensorSettingsDialog (Window parent, bool forceNew) {
            PhProbeSettings settings;
            if (sensorName.IsNotEmpty () && !forceNew) {
                settings = (PhProbeSettings)AquaPicSensors.PhProbes.GetSensorSettings (sensorName);
            } else {
                settings = new PhProbeSettings ();
            }
            return new PhProbeSettingsDialog (settings, parent);
        }

    }
}
