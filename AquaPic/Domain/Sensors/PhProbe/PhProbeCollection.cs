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
using AquaPic.Runtime;

namespace AquaPic.Sensors.PhProbe
{
    public class PhProbeCollection : GenericAnalogSensorCollection
    {
        public static PhProbeCollection SharedPhProbeCollectionInstance = new PhProbeCollection ();

        protected PhProbeCollection () : base ("phProbes") { }

        public override void AddAllSensors () {
            var sensorSettings = SettingsHelper.ReadAllSettingsInArray<PhProbeSettings> (sensorSettingsFileName, sensorSettingsArrayName);
            foreach (var setting in sensorSettings) {
                AddSensor (setting, false);
            }
        }

        protected override GenericSensor OnCreateSensor (GenericSensorSettings settings) {
            var sensorSettings = settings as PhProbeSettings;
            if (sensorSettings == null) {
                throw new ArgumentException ("Settings must be PhProbeSettings");
            }

            var phProbe = new PhProbe (
                sensorSettings.name,
                sensorSettings.channel,
                sensorSettings.zeroScaleCalibrationActual,
                sensorSettings.zeroScaleCalibrationValue,
                sensorSettings.fullScaleCalibrationActual,
                sensorSettings.fullScaleCalibrationValue,
                sensorSettings.lowPassFilterFactor);

            return phProbe;
        }

        protected override GenericSensorSettings OnUpdateSensor (string name, GenericSensorSettings settings) {
            var phProbe = sensors[name] as PhProbe;
            var sensorSettings = settings as PhProbeSettings;
            sensorSettings.zeroScaleCalibrationValue = phProbe.zeroScaleActual;
            sensorSettings.zeroScaleCalibrationValue = phProbe.zeroScaleValue;
            sensorSettings.fullScaleCalibrationActual = phProbe.fullScaleActual;
            sensorSettings.fullScaleCalibrationValue = phProbe.fullScaleValue;
            return sensorSettings;
        }

        public override GenericSensorSettings GetSensorSettings (string name) {
            CheckSensorKey (name);
            var phProbe = sensors[name] as PhProbe;
            var settings = new PhProbeSettings ();
            settings.name = phProbe.name;
            settings.channel = phProbe.channel;
            settings.zeroScaleCalibrationActual = phProbe.zeroScaleActual;
            settings.zeroScaleCalibrationValue = phProbe.zeroScaleValue;
            settings.fullScaleCalibrationActual = phProbe.fullScaleActual;
            settings.fullScaleCalibrationValue = phProbe.fullScaleValue;
            settings.lowPassFilterFactor = phProbe.lowPassFilterFactor;
            return settings;
        }
    }
}
