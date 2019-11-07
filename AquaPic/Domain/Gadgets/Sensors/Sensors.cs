#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2019 Goodtime Development

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
using AquaPic.Service;

namespace AquaPic.Gadgets.Sensor
{
    public class Sensors {
        public static FloatSwitchCollection FloatSwitches = FloatSwitchCollection.SharedFloatSwitchCollectionInstance;
        public static WaterLevelSensorCollection WaterLevelSensors = WaterLevelSensorCollection.SharedWaterLevelSensorCollectionInstance;
        public static TemperatureProbeCollection TemperatureProbes = TemperatureProbeCollection.SharedTemperatureProbeCollectionInstance;
        public static PhProbeCollection PhProbes = PhProbeCollection.SharedPhProbeCollectionInstance;
        public static SwitchCollection Switches = SwitchCollection.SharedSwitchCollectionInstance;
        public static SpecificGravitySensorCollection SpecificGravitySensors = SpecificGravitySensorCollection.SharedSpecificGravityCollection;

        public static void AddSensors () {
            if (SettingsHelper.SettingsFileExists (GenericSensorCollection.sensorSettingsFileName)) {
                FloatSwitches.ReadAllGadgetsFromFile ();
                WaterLevelSensors.ReadAllGadgetsFromFile ();
                TemperatureProbes.ReadAllGadgetsFromFile ();
                PhProbes.ReadAllGadgetsFromFile ();
                Switches.ReadAllGadgetsFromFile ();
                SpecificGravitySensors.ReadAllGadgetsFromFile ();
            } else {
                Logger.Add ("Sensors settings file did not exist, created new water level settings");

                var jo = new JObject ();
                jo.Add (new JProperty (FloatSwitches.gadgetSettingsArrayName, new JArray ()));
                jo.Add (new JProperty (WaterLevelSensors.gadgetSettingsArrayName, new JArray ()));
                jo.Add (new JProperty (TemperatureProbes.gadgetSettingsArrayName, new JArray ()));
                jo.Add (new JProperty (PhProbes.gadgetSettingsArrayName, new JArray ()));
                jo.Add (new JProperty (Switches.gadgetSettingsArrayName, new JArray ()));
                jo.Add (new JProperty (SpecificGravitySensors.gadgetSettingsArrayName, new JArray ()));

                SettingsHelper.WriteSettingsFile (GenericSensorCollection.sensorSettingsFileName, jo);
            }
        }
    }
}
