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
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;
using AquaPic.Drivers;
using AquaPic.Globals;
using AquaPic.Gadgets;
using AquaPic.Gadgets.TemperatureProbe;
using AquaPic.DataLogging;

namespace AquaPic.Modules.Temperature
{
    public partial class Temperature
    {
        private static Dictionary<string, TemperatureGroup> temperatureGroups;
        private static string _defaultTemperatureGroup;

        /**************************************************************************************************************/
        /* Default temperature properties                                                                             */
        /**************************************************************************************************************/
        public static int temperatureGroupCount {
            get {
                return temperatureGroups.Count;
            }
        }

        public static string defaultTemperatureGroup {
            get {
                if (_defaultTemperatureGroup.IsEmpty () && (temperatureGroupCount > 0)) {
                    var first = temperatureGroups.First ();
                    _defaultTemperatureGroup = first.Key;
                }

                return _defaultTemperatureGroup;
            }
        }

        public static float defaultTemperature {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].temperature;
                }

                return 0.0f;
            }
        }

        public static float defaultTemperatureSetpoint {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].temperatureSetpoint;
                }

                return 0.0f;
            }
        }

        public static float defaultTemperatureDeadband {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].temperatureDeadband;
                }

                return 0.0f;
            }
        }

        public static float defaultHighTemperatureAlarmSetpoint {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].highTemperatureAlarmSetpoint;
                }

                return 0.0f;
            }
        }

        public static float defaultLowTemperatureAlarmSetpoint {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].lowTemperatureAlarmSetpoint;
                }

                return 0.0f;
            }
        }

        public static int defaultHighTemperatureAlarmIndex {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].highTemperatureAlarmIndex;
                }

                return -1;
            }
        }

        public static int defaultLowTemperatureAlarmIndex {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].lowTemperatureAlarmIndex;
                } else {
                    return -1;
                }
            }
        }

        const string settingsFileName = "temperature";
        const string groupSettingsArrayName = "temperatureGroups";

        static Temperature () { }

        public static void Init () {
            Logger.Add ("Initializing Temperature");

            temperatureGroups = new Dictionary<string, TemperatureGroup> ();

            if (SettingsHelper.SettingsFileExists (settingsFileName)) {
                var groupSettings = SettingsHelper.ReadAllSettingsInArray<TemperatureGroupSettings> (settingsFileName, groupSettingsArrayName);
                foreach (var setting in groupSettings) {
                    AddTemperatureGroup (setting, false);
                }
            } else {
                Logger.Add ("Temperature settings file did not exist, created new temperature settings");

                var jo = new JObject ();
                jo.Add (new JProperty ("temperatureGroups", new JArray ()));
                jo.Add (new JProperty ("defaultTemperatureGroup", string.Empty));

                SettingsHelper.WriteSettingsFile (settingsFileName, jo);
            }

            TaskManager.AddCyclicInterrupt ("Temperature", 1000, Run);
        }

        public static void Run () {
            foreach (var tempGroup in temperatureGroups.Values) {
                tempGroup.GroupRun ();
            }
        }

        /**************************************************************************************************************/
        /* Temperature Groups                                                                                         */
        /**************************************************************************************************************/
        public static void AddTemperatureGroup (TemperatureGroupSettings settings, bool saveToFile = true) {
            if (TemperatureGroupNameExists (settings.name)) {
                throw new Exception (string.Format ("Temperature Group: {0} already exists", settings.name));
            }

            temperatureGroups[settings.name] = new TemperatureGroup (
                settings.name,
                settings.highTemperatureAlarmSetpoint,
                settings.lowTemperatureAlarmSetpoint,
                settings.temperatureSetpoint,
                settings.temperatureDeadband,
                settings.temperatureProbes,
                settings.heaters);

            if (_defaultTemperatureGroup.IsEmpty ()) {
                _defaultTemperatureGroup = settings.name;
            }

            if (saveToFile) {
                AddTemperatureGroupSettingsToFile (settings.name);
            }
        }

        public static void UpdateTemperatureGroup (string name, TemperatureGroupSettings settings) {
            if (TemperatureGroupNameExists (name)) {
                RemoveTemperatureGroup (name);
            }

            AddTemperatureGroup (settings);
        }

        public static void RemoveTemperatureGroup (string name) {
            CheckTemperatureGroupKey (name);
            temperatureGroups.Remove (name);

            if (_defaultTemperatureGroup == name) {
                if (temperatureGroups.Count > 0) {
                    var first = temperatureGroups.First ();
                    _defaultTemperatureGroup = first.Key;
                } else {
                    _defaultTemperatureGroup = string.Empty;
                }
            }

            DeleteTemperatureGroupSettingsFromFile (name);
        }

        public static void CheckTemperatureGroupKey (string name) {
            if (!temperatureGroups.ContainsKey (name)) {
                throw new ArgumentException ("name");
            }
        }

        public static bool CheckTemperatureGroupKeyNoThrow (string name) {
            try {
                CheckTemperatureGroupKey (name);
                return true;
            } catch {
                return false;
            }
        }

        public static bool TemperatureGroupNameExists (string name) {
            return CheckTemperatureGroupKeyNoThrow (name);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllTemperatureGroupNames () {
            List<string> names = new List<string> ();
            foreach (var group in temperatureGroups.Values) {
                names.Add (group.name);
            }
            return names.ToArray ();
        }

        /***Temperature***/
        public static float GetTemperatureGroupTemperature (string name) {
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].temperature;
        }

        /***Probes Connected***/
        public static bool GetTemperatureGroupTemperatureProbesConnected (string name) {
            CheckTemperatureGroupKey (name);
            bool connected = false;
            foreach (var probeName in temperatureGroups[name].temperatureProbes.Keys) {
                var probe = (TemperatureProbe)Sensors.TemperatureProbes.GetGadget (probeName);
                // Using OR because we really only care that at least one sensor is connected
                connected |= probe.connected;
            }
            return connected;
        }

        /***High temperature alarm setpoint***/
        public static float GetTemperatureGroupHighTemperatureAlarmSetpoint (string name) {
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].highTemperatureAlarmSetpoint;
        }

        /***Low temperature alarm setpoint***/
        public static float GetTemperatureGroupLowTemperatureAlarmSetpoint (string name) {
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].lowTemperatureAlarmSetpoint;
        }

        /***Temperature setpoint***/
        public static float GetTemperatureGroupTemperatureSetpoint (string name) {
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].temperatureSetpoint;
        }

        /***Temperature deadband***/
        public static float GetTemperatureGroupTemperatureDeadband (string name) {
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].temperatureDeadband;
        }

        /***High temperature alarm index***/
        public static int GetTemperatureGroupHighTemperatureAlarmIndex (string name) {
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].highTemperatureAlarmIndex;
        }

        /***Low temperature alarm index***/
        public static int GetTemperatureGroupLowTemperatureAlarmIndex (string name) {
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].lowTemperatureAlarmIndex;
        }

        /***Data logger***/
        public static IDataLogger GetTemperatureGroupDataLogger (string name) {
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].dataLogger;
        }

        /***Settings***************************************************************************************************/
        public static TemperatureGroupSettings GetTemperatureGroupSettings (string name) {
            CheckTemperatureGroupKey (name);
            var settings = new TemperatureGroupSettings ();
            settings.name = name;
            settings.highTemperatureAlarmSetpoint = GetTemperatureGroupHighTemperatureAlarmSetpoint (name);
            settings.lowTemperatureAlarmSetpoint = GetTemperatureGroupLowTemperatureAlarmSetpoint (name);
            settings.temperatureSetpoint = GetTemperatureGroupTemperatureSetpoint (name);
            settings.temperatureDeadband = GetTemperatureGroupTemperatureDeadband (name);
            settings.temperatureProbes = GetAllTemperatureProbesForTemperatureGroup (name);
            settings.heaters = GetAllHeaterSettings (name);
            return settings;
        }

        protected static void AddTemperatureGroupSettingsToFile (string name) {
            CheckTemperatureGroupKey (name);
            SettingsHelper.AddSettingsToArray (settingsFileName, groupSettingsArrayName, GetTemperatureGroupSettings (name));
        }

        protected static void UpdateTemperatureGroupSettingsInFile (string name) {
            SettingsHelper.UpdateSettingsInArray (settingsFileName, groupSettingsArrayName, name, GetTemperatureGroupSettings (name));
        }

        protected static void DeleteTemperatureGroupSettingsFromFile (string name) {
            SettingsHelper.DeleteSettingsFromArray (settingsFileName, groupSettingsArrayName, name);
        }

        /**************************************************************************************************************/
        /* Temperature Probe                                                                                          */
        /**************************************************************************************************************/
        public static void AddTemperatureProbeToTemperatureGroup (string groupName, string temperatureProbeName) {
            CheckTemperatureGroupKey (groupName);
            temperatureGroups[groupName].temperatureProbes.Add (temperatureProbeName, new TemperatureGroup.InternalTemperatureProbeState ());
            UpdateTemperatureGroupSettingsInFile (groupName);
        }

        public static void RemoveTemperatureProbeFromTemperatureGroup (string groupName, string temperatureProbeName) {
            CheckTemperatureGroupKey (groupName);
            temperatureGroups[groupName].temperatureProbes.Remove (temperatureProbeName);
            UpdateTemperatureGroupSettingsInFile (groupName);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllTemperatureProbesForTemperatureGroup (string groupName) {
            CheckTemperatureGroupKey (groupName);
            List<string> names = new List<string> ();
            foreach (var sensorName in temperatureGroups[groupName].temperatureProbes.Keys) {
                names.Add (sensorName);
            }
            return names.ToArray ();
        }

        /**************************************************************************************************************/
        /* Heaters                                                                                                    */
        /**************************************************************************************************************/
        public static void AddHeater (string groupName, HeaterSettings settings, bool saveToFile = true) {
            CheckTemperatureGroupKey (groupName);

            if (HeaterNameExists (groupName, settings.name)) {
                throw new Exception (string.Format ("Heater: {0} already exists", settings.name));
            }

            temperatureGroups[groupName].heaters[settings.name] = new Heater (settings, groupName);

            if (saveToFile) {
                UpdateTemperatureGroupSettingsInFile (groupName);
            }
        }

        public static void UpdateHeater (string groupName, string heaterName, HeaterSettings settings) {
            CheckTemperatureGroupKey (groupName);
            if (HeaterNameExists (groupName, heaterName)) {
                RemoveHeater (groupName, heaterName);
            }
            AddHeater (groupName, settings);
        }

        public static void RemoveHeater (string groupName, string heaterName) {
            CheckHeaterKey (groupName, heaterName);
            temperatureGroups[groupName].heaters[heaterName].Dispose ();
            temperatureGroups[groupName].heaters.Remove (heaterName);
            UpdateTemperatureGroupSettingsInFile (groupName);
        }

        public static void CheckHeaterKey (string groupName, string heaterName) {
            CheckTemperatureGroupKey (groupName);
            if (!temperatureGroups[groupName].heaters.ContainsKey (heaterName)) {
                throw new ArgumentException ("heaterName");
            }
        }

        public static bool CheckHeaterKeyNoThrow (string groupName, string heaterName) {
            try {
                CheckHeaterKey (groupName, heaterName);
                return true;
            } catch {
                return false;
            }
        }

        public static bool HeaterNameExists (string groupName, string heaterName) {
            return CheckHeaterKeyNoThrow (groupName, heaterName);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllHeaterNames (string groupName) {
            CheckTemperatureGroupKey (groupName);
            List<string> names = new List<string> ();
            foreach (var heater in temperatureGroups[groupName].heaters.Values) {
                names.Add (heater.name);
            }
            return names.ToArray ();
        }

        /***Individual Control***/
        public static IndividualControl GetHeaterIndividualControl (string groupName, string heaterName) {
            CheckHeaterKey (groupName, heaterName);
            return temperatureGroups[groupName].heaters[heaterName].channel;
        }

        /***Settings***************************************************************************************************/
        public static HeaterSettings GetHeaterSettings (string groupName, string heaterName) {
            CheckHeaterKey (groupName, heaterName);
            var settings = new HeaterSettings ();
            settings.name = heaterName;
            settings.channel = GetHeaterIndividualControl (groupName, heaterName);
            return settings;
        }

        public static IEnumerable<HeaterSettings> GetAllHeaterSettings (string groupName) {
            CheckTemperatureGroupKey (groupName);
            List<HeaterSettings> heaterSettings = new List<HeaterSettings> ();
            foreach (var heater in temperatureGroups[groupName].heaters.Values) {
                var settings = new HeaterSettings ();
                settings.name = heater.name;
                settings.channel = heater.channel;
                heaterSettings.Add (settings);
            }
            return heaterSettings;
        }
    }
}

