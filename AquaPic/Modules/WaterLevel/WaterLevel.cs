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
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;
using AquaPic.Globals;
using AquaPic.Sensors;
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
                } else {
                    return string.Empty;
                }
            }
        }

        const string settingsFileName = "water-level";
        const string groupSettingsArrayName = "waterLevelGroups";

        /**************************************************************************************************************/
        /* Water Level                                                                                                */
        /**************************************************************************************************************/
        static WaterLevel () { }

        public static void Init () {
            Logger.Add ("Initializing Water Level");

            waterLevelGroups = new Dictionary<string, WaterLevelGroup> ();

            if (SettingsHelper.SettingsFileExists (settingsFileName)) {
                /**************************************************************************************************/
                /* Water Level Groups                                                                             */
                /**************************************************************************************************/
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

            TaskManager.AddCyclicInterrupt ("Water Level", 1000, Run);
        }

        public static void Run () {
            foreach (var waterLevelGroup in waterLevelGroups.Values) {
                waterLevelGroup.GroupRun ();
            }
        }

        /**************************************************************************************************************/
        /* Water Level Groups                                                                                         */
        /**************************************************************************************************************/
        public static void AddWaterLevelGroup (WaterLevelGroupSettings settings, bool saveToFile = true) {
            if (!WaterLevelGroupNameOk (settings.name)) {
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
        
        public static void AddWaterLevelGroup (
            string name,
            float highAnalogAlarmSetpoint,
            bool enableHighAnalogAlarm,
            float lowAnalogAlarmSetpoint,
            bool enableLowAnalogAlarm,
            IEnumerable<string> floatSwitches,
            IEnumerable<string> waterLevelSensors)
        {
            if (!WaterLevelGroupNameOk (name)) {
                throw new Exception (string.Format ("Water Level Group {0} already exists", name));
            }

            waterLevelGroups[name] = new WaterLevelGroup (
                name,
                highAnalogAlarmSetpoint,
                enableHighAnalogAlarm,
                lowAnalogAlarmSetpoint,
                enableLowAnalogAlarm,
                floatSwitches,
                waterLevelSensors);
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
            SettingsHelper.DeleteSettingsFromArray (settingsFileName, groupSettingsArrayName, name);
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

        public static bool WaterLevelGroupNameOk (string name) {
            return !CheckWaterLevelGroupKeyNoThrow (name);
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

        public static bool GetWaterLevelGroupSwitchesActivated (string name, SwitchFunction function = SwitchFunction.ATO) {
            CheckWaterLevelGroupKey (name);
            bool activated = true;
            foreach (var switchName in waterLevelGroups[name].floatSwitches) {
                var floatSwitch = AquaPicSensors.FloatSwitches.GetSensor (switchName) as FloatSwitch;
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
            foreach (var sensorName in waterLevelGroups[name].waterLevelSensors) {
                var sensor = AquaPicSensors.WaterLevelSensors.GetSensor (sensorName) as WaterLevelSensor;
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
            return settings;
        }

        protected static void AddWaterLevelGroupSettingsToFile (string name) {
            CheckWaterLevelGroupKey (name);
            SettingsHelper.AddSettingsToArray (settingsFileName, groupSettingsArrayName, GetWaterLevelGroupSettings (name));
        }

        protected static void DeleteWaterLevelGroupSettingsFromFile (string name) {
            SettingsHelper.DeleteSettingsFromArray (settingsFileName, groupSettingsArrayName, name);
        }

        /**************************************************************************************************************/
        /* Analog Level Sensor                                                                                        */
        /**************************************************************************************************************/
        public static void AddWaterLevelSensorToWaterLevelGroup (string groupName, WaterLevelSensorSettings settings, bool saveToFile) {
            CheckWaterLevelGroupKey (groupName);
            waterLevelGroups[groupName].waterLevelSensors.Add (settings.name);
            AquaPicSensors.WaterLevelSensors.AddSensor (settings, saveToFile);
        }

        public static void UpdateWaterLevelSensorInWaterLevelGroup (string groupName, string waterLevelSensorName, WaterLevelSensorSettings settings) {
            CheckWaterLevelGroupKey (groupName);
            waterLevelGroups[groupName].waterLevelSensors.Remove (waterLevelSensorName);
            waterLevelGroups[groupName].waterLevelSensors.Add (settings.name);
            AquaPicSensors.WaterLevelSensors.UpdateSensor (waterLevelSensorName, settings);
        }

        public static void RemoveWaterLevelSensorFromWaterLevelGroup (string groupName, string waterLevelSensorName) {
            CheckWaterLevelGroupKey (groupName);
            waterLevelGroups[groupName].waterLevelSensors.Remove (waterLevelSensorName);
            AquaPicSensors.WaterLevelSensors.RemoveSensor (waterLevelSensorName);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllWaterLevelSensorsForWaterLevelGroup (string groupName) {
            CheckWaterLevelGroupKey (groupName);
            List<string> names = new List<string> ();
            foreach (var sensorName in waterLevelGroups[groupName].waterLevelSensors) {
                names.Add (sensorName);
            }
            return names.ToArray ();
        }

        /***Individual Control***/
        public static IndividualControl GetAnalogLevelSensorIndividualControl (string waterLevelSensorName) {
            return AquaPicSensors.WaterLevelSensors.GetSensor (waterLevelSensorName).channel;
        }

        /***Level***/
        public static float GetAnalogLevelSensorLevel (string waterLevelSensorName) {
            var waterLevelSensor = AquaPicSensors.WaterLevelSensors.GetSensor (waterLevelSensorName) as WaterLevelSensor;
            return waterLevelSensor.level;
        }

        /***Connected***/
        public static bool GetAnalogLevelSensorConnected (string waterLevelSensorName) {
            var waterLevelSensor = AquaPicSensors.WaterLevelSensors.GetSensor (waterLevelSensorName) as WaterLevelSensor;
            return waterLevelSensor.connected;
        }

        /***Disconnected alarm index***/
        public static int GetAnalogLevelSensorDisconnectedAlarmIndex (string waterLevelSensorName) {
            var waterLevelSensor = AquaPicSensors.WaterLevelSensors.GetSensor (waterLevelSensorName) as WaterLevelSensor;
            return waterLevelSensor.sensorDisconnectedAlarmIndex;
        }

        /***Zero scale value***/
        public static float GetAnalogLevelSensorZeroScaleValue (string waterLevelSensorName) {
            var waterLevelSensor = AquaPicSensors.WaterLevelSensors.GetSensor (waterLevelSensorName) as WaterLevelSensor;
            return waterLevelSensor.zeroScaleValue;
        }

        /***Full scale actual***/
        public static float GetAnalogLevelSensorFullScaleActual (string waterLevelSensorName) {
            var waterLevelSensor = AquaPicSensors.WaterLevelSensors.GetSensor (waterLevelSensorName) as WaterLevelSensor;
            return waterLevelSensor.fullScaleActual;
        }

        /***Full scale value***/
        public static float GetAnalogLevelSensorFullScaleValue (string waterLevelSensorName) {
            var waterLevelSensor = AquaPicSensors.WaterLevelSensors.GetSensor (waterLevelSensorName) as WaterLevelSensor;
            return waterLevelSensor.fullScaleValue;
        }

        /***Setters****************************************************************************************************/
        /***Calibration data***/
        public static bool SetCalibrationData (string name, float zeroScaleValue, float fullScaleActual, float fullScaleValue) {
            AquaPicSensors.WaterLevelSensors.SetCalibrationData (name, zeroScaleValue, fullScaleActual, fullScaleValue);
            return true;
        }

        /**************************************************************************************************************/
        /* Float Switches                                                                                             */
        /**************************************************************************************************************/
        public static void AddFloatSwitchToWaterLevelGroup (string groupName, FloatSwitchSettings settings, bool saveToFile) {
            CheckWaterLevelGroupKey (groupName);
            waterLevelGroups[groupName].floatSwitches.Add (settings.name);
            AquaPicSensors.FloatSwitches.AddSensor (settings, saveToFile);
        }

        public static void UpdateFloatSwitchInWaterLevelGroup (string groupName, string floatSwitchName, FloatSwitchSettings settings) {
            CheckWaterLevelGroupKey (groupName);
            waterLevelGroups[groupName].floatSwitches.Remove (floatSwitchName);
            waterLevelGroups[groupName].floatSwitches.Add (settings.name);
            AquaPicSensors.FloatSwitches.UpdateSensor (floatSwitchName, settings);
        }

        public static void RemoveFloatSwitchFromWaterLevelGroup (string groupName, string floatSwitchName) {
            CheckWaterLevelGroupKey (groupName);
            waterLevelGroups[groupName].floatSwitches.Remove (floatSwitchName);
            AquaPicSensors.FloatSwitches.RemoveSensor (floatSwitchName);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllFloatSwitchesForWaterLevelGroup (string groupName) {
            CheckWaterLevelGroupKey (groupName);
            List<string> names = new List<string> ();
            foreach (var sensorName in waterLevelGroups[groupName].floatSwitches) {
                names.Add (sensorName);
            }
            return names.ToArray ();
        }

        /***Individual Control***/
        public static IndividualControl GetFloatSwitchIndividualControl (string floatSwitchName) {
            return AquaPicSensors.FloatSwitches.GetSensor (floatSwitchName).channel;
        }

        /***State***/
        public static bool GetFloatSwitchState (string floatSwitchName) {
            var floatSwitch = AquaPicSensors.FloatSwitches.GetSensor (floatSwitchName) as FloatSwitch;
            return floatSwitch.activated;
        }

        /***Type***/
        public static SwitchType GetFloatSwitchType (string floatSwitchName) {
            var floatSwitch = AquaPicSensors.FloatSwitches.GetSensor (floatSwitchName) as FloatSwitch;
            return floatSwitch.switchType;
        }

        /***Function***/
        public static SwitchFunction GetFloatSwitchFunction (string floatSwitchName) {
            var floatSwitch = AquaPicSensors.FloatSwitches.GetSensor (floatSwitchName) as FloatSwitch;
            return floatSwitch.switchFuntion;
        }

        /***Physical Level***/
        public static float GetFloatSwitchPhysicalLevel (string floatSwitchName) {
            var floatSwitch = AquaPicSensors.FloatSwitches.GetSensor (floatSwitchName) as FloatSwitch;
            return floatSwitch.physicalLevel;
        }

        /***Time Offset***/
        public static uint GetFloatSwitchTimeOffset (string floatSwitchName) {
            var floatSwitch = AquaPicSensors.FloatSwitches.GetSensor (floatSwitchName) as FloatSwitch;
            return floatSwitch.timeOffset;
        }
    }
}

