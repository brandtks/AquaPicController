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

namespace AquaPic.Modules
{
    public enum AutoTopOffState
    {
        Off,
        Standby,
        Filling,
        Cooldown,
        Error
    }

    public partial class AutoTopOff
    {
        private static Dictionary<string, AutoTopOffGroup> atoGroups;

        public static int atoGroupCount {
            get {
                return atoGroups.Count;
            }
        }

        public static string firstAtoGroup {
            get {
                if (atoGroups.Count > 0) {
                    var first = atoGroups.First ();
                    return first.Key;
                }

                return string.Empty;
            }
        }

        const string settingsFile = "autoTopOffProperties";
        const string settingsArrayName = "atoGroups";

        static AutoTopOff () { }

        public static void Init () {
            Logger.Add ("Initializing Auto Top Off");

            atoGroups = new Dictionary<string, AutoTopOffGroup> ();

            if (SettingsHelper.SettingsFileExists (settingsFile)) {
                var settings = SettingsHelper.ReadAllSettingsInArray<AutoTopOffGroupSettings> (settingsFile, settingsArrayName);
                foreach (var setting in settings) {
                    AddAtoGroup (setting, false);
                }
            } else {
                Logger.Add ("ATO settings file did not exist, created new ATO settings");

                var jo = new JObject ();
                jo.Add (new JProperty (settingsArrayName, new JArray ()));

                SettingsHelper.WriteSettingsFile (settingsFile, jo);
            }

            TaskManager.AddCyclicInterrupt ("Auto Top Off", 1000, Run);
        }

        public static void Run () {
            foreach (var group in atoGroups.Values) {
                group.GroupRun ();
            }
        }

        /**************************************************************************************************************/
        /* Auto Top Off Groups                                                                                        */
        /**************************************************************************************************************/
        public static void AddAtoGroup (AutoTopOffGroupSettings settings, bool saveToFile = true) {
            if (!AtoGroupNameOk (settings.name)) {
                throw new Exception (string.Format ("ATO Group: {0} already exists", settings.name));
            }

            atoGroups[settings.name] = new AutoTopOffGroup (
                settings.name,
                settings.enable,
                settings.requestBitName,
                settings.waterLevelGroupName,
                settings.maximumRuntime,
                settings.minimumCooldown,
                settings.useAnalogSensors,
                settings.analogOnSetpoint,
                settings.analogOffSetpoint,
                settings.useFloatSwitches);

            if (saveToFile) {
                AddAutoTopOffGroupSettingsToFile (settings);
            }
        }

        public static void AddAtoGroup (
            string name,
            bool enable,
            string requestBitName,
            string waterLevelGroupName,
            uint maximumRuntime,
            uint minimumCooldown,
            bool useAnalogSensors,
            float analogOnSetpoint,
            float analogOffSetpoint,
            bool useFloatSwitches)
        {
            if (!AtoGroupNameOk (name)) {
                throw new Exception (string.Format ("ATO Group: {0} already exists", name));
            }

            var atoGroup = new AutoTopOffGroup (
                name,
                enable,
                requestBitName,
                waterLevelGroupName,
                maximumRuntime,
                minimumCooldown,
                useAnalogSensors,
                analogOnSetpoint,
                analogOffSetpoint,
                useFloatSwitches);

            atoGroups.Add (name, atoGroup);
        }

        public static void UpdateAtoGroup (string name, AutoTopOffGroupSettings settings) {
            if (CheckAtoGroupKeyNoThrow (name)) {
                RemoveAtoGroup (name);
            }
            AddAtoGroup (settings);
        }

        public static void RemoveAtoGroup (string name) {
            CheckAtoGroupKey (name);
            atoGroups.Remove (name);
            DeleteAutoTopOffGroupSettingsFromFile (name);
        }

        public static void CheckAtoGroupKey (string name) {
            if (!atoGroups.ContainsKey (name)) {
                throw new ArgumentException ("name");
            }
        }

        public static bool CheckAtoGroupKeyNoThrow (string name) {
            try {
                CheckAtoGroupKey (name);
                return true;
            } catch {
                return false;
            }
        }

        public static bool AtoGroupNameOk (string name) {
            return !CheckAtoGroupKeyNoThrow (name);
        }

        public static bool ClearAtoAlarm (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].ClearAlarm ();
        }

        public static void ResetCooldownTime (string name) {
            CheckAtoGroupKey (name);
            atoGroups[name].ResetCooldown ();
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllAtoGroupNames () {
            List<string> names = new List<string> ();
            foreach (var waterLevelGroup in atoGroups.Values) {
                names.Add (waterLevelGroup.name);
            }
            return names.ToArray ();
        }

        /***Enable***/
        public static bool GetAtoGroupEnable (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].enable;
        }

        /***State***/
        public static AutoTopOffState GetAtoGroupState (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].state;
        }

        /***Request Bit Name***/
        public static string GetAtoGroupRequestBitName (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].requestBitName;
        }

        /***Water Level Group Name***/
        public static string GetAtoGroupWaterLevelGroupName (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].waterLevelGroupName;
        }

        /***ATO Time***/
        public static uint GetAtoGroupAtoTime (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].atoTime;
        }

        /***Maximum Runtime***/
        public static uint GetAtoGroupMaximumRuntime (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].maximumRuntime;
        }

        /***Minimum Cooldown***/
        public static uint GetAtoGroupMinimumCooldown (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].minimumCooldown;
        }

        /***Use Analog Sensor***/
        public static bool GetAtoGroupUseAnalogSensor (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].useAnalogSensors;
        }

        /***Analog On Setpoint***/
        public static float GetAtoGroupAnalogOnSetpoint (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].analogOnSetpoint;
        }

        /***Analog Off Setpoint***/
        public static float GetAtoGroupAnalogOffSetpoint (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].analogOffSetpoint;
        }

        /***Use Float Switches***/
        public static bool GetAtoGroupUseFloatSwitches (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].useFloatSwitches;
        }

        /***Failed Alarm Index***/
        public static int GetAtoGroupFailAlarmIndex (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].failAlarmIndex;
        }

        /***Setters****************************************************************************************************/
        /***Names***/
        public static void SetAtoGroupName (string oldName, string newName) {
            CheckAtoGroupKey (oldName);
            if (!AtoGroupNameOk (newName)) {
                throw new Exception (string.Format ("ATO Group: {0} already exists", newName));
            }

            var atoGroup = atoGroups[oldName];
            atoGroups[oldName].name = newName;
            atoGroups.Remove (oldName);
            atoGroups.Add (newName, atoGroup);
        }

        /***Enable***/
        public static void SetAtoGroupEnable (string name, bool enable) {
            CheckAtoGroupKey (name);
            atoGroups[name].enable = enable;
        }

        /***Request Bit Name***/
        public static void SetAtoGroupRequestBitName (string name, string requestBitName) {
            CheckAtoGroupKey (name);
            var state = Bit.Get (atoGroups[name].requestBitName);
            Bit.Remove (atoGroups[name].requestBitName);
            atoGroups[name].requestBitName = requestBitName;
            if (state == MyState.Set) {
                Bit.Set (atoGroups[name].requestBitName);
            } else {
                Bit.Reset (atoGroups[name].requestBitName);
            }
        }

        /***Water Level Group Name***/
        public static void SetAtoGroupWaterLevelGroupName (string name, string waterLevelGroupName) {
            CheckAtoGroupKey (name);
            atoGroups[name].waterLevelGroupName = waterLevelGroupName;
        }

        /***Maximum Runtime***/
        public static void SetAtoGroupMaximumRuntime (string name, uint maximumRuntime) {
            CheckAtoGroupKey (name);
            atoGroups[name].maximumRuntime = maximumRuntime;
        }

        /***Minimum Cooldown***/
        public static void SetAtoGroupMinimumCooldown (string name, uint minimumCooldown) {
            CheckAtoGroupKey (name);
            atoGroups[name].minimumCooldown = minimumCooldown;
        }

        /***Use Analog Sensor***/
        public static void SetAtoGroupUseAnalogSensor (string name, bool useAnalogSensors) {
            CheckAtoGroupKey (name);
            atoGroups[name].useAnalogSensors = useAnalogSensors;
        }

        /***Analog On Setpoint***/
        public static void SetAtoGroupAnalogOnSetpoint (string name, float analogOnSetpoint) {
            CheckAtoGroupKey (name);
            atoGroups[name].analogOnSetpoint = analogOnSetpoint;
        }

        /***Analog Off Setpoint***/
        public static void SetAtoGroupAnalogOffSetpoint (string name, float analogOffSetpoint) {
            CheckAtoGroupKey (name);
            atoGroups[name].analogOffSetpoint = analogOffSetpoint;
        }

        /***Use Float Switches***/
        public static void SetAtoGroupUseFloatSwitches (string name, bool useFloatSwitches) {
            CheckAtoGroupKey (name);
            atoGroups[name].useFloatSwitches = useFloatSwitches;
        }

        /***Settings***************************************************************************************************/
        public static AutoTopOffGroupSettings GetAutoTopOffGroupSettings (string name) {
            CheckAtoGroupKey (name);
            var settings = new AutoTopOffGroupSettings ();
            settings.name = name;
            settings.enable = GetAtoGroupEnable (name);
            settings.requestBitName = GetAtoGroupRequestBitName (name);
            settings.waterLevelGroupName = GetAtoGroupWaterLevelGroupName (name);
            settings.maximumRuntime = GetAtoGroupMaximumRuntime (name);
            settings.minimumCooldown = GetAtoGroupMinimumCooldown (name);
            settings.useAnalogSensors = GetAtoGroupUseAnalogSensor (name);
            settings.analogOnSetpoint = GetAtoGroupAnalogOnSetpoint (name);
            settings.analogOffSetpoint = GetAtoGroupAnalogOffSetpoint (name);
            settings.useFloatSwitches = GetAtoGroupUseFloatSwitches (name);
            return settings;
        }

        protected static void AddAutoTopOffGroupSettingsToFile (AutoTopOffGroupSettings settings) {
            SettingsHelper.AddSettingsToArray (settingsFile, settingsArrayName, settings);
        }

        protected static void DeleteAutoTopOffGroupSettingsFromFile (string name) {
            SettingsHelper.DeleteSettingsFromArray (settingsFile, settingsArrayName, name);
        }
    }
}

