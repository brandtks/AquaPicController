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
using AquaPic.Globals;
using AquaPic.Runtime;

namespace AquaPic.Sensors
{
    public class WaterLevelSensorCollection : GenericSensorCollection
    {
        public static WaterLevelSensorCollection SharedWaterLevelSensorCollectionInstance = new WaterLevelSensorCollection ();

        protected WaterLevelSensorCollection () : base ("waterLevelSensors") { }

        public override void AddAllSensors () {
            var sensorSettings = SettingsHelper.ReadAllSettingsInArray<WaterLevelSensorSettings> (sensorSettingsFileName, sensorSettingsArrayName);
            foreach (var setting in sensorSettings) {
                AddSensor (setting, false);
            }
        }

        protected override GenericSensor OnCreateSensor (GenericSensorSettings settings) {
            var sensorSettings = settings as WaterLevelSensorSettings;
            if (sensorSettings == null) {
                throw new ArgumentException ("Settings must be WaterLevelSensorSettings");
            }

            var waterLevelSensor = new WaterLevelSensor (
                sensorSettings.name,
                sensorSettings.channel,
                sensorSettings.zeroScaleCalibrationValue,
                sensorSettings.fullScaleCalibrationActual,
                sensorSettings.fullScaleCalibrationValue);

            return waterLevelSensor;
        }

        protected override GenericSensorSettings OnUpdateSensor (string name, GenericSensorSettings settings) {
            var levelSensor = sensors[name] as WaterLevelSensor;
            var sensorSettings = settings as WaterLevelSensorSettings;
            sensorSettings.zeroScaleCalibrationValue = levelSensor.zeroScaleValue;
            sensorSettings.fullScaleCalibrationActual = levelSensor.fullScaleActual;
            sensorSettings.fullScaleCalibrationValue = levelSensor.fullScaleValue;
            return sensorSettings;
        }

        public override GenericSensorSettings GetSensorSettings (string name) {
            CheckSensorKey (name);
            var levelSensor = sensors[name] as WaterLevelSensor;
            var settings = new WaterLevelSensorSettings ();
            settings.name = levelSensor.name;
            settings.channel = levelSensor.channel;
            settings.zeroScaleCalibrationValue = levelSensor.zeroScaleValue;
            settings.fullScaleCalibrationActual = levelSensor.fullScaleActual;
            settings.fullScaleCalibrationValue = levelSensor.fullScaleValue;
            return settings;
        }

        public void SetCalibrationData (string name, float zeroScaleValue, float fullScaleActual, float fullScaleValue) {
            CheckSensorKey (name);

            if (fullScaleValue <= zeroScaleValue)
                throw new ArgumentException ("Full scale value can't be less than or equal to zero value");

            if (fullScaleActual < 0.0f)
                throw new ArgumentException ("Full scale actual can't be less than zero");

            var waterLevelSensor = sensors[name] as WaterLevelSensor;

            waterLevelSensor.zeroScaleValue = zeroScaleValue;
            waterLevelSensor.fullScaleActual = fullScaleActual;
            waterLevelSensor.fullScaleValue = fullScaleValue;

            UpdateSensorSettingsInFile (name);
        }
    }
}
