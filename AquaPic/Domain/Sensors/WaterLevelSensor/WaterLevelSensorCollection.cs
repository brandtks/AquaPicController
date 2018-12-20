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

        public override GenericSensor OnCreateSensor (GenericSensorSettings settings) {
            var sensorSettings = settings as WaterLevelSensorSettings;
            if (sensorSettings == null) {
                throw new ArgumentException ("Settings must be WaterLevelSensorSettings");
            }

            var waterLevelSensor = new WaterLevelSensor (
                sensorSettings.name,
                sensorSettings.channel,
                sensorSettings.waterLevelGroupName,
                sensorSettings.zeroScaleCalibrationValue,
                sensorSettings.fullScaleCalibrationActual,
                sensorSettings.fullScaleCalibrationValue);

            return waterLevelSensor;
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
    }
}
