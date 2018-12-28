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
using Newtonsoft.Json.Linq;
using AquaPic.Runtime;
using AquaPic.Sensors.TemperatureProbe;

namespace AquaPic.Sensors
{
    public class AquaPicSensors
    {
        public static readonly string sensorSettingsFileName = "sensors";

        public static FloatSwitchCollection FloatSwitches = FloatSwitchCollection.SharedFloatSwitchCollectionInstance;
        public static WaterLevelSensorCollection WaterLevelSensors = WaterLevelSensorCollection.SharedWaterLevelSensorCollectionInstance;
        public static TemperatureProbeCollection TemperatureProbes = TemperatureProbeCollection.SharedTemperatureProbeCollectionInstance;

        public static void AddSensors () {
            if (SettingsHelper.SettingsFileExists (sensorSettingsFileName)) {
                FloatSwitches.AddAllSensors ();
                WaterLevelSensors.AddAllSensors ();
                TemperatureProbes.AddAllSensors ();
            } else {
                Logger.Add ("Sensors settings file did not exist, created new water level settings");

                var jo = new JObject ();
                jo.Add (new JProperty (FloatSwitches.sensorSettingsArrayName, new JArray ()));
                jo.Add (new JProperty (WaterLevelSensors.sensorSettingsArrayName, new JArray ()));
                jo.Add (new JProperty (TemperatureProbes.sensorSettingsArrayName, new JArray ()));

                SettingsHelper.WriteSettingsFile (sensorSettingsFileName, jo);
            }
        }
    }
}
