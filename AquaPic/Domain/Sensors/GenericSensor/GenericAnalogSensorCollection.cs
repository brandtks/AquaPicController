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

namespace AquaPic.Sensors
{
    public class GenericAnalogSensorCollection : GenericSensorCollection
    {
        public GenericAnalogSensorCollection (string sensorSettingsArrayName) : base (sensorSettingsArrayName) { }

        public override void AddAllSensors () {
            var sensorSettings = SettingsHelper.ReadAllSettingsInArray<GenericAnalogSensorSettings> (sensorSettingsFileName, sensorSettingsArrayName);
            foreach (var setting in sensorSettings) {
                AddSensor (setting, false);
            }
        }

        protected override GenericSensor OnCreateSensor (GenericSensorSettings settings) {
            var sensorSettings = settings as GenericAnalogSensorSettings;
            if (sensorSettings == null) {
                throw new ArgumentException ("Settings must be GenericAnalogSensorSettings");
            }

            var sensor = CreateAnalogSensor (sensorSettings);

            return sensor;
        }

        protected virtual GenericAnalogSensor CreateAnalogSensor (GenericAnalogSensorSettings settings) => throw new NotImplementedException ();

        protected override GenericSensorSettings OnUpdateSensor (string name, GenericSensorSettings settings) {
            var phProbe = sensors[name] as GenericAnalogSensor;
            var sensorSettings = settings as GenericAnalogSensorSettings;
            sensorSettings.zeroScaleCalibrationValue = phProbe.zeroScaleCalibrationActual;
            sensorSettings.zeroScaleCalibrationValue = phProbe.zeroScaleCalibrationValue;
            sensorSettings.fullScaleCalibrationActual = phProbe.fullScaleCalibrationActual;
            sensorSettings.fullScaleCalibrationValue = phProbe.fullScaleCalibrationValue;
            return sensorSettings;
        }

        public override GenericSensorSettings GetSensorSettings (string name) {
            CheckSensorKey (name);
            var phProbe = sensors[name] as GenericAnalogSensor;
            var settings = new GenericAnalogSensorSettings ();
            settings.name = phProbe.name;
            settings.channel = phProbe.channel;
            settings.zeroScaleCalibrationActual = phProbe.zeroScaleCalibrationActual;
            settings.zeroScaleCalibrationValue = phProbe.zeroScaleCalibrationValue;
            settings.fullScaleCalibrationActual = phProbe.fullScaleCalibrationActual;
            settings.fullScaleCalibrationValue = phProbe.fullScaleCalibrationValue;
            settings.lowPassFilterFactor = phProbe.lowPassFilterFactor;
            return settings;
        }

        public void SetCalibrationData (string name, float zeroScaleActual, float zeroScaleValue, float fullScaleActual, float fullScaleValue) {
            CheckSensorKey (name);

            if (fullScaleValue <= zeroScaleValue)
                throw new ArgumentException ("Full scale value can't be less than or equal to zero value");

            if (fullScaleActual < 0.0f)
                throw new ArgumentException ("Full scale actual can't be less than zero");

            var phProbe = sensors[name] as GenericAnalogSensor;

            phProbe.zeroScaleCalibrationActual = zeroScaleActual;
            phProbe.zeroScaleCalibrationValue = zeroScaleValue;
            phProbe.fullScaleCalibrationActual = fullScaleActual;
            phProbe.fullScaleCalibrationValue = fullScaleValue;

            UpdateSensorSettingsInFile (name);
        }
    }
}
