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
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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
        static Dictionary<string, WaterLevelSensor> analogLevelSensors;
        static Dictionary<string, FloatSwitch> floatSwitches;

        /**************************************************************************************************************/
        /* Analog Water Sensors                                                                                       */
        /**************************************************************************************************************/
        public static int analogLevelSensorCount {
            get {
                return analogLevelSensors.Count;
            }
        }

        public static string firstAnalogLevelSensor {
            get {
                if (analogLevelSensors.Count > 0) {
                    var first = analogLevelSensors.First ();
                    return first.Key;
                }

                return string.Empty;
            }
        }

        /**************************************************************************************************************/
        /* Float Switches                                                                                             */
        /**************************************************************************************************************/
        public static int floatSwitchCount {
            get {
                return floatSwitches.Count;
            }
        }

        public static string firstFloatSwitch {
            get {
                if (floatSwitches.Count > 0) {
                    var first = floatSwitches.First ();
                    return first.Key;
                } else {
                    return string.Empty;
                }
            }
        }

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

        const string settingsFile = "waterLevelProperties";
        const string groupSettingsArrayName = "waterLevelGroups";
        const string analogSensorSettingsArrayName = "analogSensors";
        const string floatSwitchSettingsArrayName = "floatSwitches";

        /**************************************************************************************************************/
        /* Water Level                                                                                                */
        /**************************************************************************************************************/
        static WaterLevel () { }

        public static void Init () {
            Logger.Add ("Initializing Water Level");

            waterLevelGroups = new Dictionary<string, WaterLevelGroup> ();
            analogLevelSensors = new Dictionary<string, WaterLevelSensor> ();
            floatSwitches = new Dictionary<string, FloatSwitch> ();

            if (SettingsHelper.SettingsFileExists (settingsFile)) {
                /**************************************************************************************************/
                /* Water Level Groups                                                                             */
                /**************************************************************************************************/
                var groupSettings = SettingsHelper.ReadAllSettingsInArray<WaterLevelGroupSettings> (settingsFile, groupSettingsArrayName);
                foreach (var setting in groupSettings) {
                    AddWaterLevelGroup (setting, false);
                }

                /**************************************************************************************************/
                /* Analog Sensors                                                                                 */
                /**************************************************************************************************/
                var analogSettings = SettingsHelper.ReadAllSettingsInArray<WaterLevelSensorSettings> (settingsFile, analogSensorSettingsArrayName);
                foreach (var setting in analogSettings) {
                    AddAnalogLevelSensor (setting, false);
                }

                /**************************************************************************************************/
                /* Float Switches                                                                                 */
                /**************************************************************************************************/
                var switchSettings = SettingsHelper.ReadAllSettingsInArray<FloatSwitchSettings> (settingsFile, floatSwitchSettingsArrayName);
                foreach (var setting in switchSettings) {
                    AddFloatSwitch (setting, false);
                }
            } else {
                Logger.Add ("Water level settings file did not exist, created new water level settings");

                var jo = new JObject ();
                jo.Add (new JProperty (groupSettingsArrayName, new JArray ()));
                jo.Add (new JProperty ("defaultWaterLevelGroup", string.Empty));
                jo.Add (new JProperty (analogSensorSettingsArrayName, new JArray ()));
                jo.Add (new JProperty (floatSwitchSettingsArrayName, new JArray ()));

                SettingsHelper.WriteSettingsFile (settingsFile, jo);
            }

            TaskManager.AddCyclicInterrupt ("Water Level", 1000, Run);
        }

        public static void Run () {
            foreach (var waterLevelGroup in waterLevelGroups.Values) {
                waterLevelGroup.GroupRun ();
            }

            // Get the value of the all the analog sensors no assigned to a group
            foreach (var analogSensor in analogLevelSensors.Values) {
                if (!CheckWaterLevelGroupKeyNoThrow (analogSensor.waterLevelGroupName)) {
                    analogSensor.Get ();
                }
            }

            // Get the value of the all the float switches no assigned to a group
            foreach (var floatSwitch in floatSwitches.Values) {
                if (!CheckWaterLevelGroupKeyNoThrow (floatSwitch.waterLevelGroupName)) {
                    floatSwitch.Get ();
                }
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
                settings.enableLowAnalogAlarm);

            if (saveToFile) {
                AddWaterLevelGroupSettingsToFile (settings.name);
            }
        }
        
        public static void AddWaterLevelGroup (
            string name,
            float highAnalogAlarmSetpoint,
            bool enableHighAnalogAlarm,
            float lowAnalogAlarmSetpoint,
            bool enableLowAnalogAlarm)
        {
            if (!WaterLevelGroupNameOk (name)) {
                throw new Exception (string.Format ("Water Level Group {0} already exists", name));
            }

            waterLevelGroups[name] = new WaterLevelGroup (
                name,
                highAnalogAlarmSetpoint,
                enableHighAnalogAlarm,
                lowAnalogAlarmSetpoint,
                enableLowAnalogAlarm);
        }

        public static void UpdateWaterLevelGroup (string name, WaterLevelGroupSettings settings) {
            if (!CheckWaterLevelGroupKeyNoThrow (name)) {
                RemoveWaterLevelGroup (name);
            }

            AddWaterLevelGroup (settings);

            foreach (var floatSwitch in floatSwitches.Values) {
                if (floatSwitch.waterLevelGroupName == name) {
                    floatSwitch.waterLevelGroupName = settings.name;
                    UpdateFloatSwitchSettingsInFile (floatSwitch.name);
                }
            }

            foreach (var sensor in analogLevelSensors.Values) {
                if (sensor.waterLevelGroupName == name) {
                    sensor.waterLevelGroupName = settings.name;
                    UpdateAnalogSensorSettingsInFile (sensor.name);
                }
            }
        }

        public static void RemoveWaterLevelGroup (string name) {
            CheckWaterLevelGroupKey (name);
            waterLevelGroups.Remove (name);
            SettingsHelper.DeleteSettingsFromArray (settingsFile, groupSettingsArrayName, name);
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
            foreach (var s in floatSwitches.Values) {
                if ((s.waterLevelGroupName == name) && (s.function == function)) {
                    // Using AND because we want all the switches to be activated
                    activated &= s.activated;
                }
            }
            return activated;
        }

        /***Analog sensors connected***/
        public static bool GetWaterLevelGroupAnalogSensorConnected (string name) {
            CheckWaterLevelGroupKey (name);
            bool connected = false;
            foreach (var sensor in analogLevelSensors.Values) {
                if (sensor.waterLevelGroupName == name) {
                    // Using OR because we really only care that at least one sensor is connected
                    connected |= sensor.connected;
                }
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
            SettingsHelper.AddSettingsToArray (settingsFile, groupSettingsArrayName, GetWaterLevelGroupSettings (name));
        }

        protected static void DeleteWaterLevelGroupSettingsFromFile (string name) {
            SettingsHelper.DeleteSettingsFromArray (settingsFile, groupSettingsArrayName, name);
        }

        /**************************************************************************************************************/
        /* Analog Level Sensor                                                                                        */
        /**************************************************************************************************************/
        public static void AddAnalogLevelSensor (WaterLevelSensorSettings settings, bool saveToFile = true) {
            if (!AnalogLevelSensorNameOk (settings.name)) {
                throw new Exception (string.Format ("Analog Sensor: {0} already exists", settings.name));
            }

            analogLevelSensors[settings.name] = new WaterLevelSensor (
                settings.name,
                settings.channel,
                settings.waterLevelGroupName,
                settings.zeroScaleCalibrationValue,
                settings.fullScaleCalibrationActual,
                settings.fullScaleCalibrationValue);

            if (saveToFile) {
                AddAnalogSensorSettingsToFile (settings);
            }
        }

        public static void UpdateAnalogLevelSensor (string analogLevelSensorName, WaterLevelSensorSettings settings) {
            if (!CheckAnalogLevelSensorKeyNoThrow (analogLevelSensorName)) {
                settings.zeroScaleCalibrationValue = GetAnalogLevelSensorZeroScaleValue (analogLevelSensorName);
                settings.fullScaleCalibrationActual = GetAnalogLevelSensorFullScaleActual (analogLevelSensorName);
                settings.fullScaleCalibrationValue = GetAnalogLevelSensorFullScaleValue (analogLevelSensorName);
                RemoveAnalogLevelSensor (analogLevelSensorName);
            }

            AddAnalogLevelSensor (settings);
        }

        public static void RemoveAnalogLevelSensor (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);

            analogLevelSensors[analogLevelSensorName].Remove ();
            analogLevelSensors.Remove (analogLevelSensorName);
            DeleteAnalogSensorSettingFromFile (analogLevelSensorName);
        }

        public static void CheckAnalogLevelSensorKey (string analogLevelSensorName) {
            if (!analogLevelSensors.ContainsKey (analogLevelSensorName)) {
                throw new ArgumentException ("analogLevelSensorName");
            }
        }

        public static bool CheckAnalogLevelSensorKeyNoThrow (string analogLevelSensorName) {
            try {
                CheckAnalogLevelSensorKey (analogLevelSensorName);
                return true;
            } catch {
                return false;
            }
        }

        public static bool AnalogLevelSensorNameOk (string analogLevelSensorName) {
            return !CheckAnalogLevelSensorKeyNoThrow (analogLevelSensorName);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllAnalogLevelSensors () {
            List<string> names = new List<string> ();
            foreach (var analogLevelSensor in analogLevelSensors.Values) {
                names.Add (analogLevelSensor.name);
            }
            return names.ToArray ();
        }

        /***Individual Control***/
        public static IndividualControl GetAnalogLevelSensorIndividualControl (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].channel;
        }

        /***Level***/
        public static float GetAnalogLevelSensorLevel (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].level;
        }

        /***Water Level Group Name***/
        public static string GetAnalogLevelSensorWaterLevelGroupName (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].waterLevelGroupName;
        }

        /***Connected***/
        public static bool GetAnalogLevelSensorConnected (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].connected;
        }

        /***Disconnected alarm index***/
        public static int GetAnalogLevelSensorDisconnectedAlarmIndex (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].sensorDisconnectedAlarmIndex;
        }

        /***Zero scale value***/
        public static float GetAnalogLevelSensorZeroScaleValue (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].zeroScaleValue;
        }

        /***Full scale actual***/
        public static float GetAnalogLevelSensorFullScaleActual (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].fullScaleActual;
        }

        /***Full scale value***/
        public static float GetAnalogLevelSensorFullScaleValue (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].fullScaleValue;
        }

        /***Setters****************************************************************************************************/
        /***Calibration data***/
        public static bool SetCalibrationData (string name, float zeroValue, float fullScaleActual, float fullScaleValue) {
            CheckAnalogLevelSensorKey (name);

            if (fullScaleValue <= zeroValue)
                throw new ArgumentException ("Full scale value can't be less than or equal to zero value");

            if (fullScaleActual < 0.0f)
                throw new ArgumentException ("Full scale actual can't be less than zero");

            analogLevelSensors[name].zeroScaleValue = zeroValue;
            analogLevelSensors[name].fullScaleActual = fullScaleActual;
            analogLevelSensors[name].fullScaleValue = fullScaleValue;

            UpdateAnalogSensorSettingsInFile (name);

            return true;
        }

        /***Settings***************************************************************************************************/
        public static WaterLevelSensorSettings GetAnalogSensorSettingsSettings (string name) {
            CheckAnalogLevelSensorKey (name);
            var settings = new WaterLevelSensorSettings ();
            settings.name = name;
            settings.channel = GetAnalogLevelSensorIndividualControl (name);
            settings.waterLevelGroupName = GetAnalogLevelSensorWaterLevelGroupName (name);
            settings.zeroScaleCalibrationValue = GetAnalogLevelSensorZeroScaleValue (name);
            settings.fullScaleCalibrationActual = GetAnalogLevelSensorFullScaleActual (name);
            settings.fullScaleCalibrationValue = GetAnalogLevelSensorFullScaleValue (name);
            return settings;
        }

        protected static void AddAnalogSensorSettingsToFile (WaterLevelSensorSettings settings) {
            SettingsHelper.AddSettingsToArray (settingsFile, analogSensorSettingsArrayName, settings);
        }

        protected static void UpdateAnalogSensorSettingsInFile (string name) {
            SettingsHelper.UpdateSettingsInArray (settingsFile, analogSensorSettingsArrayName, name, GetAnalogSensorSettingsSettings (name));
        }

        protected static void DeleteAnalogSensorSettingFromFile (string name) {
            SettingsHelper.DeleteSettingsFromArray (settingsFile, analogSensorSettingsArrayName, name);
        }

        /**************************************************************************************************************/
        /* Float Switches                                                                                             */
        /**************************************************************************************************************/
        public static void AddFloatSwitch (FloatSwitchSettings settings, bool saveToFile = true) {
            if (!FloatSwitchNameOk (settings.name)) {
                throw new Exception (string.Format ("Float Switch: {0} already exists", settings.name));
            }

            if ((settings.switchFuntion == SwitchFunction.HighLevel) && (settings.switchType != SwitchType.NormallyClosed)) {
                Logger.AddWarning ("High level switch should be normally closed");
            } else if ((settings.switchFuntion == SwitchFunction.LowLevel) && (settings.switchType != SwitchType.NormallyClosed)) {
                Logger.AddWarning ("Low level switch should be normally closed");
            } else if ((settings.switchFuntion == SwitchFunction.ATO) && (settings.switchType != SwitchType.NormallyOpened)) {
                Logger.AddWarning ("ATO switch should be normally opened");
            }

            floatSwitches[settings.name] = new FloatSwitch (
                settings.name,
                settings.switchType,
                settings.switchFuntion,
                settings.physicalLevel,
                settings.channel,
                settings.timeOffset,
                settings.waterLevelGroupName);

            if (saveToFile) {
                AddFloatSwitchSettingsToFile (settings);
            }
        }

        public static void UpdateFloatSwitch (string floatSwitchName, FloatSwitchSettings settings) {
            if (!CheckFloatSwitchKeyNoThrow (floatSwitchName)) {
                RemoveFloatSwitch (floatSwitchName);
            }
            AddFloatSwitch (settings);
        }

        public static void RemoveFloatSwitch (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].Remove ();   // this removes the physical digital input
            floatSwitches.Remove (floatSwitchName);     // this removes the entry from the dictionary
            SettingsHelper.DeleteSettingsFromArray (settingsFile, floatSwitchSettingsArrayName, floatSwitchName);
        }

        public static void CheckFloatSwitchKey (string floatSwitchName) {
            if (!floatSwitches.ContainsKey (floatSwitchName)) {
                throw new ArgumentException ("floatSwitchName");
            }
        }

        public static bool CheckFloatSwitchKeyNoThrow (string floatSwitchName) {
            try {
                CheckFloatSwitchKey (floatSwitchName);
                return true;
            } catch {
                return false;
            }
        }

        public static bool FloatSwitchNameOk (string floatSwitchName) {
            return !CheckFloatSwitchKeyNoThrow (floatSwitchName);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllFloatSwitches () {
            List<string> names = new List<string> ();
            foreach (var floatSwitch in floatSwitches.Values) {
                names.Add (floatSwitch.name);
            }
            return names.ToArray ();
        }

        /***Individual Control***/
        public static IndividualControl GetFloatSwitchIndividualControl (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].channel;
        }

        /***State***/
        public static bool GetFloatSwitchState (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].activated;
        }

        /***Water level group name***/
        public static string GetFloatSwitchWaterLevelGroupName (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].waterLevelGroupName;
        }

        /***Type***/
        public static SwitchType GetFloatSwitchType (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].type;
        }

        /***Function***/
        public static SwitchFunction GetFloatSwitchFunction (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].function;
        }

        /***Physical Level***/
        public static float GetFloatSwitchPhysicalLevel (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].physicalLevel;
        }

        /***Time Offset***/
        public static uint GetFloatSwitchTimeOffset (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].onDelayTimer.timerInterval;
        }

        /***Settings***************************************************************************************************/
        public static FloatSwitchSettings GetFloatSwitchSettingsSettings (string name) {
            CheckFloatSwitchKey (name);
            var settings = new FloatSwitchSettings ();
            settings.name = name;
            settings.channel = GetFloatSwitchIndividualControl (name);
            settings.physicalLevel = GetFloatSwitchPhysicalLevel (name);
            settings.switchType = GetFloatSwitchType (name);
            settings.switchFuntion = GetFloatSwitchFunction (name);
            settings.timeOffset = GetFloatSwitchTimeOffset (name);
            settings.waterLevelGroupName = GetFloatSwitchWaterLevelGroupName (name);
            return settings;
        }

        protected static void AddFloatSwitchSettingsToFile (FloatSwitchSettings settings) {
            SettingsHelper.AddSettingsToArray (settingsFile, floatSwitchSettingsArrayName, settings);
        }

        protected static void UpdateFloatSwitchSettingsInFile (string name) {
            SettingsHelper.UpdateSettingsInArray (settingsFile, floatSwitchSettingsArrayName, name, GetFloatSwitchSettingsSettings (name));
        }

        protected static void DeleteFloatSwitchSettingsFromFile (string name) {
            SettingsHelper.DeleteSettingsFromArray (settingsFile, floatSwitchSettingsArrayName, name);
        }
    }
}

