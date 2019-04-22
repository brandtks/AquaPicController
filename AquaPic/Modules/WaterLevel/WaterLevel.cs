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
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using AquaPic.Service;
using AquaPic.Gadgets.Sensor;
using AquaPic.Gadgets.Sensor.FloatSwitch;
using AquaPic.Gadgets.Sensor.WaterLevelSensor;
using AquaPic.DataLogging;

namespace AquaPic.Modules
{
    public partial class WaterLevel
    {
        static Dictionary<string, WaterLevelGroup> waterLevelGroups;

        /**************************************************************************************************************/
        /* Default water level properties                                                                             */
        /**************************************************************************************************************/
        public static int waterLevelGroupCount {
            get {
                return waterLevelGroups.Count;
            }
        }

        public static string firstWaterLevelGroup {
            get {
                if (waterLevelGroups.Count > 0) {
                    var first = waterLevelGroups.First ();
                    return first.Key;
                } 
                return string.Empty;
            }
        }

        const string settingsFileName = "water-level";
        const string groupSettingsArrayName = "waterLevelGroups";

        static WaterLevel () { }

        public static void Init () {
            Logger.Add ("Initializing Water Level");

            waterLevelGroups = new Dictionary<string, WaterLevelGroup> ();

            if (SettingsHelper.SettingsFileExists (settingsFileName)) {
                var groupSettings = SettingsHelper.ReadAllSettingsInArray<WaterLevelGroupSettings> (settingsFileName, groupSettingsArrayName);
                foreach (var setting in groupSettings) {
                    AddWaterLevelGroup (setting, false);
                }
            } else {
                Logger.Add ("Water level settings file did not exist, created new water level settings");

                var jo = new JObject ();
                jo.Add (new JProperty (groupSettingsArrayName, new JArray ()));
                jo.Add (new JProperty ("defaultWaterLevelGroup", string.Empty));

                SettingsHelper.WriteSettingsFile (settingsFileName, jo);
            }
        }

        /**************************************************************************************************************/
        /* Water Level Groups                                                                                         */
        /**************************************************************************************************************/
        public static void AddWaterLevelGroup (WaterLevelGroupSettings settings, bool saveToFile = true) {
            if (WaterLevelGroupNameExists (settings.name)) {
                throw new Exception (string.Format ("Water Level Group {0} already exists", settings.name));
            }

            waterLevelGroups[settings.name] = new WaterLevelGroup (
                settings.name,
                settings.highAnalogAlarmSetpoint,
                settings.enableHighAnalogAlarm,
                settings.lowAnalogAlarmSetpoint,
                settings.enableLowAnalogAlarm,
                settings.floatSwitches,
                settings.waterLevelSensors);

            if (saveToFile) {
                AddWaterLevelGroupSettingsToFile (settings.name);
            }
        }

        public static void UpdateWaterLevelGroup (string name, WaterLevelGroupSettings settings) {
            if (!CheckWaterLevelGroupKeyNoThrow (name)) {
                RemoveWaterLevelGroup (name);
            }

            AddWaterLevelGroup (settings);
        }

        public static void RemoveWaterLevelGroup (string name) {
            CheckWaterLevelGroupKey (name);
            waterLevelGroups.Remove (name);
            DeleteWaterLevelGroupSettingsFromFile (name);
        }

        public static void CheckWaterLevelGroupKey (string name) {
            if (!waterLevelGroups.ContainsKey (name)) {
                throw new ArgumentException ("name");
            }
        }

        public static bool CheckWaterLevelGroupKeyNoThrow (string name) {
            try {
                CheckWaterLevelGroupKey (name);
                return true;
            } catch {
                return false;
            }
        }

        public static bool WaterLevelGroupNameExists (string name) {
            return CheckWaterLevelGroupKeyNoThrow (name);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllWaterLevelGroupNames () {
            List<string> names = new List<string> ();
            foreach (var waterLevelGroup in waterLevelGroups.Values) {
                names.Add (waterLevelGroup.name);
            }
            return names.ToArray ();
        }

        /***Level***/
        public static float GetWaterLevelGroupLevel (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].level;
        }

        /***Max Level***/
        public static float GetWaterLevelGroupMaxLevel (string name) {
            CheckWaterLevelGroupKey (name);
            var maxLevel = 0f;
            foreach (var sensorName in waterLevelGroups[name].waterLevelSensors.Keys) {
                var sensor = Sensors.WaterLevelSensors.GetGadget (sensorName) as WaterLevelSensor;
                if (sensor.fullScaleCalibrationActual > maxLevel) {
                    maxLevel = sensor.fullScaleCalibrationActual;
                }
            }
            return maxLevel;
        }

        /***Float Switch Activated***/
        public static bool GetWaterLevelGroupSwitchesActivated (string name, SwitchFunction function = SwitchFunction.ATO) {
            CheckWaterLevelGroupKey (name);
            bool activated = true;
            foreach (var switchName in waterLevelGroups[name].floatSwitches) {
                var floatSwitch = Sensors.FloatSwitches.GetGadget (switchName) as FloatSwitch;
                if (floatSwitch.switchFuntion == function) {
                    // Using AND because we want all the switches to be activated
                    activated &= floatSwitch.activated;
                }
            }
            return activated;
        }

        /***Analog sensors connected***/
        public static bool GetWaterLevelGroupAnalogSensorConnected (string name) {
            CheckWaterLevelGroupKey (name);
            bool connected = false;
            foreach (var sensorName in waterLevelGroups[name].waterLevelSensors.Keys) {
                var sensor = Sensors.WaterLevelSensors.GetGadget (sensorName) as WaterLevelSensor;
                // Using OR because we really only care that at least one sensor is connected
                connected |= sensor.connected;
            }
            return connected;
        }

        /***High analog alarm setpoint**/
        public static float GetWaterLevelGroupHighAnalogAlarmSetpoint (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].highAnalogAlarmSetpoint;
        }

        /***High analog alarm enable**/
        public static bool GetWaterLevelGroupHighAnalogAlarmEnable (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].enableHighAnalogAlarm;
        }

        /***High analog alarm index**/
        public static int GetWaterLevelGroupHighAnalogAlarmIndex (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].highAnalogAlarmIndex;
        }

        /***High switch alarm index***/
        public static int GetWaterLevelGroupHighSwitchAlarmIndex (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].highSwitchAlarmIndex;
        }

        /***High alarming***/
        public static bool GetWaterLevelGroupHighAlarming (string name) {
            CheckWaterLevelGroupKey (name);
            bool alarming = false;
            if (waterLevelGroups[name].enableHighAnalogAlarm) {
                alarming = Alarm.CheckAlarming (waterLevelGroups[name].highAnalogAlarmIndex);
            }
            alarming |= Alarm.CheckAlarming (waterLevelGroups[name].highSwitchAlarmIndex);
            return alarming;
        }

        /***Low analog alarm setpoint**/
        public static float GetWaterLevelGroupLowAnalogAlarmSetpoint (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].lowAnalogAlarmSetpoint;
        }

        /***Low analog alarm enable**/
        public static bool GetWaterLevelGroupLowAnalogAlarmEnable (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].enableLowAnalogAlarm;
        }

        /***Low analog alarm index**/
        public static int GetWaterLevelGroupLowAnalogAlarmIndex (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].lowAnalogAlarmIndex;
        }

        /***Low switch alarm index***/
        public static int GetWaterLevelGroupLowSwitchAlarmIndex (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].lowSwitchAlarmIndex;
        }

        /***Low alarming***/
        public static bool GetWaterLevelGroupLowAlarming (string name) {
            CheckWaterLevelGroupKey (name);
            bool alarming = false;
            if (waterLevelGroups[name].enableLowAnalogAlarm) {
                alarming = Alarm.CheckAlarming (waterLevelGroups[name].lowAnalogAlarmIndex);
            }
            alarming |= Alarm.CheckAlarming (waterLevelGroups[name].lowSwitchAlarmIndex);
            return alarming;
        }

        /***Data logger***/
        public static IDataLogger GetWaterLevelGroupDataLogger (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].dataLogger;
        }

        /***Settings***************************************************************************************************/
        public static WaterLevelGroupSettings GetWaterLevelGroupSettings (string name) {
            CheckWaterLevelGroupKey (name);
            var settings = new WaterLevelGroupSettings ();
            settings.name = name;
            settings.highAnalogAlarmSetpoint = GetWaterLevelGroupHighAnalogAlarmSetpoint (name);
            settings.enableHighAnalogAlarm = GetWaterLevelGroupHighAnalogAlarmEnable (name);
            settings.lowAnalogAlarmSetpoint = GetWaterLevelGroupLowAnalogAlarmSetpoint (name);
            settings.enableLowAnalogAlarm = GetWaterLevelGroupLowAnalogAlarmEnable (name);
            settings.floatSwitches = GetAllFloatSwitchesForWaterLevelGroup (name);
            settings.waterLevelSensors = GetAllWaterLevelSensorsForWaterLevelGroup (name);
            return settings;
        }

        protected static void AddWaterLevelGroupSettingsToFile (string name) {
            CheckWaterLevelGroupKey (name);
            SettingsHelper.AddSettingsToArray (settingsFileName, groupSettingsArrayName, GetWaterLevelGroupSettings (name));
        }

        protected static void UpdateWaterLevelGroupSettingsInFile (string name) {
            SettingsHelper.UpdateSettingsInArray (settingsFileName, groupSettingsArrayName, name, GetWaterLevelGroupSettings (name));
        }

        protected static void DeleteWaterLevelGroupSettingsFromFile (string name) {
            SettingsHelper.DeleteSettingsFromArray (settingsFileName, groupSettingsArrayName, name);
        }

        /**************************************************************************************************************/
        /* Analog Level Sensor                                                                                        */
        /**************************************************************************************************************/
        public static void AddWaterLevelSensorToWaterLevelGroup (string groupName, string waterLevelSensorName) {
            CheckWaterLevelGroupKey (groupName);
            waterLevelGroups[groupName].waterLevelSensors.Add (waterLevelSensorName, new WaterLevelGroup.InternalWaterLevelSensorState ());
            UpdateWaterLevelGroupSettingsInFile (groupName);
        }

        public static void RemoveWaterLevelSensorFromWaterLevelGroup (string groupName, string waterLevelSensorName) {
            CheckWaterLevelGroupKey (groupName);
            waterLevelGroups[groupName].waterLevelSensors.Remove (waterLevelSensorName);
            UpdateWaterLevelGroupSettingsInFile (groupName);
        }

        /***Getters****************************************************************************************************/
        public static string[] GetAllWaterLevelSensorsForWaterLevelGroup (string groupName) {
            CheckWaterLevelGroupKey (groupName);
            List<string> names = new List<string> ();
            foreach (var sensorName in waterLevelGroups[groupName].waterLevelSensors.Keys) {
                names.Add (sensorName);
            }
            return names.ToArray ();
        }

        /**************************************************************************************************************/
        /* Float Switches                                                                                             */
        /**************************************************************************************************************/
        public static void AddFloatSwitchToWaterLevelGroup (string groupName, string floatSwitchName) {
            CheckWaterLevelGroupKey (groupName);
            waterLevelGroups[groupName].floatSwitches.Add (floatSwitchName);
            UpdateWaterLevelGroupSettingsInFile (groupName);
        }

        public static void RemoveFloatSwitchFromWaterLevelGroup (string groupName, string floatSwitchName) {
            CheckWaterLevelGroupKey (groupName);
            waterLevelGroups[groupName].floatSwitches.Remove (floatSwitchName);
            UpdateWaterLevelGroupSettingsInFile (groupName);
        }

        /***Getters****************************************************************************************************/
        public static string[] GetAllFloatSwitchesForWaterLevelGroup (string groupName) {
            CheckWaterLevelGroupKey (groupName);
            List<string> names = new List<string> ();
            foreach (var sensorName in waterLevelGroups[groupName].floatSwitches) {
                names.Add (sensorName);
            }
            return names.ToArray ();
        }
    }
}

