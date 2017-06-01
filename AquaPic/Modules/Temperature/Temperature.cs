#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Runtime;
using AquaPic.Drivers;
using AquaPic.Utilites;
using AquaPic.Sensors;

namespace AquaPic.Modules
{
    public partial class Temperature
    {
        private static Dictionary<string, Heater> heaters;
        private static Dictionary<string, TemperatureProbe> probes;
        private static Dictionary<string, TemperatureGroup> temperatureGroups;
        private static string _defaultTemperatureGroup;

        /**************************************************************************************************************/
        /* Heaters                                                                                                    */
        /**************************************************************************************************************/
        public static int heaterCount {
            get {
                return heaters.Count;
            }
        }

        public static string defaultHeater {
            get {
                if (heaters.Count > 0) {
                    var first = heaters.First ();
                    return first.Key;
                }

                return string.Empty;
            }
        }

        /**************************************************************************************************************/
        /* Temperature Probes                                                                                         */
        /**************************************************************************************************************/
        public static int temperatureProbeCount {
            get {
                return probes.Count;
            }
        }

        public static string defaultTemperatureProbe {
            get {
                if (probes.Count > 0) {
                    var first = probes.First ();
                    return first.Key;
                }
                    
                return string.Empty;
            }
        }

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

        public static float temperature {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].temperature;
                }
                    
                return 0.0f;
            }
        }

        public static float temperatureSetpoint {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].temperatureSetpoint;
                }
                    
                return 0.0f;
            }
        }

        public static float temperatureDeadband {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].temperatureDeadband;
                }
                    
                return 0.0f;
            }
        }

        public static float highTemperatureAlarmSetpoint {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].highTemperatureAlarmSetpoint;
                }
                    
                return 0.0f;
            }
        }

        public static float lowTemperatureAlarmSetpoint {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].lowTemperatureAlarmSetpoint;
                }
                    
                return 0.0f;
            }
        }

        public static int highTemperatureAlarmIndex {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].highTemperatureAlarmIndex;
                }

                return -1;
            }
        }

        public static int lowTemperatureAlarmIndex {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].lowTemperatureAlarmIndex;
                } else {
                    return -1;
                }
            }
        }

        /**************************************************************************************************************/
        /* Temperature                                                                                                */
        /**************************************************************************************************************/
        static Temperature () {
            heaters = new Dictionary<string,Heater> ();
            probes = new Dictionary<string,TemperatureProbe> ();
            temperatureGroups = new Dictionary<string,TemperatureGroup> ();

            string path = Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "tempProperties.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    JObject jo = JToken.ReadFrom (new JsonTextReader (reader)) as JObject;

                    /******************************************************************************************************/
                    /* Temperature Groups                                                                                 */
                    /******************************************************************************************************/
                    var ja = jo["temperatureGroups"] as JArray;
                    foreach (var jt in ja) {
                        var obj = jt as JObject;
                        var name = (string)obj["name"];
                        var highTemperatureAlarmSetpoint = Convert.ToSingle (obj["highTemperatureAlarmSetpoint"]);
                        var lowTemperatureAlarmSetpoint = Convert.ToSingle (obj["lowTemperatureAlarmSetpoint"]);
                        var temperatureSetpoint = Convert.ToSingle (obj["temperatureSetpoint"]);
                        var temperatureDeadband = Convert.ToSingle (obj["temperatureDeadband"]);

                        AddTemperatureGroup (
                            name,
                            highTemperatureAlarmSetpoint,
                            lowTemperatureAlarmSetpoint,
                            temperatureSetpoint,
                            temperatureDeadband);
                    }

                    _defaultTemperatureGroup = (string)jo["defaultTemperatureGroup"];
                    if (!CheckTemperatureGroupKeyNoThrow (_defaultTemperatureGroup)) {
                        if (temperatureGroups.Count > 0) {
                            var first = temperatureGroups.First ();
                            _defaultTemperatureGroup = first.Key;
                        } else {
                            _defaultTemperatureGroup = string.Empty;
                        }
                    }

                    /******************************************************************************************************/
                    /* Temperature Probes                                                                                 */
                    /******************************************************************************************************/
                    ja = jo["temperatureProbes"] as JArray;
                    foreach (var jt in ja) {
                        JObject obj = jt as JObject;
                        string name = (string)obj["name"];
                        int cardId = AquaPicDrivers.AnalogInput.GetCardIndex ((string)obj["inputCard"]);
                        int channelId = Convert.ToInt32 (obj["channel"]);

                        float zeroActual;
                        try {
                            zeroActual = Convert.ToSingle (obj["zeroCalibrationActual"]);
                        } catch {
                            zeroActual = 32.0f;
                        }

                        float zeroValue;
                        try {
                            zeroValue = Convert.ToSingle (obj["zeroCalibrationValue"]);
                        } catch {
                            zeroValue = 82.0f;
                        }

                        float fullScaleActual;
                        try {
                            fullScaleActual = Convert.ToSingle (obj["fullScaleCalibrationActual"]);
                        } catch {
                            fullScaleActual = 100.0f;
                        }

                        float fullScaleValue;
                        try {
                            fullScaleValue = Convert.ToSingle (obj["fullScaleCalibrationValue"]);
                        } catch {
                            fullScaleValue = 4095.0f;
                        }

                        var temperatureGroupName = (string)obj["temperatureGroup"];
                        if (!CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                            Logger.AddWarning ("Temperature probe {0} added to nonexistant group {1}", name, temperatureGroupName);
                        }

                        AddTemperatureProbe (
                            name,
                            cardId,
                            channelId,
                            zeroActual,
                            zeroValue,
                            fullScaleActual,
                            fullScaleValue,
                            temperatureGroupName);
                    }

                    /******************************************************************************************************/
                    /* Heaters                                                                                            */
                    /******************************************************************************************************/
                    ja = jo["heaters"] as JArray;
                    foreach (var jt in ja) {
                        JObject obj = jt as JObject;
                        string name = (string)obj["name"];
                        int powerStripId = Power.GetPowerStripIndex ((string)obj["powerStrip"]);
                        int outletId = Convert.ToInt32 (obj["outlet"]);

                        var temperatureGroupName = (string)obj["temperatureGroup"];
                        if (!CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                            Logger.AddWarning ("Temperature probe {0} added to nonexistant group {1}", name, temperatureGroupName);
                        }

                        AddHeater (name, powerStripId, outletId, temperatureGroupName);
                    }
                }
            } else {
                Logger.Add ("Temperature settings file did not exist, created new temperature settings");
                var file = File.Create (path);
                file.Close ();

                var jo = new JObject ();
                jo.Add (new JProperty ("temperatureGroups", new JArray ()));
                jo.Add (new JProperty ("defaultTemperatureGroup", string.Empty));
                jo.Add (new JProperty ("temperatureProbes", new JArray ()));
                jo.Add (new JProperty ("heaters", new JArray ()));

                File.WriteAllText (path, jo.ToString ());
            }

            TaskManager.AddCyclicInterrupt ("Temperature", 1000, Run);
        }

        public static void Init () {
            Logger.Add ("Initializing Temperature");
        }

        public static void Run () {
            foreach (var group in temperatureGroups.Values) {
                group.Run ();
            }

            foreach (var probe in probes.Values) {
                if (!CheckTemperatureGroupKeyNoThrow (probe.temperatureGroupName)) {
                    probe.Get ();
                }
            }
        }

        /**************************************************************************************************************/
        /* Temperature Groups                                                                                         */
        /**************************************************************************************************************/
        public static void AddTemperatureGroup (
            string name,
            float highTemperatureAlarmSetpoint,
            float lowTemperatureAlarmSetpoint,
            float temperatureSetpoint,
            float temperatureDeadband
        ) {
            if (!TemperatureGroupNameOk (name)) {
                throw new Exception (string.Format ("Temperature Group: {0} already exists", name));
            }

            temperatureGroups[name] = new TemperatureGroup (
                name,  
                highTemperatureAlarmSetpoint,
                lowTemperatureAlarmSetpoint,
                temperatureSetpoint,
                temperatureDeadband);

            if (_defaultTemperatureGroup.IsEmpty ()) {
                _defaultTemperatureGroup = name;
            }
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

        public static bool TemperatureGroupNameOk (string name) {
            return !CheckTemperatureGroupKeyNoThrow (name);
        }

        public static bool AreTemperatureProbesConnected (string name) {
            CheckTemperatureGroupKey (name);
            bool connected = false;
            foreach (var probe in probes.Values) {
                if (probe.temperatureGroupName == name) {
                    connected |= IsTemperatureProbeConnected (probe.name);
                }
            }
            return connected;
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

        public static DataLogger GetTemperatureGroupDataLogger (string name) {
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].dataLogger;
        }

        /***Setters****************************************************************************************************/
        /***High temperature alarm setpoint***/
        public static void SetTemperatureGroupHighTemperatureAlarmSetpoint (string name, float highTemperatureAlarmSetpoint) {
            CheckTemperatureGroupKey (name);
            temperatureGroups[name].highTemperatureAlarmSetpoint = highTemperatureAlarmSetpoint;
        }

        /***Low temperature alarm setpoint***/
        public static void SetTemperatureGroupLowTemperatureAlarmSetpoint (string name, float lowTemperatureAlarmSetpoint) {
            CheckTemperatureGroupKey (name);
            temperatureGroups[name].lowTemperatureAlarmSetpoint = lowTemperatureAlarmSetpoint;
        }

        /***Temperature setpoint***/
        public static void SetTemperatureGroupTemperatureSetpoint (string name, float temperatureSetpoint) {
            CheckTemperatureGroupKey (name);
            temperatureGroups[name].temperatureSetpoint = temperatureSetpoint;
        }

        /***Temperature deadband***/
        public static void SetTemperatureGroupTemperatureDeadband (string name, float temperatureDeadband) {
            CheckTemperatureGroupKey (name);
            temperatureGroups[name].temperatureDeadband = temperatureDeadband;
        }

        /**************************************************************************************************************/
        /* Heaters                                                                                                    */
        /**************************************************************************************************************/
        public static void AddHeater (string name, int powerID, int plugID, string temperatureGroupName) {
            if (!HeaterNameOk (name)) {
                throw new Exception (string.Format ("Heater: {0} already exists", name));
            }
            
            heaters [name] = new Heater (name, powerID, plugID, temperatureGroupName);
        }

        public static void RemoveHeater (string heaterName) {
            CheckHeaterKey (heaterName);
            Power.RemoveOutlet (heaters[heaterName].plug);
            Power.RemoveHandlerOnStateChange (heaters[heaterName].plug, heaters[heaterName].OnStateChange);
            heaters.Remove (heaterName);
        }

        public static void CheckHeaterKey (string heaterName) {
            if (!heaters.ContainsKey (heaterName)) {
                throw new ArgumentException ("heaterName");
            }
        }

        public static bool CheckHeaterKeyNoThrow (string heaterName) {
            try {
                CheckHeaterKey (heaterName);
                return true;
            } catch {
                return false;
            }
        }

        public static bool HeaterNameOk (string heaterName) {
            return !CheckHeaterKeyNoThrow (heaterName);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllHeaterNames () {
            List<string> names = new List<string> ();
            foreach (var heater in heaters.Values) {
                names.Add (heater.name);
            }
            return names.ToArray ();
        }

        /***Individual Control***/
        public static IndividualControl GetHeaterIndividualControl (string heaterName) {
            CheckHeaterKey (heaterName);
            return heaters[heaterName].plug;
        }

        /***Temperature group name***/
        public static string GetHeaterTemperatureGroupName (string heaterName) {
            CheckHeaterKey (heaterName);
            return heaters[heaterName].temperatureGroupName;
        }

        /***Setters****************************************************************************************************/
        /***Name***/
        public static void SetHeaterName (string oldHeaterName, string newHeaterName) {
            CheckHeaterKey (oldHeaterName);
            if (!HeaterNameOk (newHeaterName)) {
                throw new Exception (string.Format ("Heater: {0} already exists", newHeaterName));
            }

            var heater = heaters[oldHeaterName];
            
            heater.name = newHeaterName;
            Power.SetOutletName (heater.plug, heater.name);
            
            heaters.Remove (oldHeaterName);
            heaters[newHeaterName] = heater;
        }

        /***Individual Control***/
        public static void SetHeaterIndividualControl (string heaterName, IndividualControl ic) {
            CheckHeaterKey (heaterName);
            Power.RemoveOutlet (heaters[heaterName].plug);
            heaters[heaterName].plug = ic;
            var coil = Power.AddOutlet (heaters[heaterName].plug, heaters[heaterName].name, MyState.On, "Temperature");
            coil.ConditionGetter = heaters[heaterName].OnPlugControl;
        }

        /***Temperature group***/
        public static void SetHeaterTemperatureGroupName (string heaterName, string temperatureGroupName) {
            CheckHeaterKey (heaterName);
            heaters[heaterName].temperatureGroupName = temperatureGroupName;
        }
        
        /**************************************************************************************************************/
        /* Temperature Probes                                                                                         */
        /**************************************************************************************************************/
        public static void AddTemperatureProbe (
            string name, 
            int cardID, 
            int channelID, 
            float zeroActual, 
            float zeroValue, 
            float fullScaleActual, 
            float fullScaleValue, 
            string temperatureGroupName
        ) {
            if (!TemperatureProbeNameOk (name)) {
                throw new Exception (string.Format ("Probe: {0} already exists", name));
            }

            probes[name] = new TemperatureProbe (
                name, 
                cardID, 
                channelID, 
                zeroActual, 
                zeroValue, 
                fullScaleActual, 
                fullScaleValue,
                temperatureGroupName);
        }

        public static void RemoveTemperatureProbe (string probeName) {
            CheckTemperatureProbeKey (probeName);
            probes[probeName].Remove ();
            probes.Remove (probeName);
        }

        public static void CheckTemperatureProbeKey (string probeName) {
            if (!probes.ContainsKey (probeName)) {
                throw new ArgumentException ("probeName");
            }
        }

        public static bool CheckTemperatureProbeKeyNoThrow (string probeName) {
            try {
                CheckTemperatureProbeKey (probeName);
                return true;
            } catch {
                return false;
            }
        }

        public static bool TemperatureProbeNameOk (string probeName) {
            return !CheckTemperatureProbeKeyNoThrow (probeName);
        }

        public static bool IsTemperatureProbeConnected (string probeName) {
            CheckTemperatureProbeKey (probeName);
            if (probes[probeName].temperature < probes[probeName].zeroActual) {
                return false;
            } else {
                return true;
            }
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllTemperatureProbeNames () {
            List<string> names = new List<string> ();
            foreach (var probe in probes.Values) {
                names.Add (probe.name);
            }
            return names.ToArray ();
        }

        /***Temperature***/
        public static float GetTemperatureProbeTemperature (string probeName) {
            CheckTemperatureProbeKey (probeName);
            return probes[probeName].temperature;
        }

        /***Individual Control***/
        public static IndividualControl GetTemperatureProbeIndividualControl (string probeName) {
            CheckTemperatureProbeKey (probeName);
            return probes[probeName].channel;
        }

        /***Temperature group***/
        public static string GetTemperatureProbeTemperatureGroupName (string probeName) {
            CheckTemperatureProbeKey (probeName);
            return probes[probeName].temperatureGroupName;
        }

        /***Zero actual***/
        public static float GetTemperatureProbeZeroActual (string probeName) {
            CheckTemperatureProbeKey (probeName);
            return probes[probeName].zeroActual;
        }

        /***Zero value***/
        public static float GetTemperatureProbeZeroValue (string probeName) {
            CheckTemperatureProbeKey (probeName);
            return probes[probeName].zeroValue;
        }

        /***Full scale actual***/
        public static float GetTemperatureProbeFullScaleActual (string probeName) {
            CheckTemperatureProbeKey (probeName);
            return probes[probeName].fullScaleActual;
        }

        /***Full scale value***/
        public static float GetTemperatureProbeFullScaleValue (string probeName) {
            CheckTemperatureProbeKey (probeName);
            return probes[probeName].fullScaleValue;
        }

        /***Setters****************************************************************************************************/
        /***Name***/
        public static void SetTemperatureProbeName (string oldProbeName, string newProbeName) {
            CheckTemperatureProbeKey (oldProbeName);
            if (!TemperatureProbeNameOk (newProbeName)) {
                throw new Exception (string.Format ("Probe: {0} already exists", newProbeName));
            }

            var probe = probes[oldProbeName];
            
            probe.SetName (newProbeName);
            
            probes.Remove (oldProbeName);
            probes[newProbeName] = probe;
        }

        /***Individual Control***/
        public static void SetTemperatureProbeIndividualControl (string probeName, IndividualControl ic) {
            CheckTemperatureProbeKey (probeName);
            probes[probeName].Remove ();
            probes[probeName].Add (ic);
        }

        /***Temperature group***/
        public static void SetTemperatureProbeTemperatureGroupName (string probeName, string temperatureGroupName) {
            CheckTemperatureProbeKey (probeName);
            probes[probeName].temperatureGroupName = temperatureGroupName;
        }

        /***Calibration data***/
        public static bool SetTemperatureProbeCalibrationData (
            string probeName,
            float zeroActual,
            float zeroValue,
            float fullScaleActual,
            float fullScaleValue
        ) {
            if (fullScaleValue <= zeroValue)
                throw new ArgumentException ("Full scale value can't be less than or equal to zero value");

            if (fullScaleActual < 0.0f)
                throw new ArgumentException ("Full scale actual can't be less than zero");

            probes[probeName].zeroActual = zeroActual;
            probes[probeName].zeroValue = zeroValue;
            probes[probeName].fullScaleActual = fullScaleActual;
            probes[probeName].fullScaleValue = fullScaleValue;

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            JArray ja = jo["temperatureProbes"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja[i]["name"];
                if (probeName == n) {
                    arrIdx = i;
                    break;
                }
            }

            if (arrIdx == -1) {
                return false;
            }

            ja[arrIdx]["zeroCalibrationActual"] = probes[probeName].zeroActual.ToString ();
            ja[arrIdx]["zeroCalibrationValue"] = probes[probeName].zeroValue.ToString ();
            ja[arrIdx]["fullScaleCalibrationActual"] = probes[probeName].fullScaleActual.ToString ();
            ja[arrIdx]["fullScaleCalibrationValue"] = probes[probeName].fullScaleValue.ToString ();

            File.WriteAllText (path, jo.ToString ());

            return true;
        }

    }
}

