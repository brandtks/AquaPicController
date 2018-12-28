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

namespace AquaPic.Sensors.TemperatureProbe
{
    public class TemperatureProbeCollection : GenericSensorCollection
    {
        public static TemperatureProbeCollection SharedTemperatureProbeCollectionInstance = new TemperatureProbeCollection ();

        protected TemperatureProbeCollection () : base ("temperatureProbes") { }

        public override void AddAllSensors () {
            var sensorSettings = SettingsHelper.ReadAllSettingsInArray<TemperatureProbeSettings> (sensorSettingsFileName, sensorSettingsArrayName);
            foreach (var setting in sensorSettings) {
                AddSensor (setting, false);
            }
        }

        protected override GenericSensor OnCreateSensor (GenericSensorSettings settings) {
            var sensorSettings = settings as TemperatureProbeSettings;
            if (sensorSettings == null) {
                throw new ArgumentException ("Settings must be TemperatureProbeSettings");
            }

            var temperatureProbe = new TemperatureProbe (
                sensorSettings.name,
                sensorSettings.channel,
                sensorSettings.zeroScaleCalibrationActual,
                sensorSettings.zeroScaleCalibrationValue,
                sensorSettings.fullScaleCalibrationActual,
                sensorSettings.fullScaleCalibrationValue);

            return temperatureProbe;
        }

        protected override GenericSensorSettings OnUpdateSensor (string name, GenericSensorSettings settings) {
            var tempProbe = sensors[name] as TemperatureProbe;
            var sensorSettings = settings as TemperatureProbeSettings;
            sensorSettings.zeroScaleCalibrationValue = tempProbe.zeroScaleActual;
            sensorSettings.zeroScaleCalibrationValue = tempProbe.zeroScaleValue;
            sensorSettings.fullScaleCalibrationActual = tempProbe.fullScaleActual;
            sensorSettings.fullScaleCalibrationValue = tempProbe.fullScaleValue;
            return sensorSettings;
        }

        public override GenericSensorSettings GetSensorSettings (string name) {
            CheckSensorKey (name);
            var tempProbe = sensors[name] as TemperatureProbe;
            var settings = new TemperatureProbeSettings ();
            settings.name = tempProbe.name;
            settings.channel = tempProbe.channel;
            settings.zeroScaleCalibrationActual = tempProbe.zeroScaleActual;
            settings.zeroScaleCalibrationValue = tempProbe.zeroScaleValue;
            settings.fullScaleCalibrationActual = tempProbe.fullScaleActual;
            settings.fullScaleCalibrationValue = tempProbe.fullScaleValue;
            return settings;
        }

        public void SetCalibrationData (string name, float zeroScaleActual, float zeroScaleValue, float fullScaleActual, float fullScaleValue) {
            CheckSensorKey (name);

            if (fullScaleValue <= zeroScaleValue)
                throw new ArgumentException ("Full scale value can't be less than or equal to zero value");

            if (fullScaleActual < 0.0f)
                throw new ArgumentException ("Full scale actual can't be less than zero");

            var temperatureProbe = sensors[name] as TemperatureProbe;

            temperatureProbe.zeroScaleActual = zeroScaleActual;
            temperatureProbe.zeroScaleValue = zeroScaleValue;
            temperatureProbe.fullScaleActual = fullScaleActual;
            temperatureProbe.fullScaleValue = fullScaleValue;

            UpdateSensorSettingsInFile (name);
        }
    }
}
