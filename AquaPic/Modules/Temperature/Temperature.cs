using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Runtime;
using AquaPic.Drivers;
using AquaPic.Utilites;

namespace AquaPic.Modules
{
    public partial class Temperature
    {
        private static List<Heater> heaters;
        private static List<TemperatureProbe> probes;
        private static Dictionary<string,TemperatureGroup> temperatureGroups;
        private static string _defaultTemperatureGroup;

        /**************************************************************************************************************/
        /* Heaters                                                                                                    */
        /**************************************************************************************************************/
        public static int heaterCount {
            get {
                return heaters.Count;
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
                return _defaultTemperatureGroup;
            }
        }

        public static float temperature {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].temperature;
                } else {
                    return 0.0f;
                }
            }
        }

        public static float temperatureSetpoint {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].temperatureSetpoint;
                } else {
                    return 0.0f;
                }
            }
        }

        public static float temperatureDeadband {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].temperatureDeadband;
                } else {
                    return 0.0f;
                }
            }
        }

        public static float highTemperatureAlarmSetpoint {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].highTemperatureAlarmSetpoint;
                } else {
                    return 0.0f;
                }
            }
        }

        public static float lowTemperatureAlarmSetpoint {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].lowTemperatureAlarmSetpoint;
                } else {
                    return 0.0f;
                }
            }
        }

        public static int highTemperatureAlarmIndex {
            get {
                if (!string.IsNullOrWhiteSpace (_defaultTemperatureGroup)) {
                    return temperatureGroups[_defaultTemperatureGroup].highTemperatureAlarmIndex;
                } else {
                    return -1;
                }
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
            heaters = new List<Heater> ();
            probes = new List<TemperatureProbe> ();
            temperatureGroups = new Dictionary<string,TemperatureGroup> ();

            string path = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "tempProperties.json");

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
                        zeroValue = 0.0f;
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

            TaskManager.AddCyclicInterrupt ("Temperature", 1000, Run);
        }

        public static void Init () {
            Logger.Add ("Initializing Temperature");
        }

        public static void Run () {
            foreach (var group in temperatureGroups.Values) {
                group.Run ();
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
            if (TemperatureGroupNameOk (name)) {
                temperatureGroups.Add ( 
                    name, 
                    new TemperatureGroup (
                        name,  
                        highTemperatureAlarmSetpoint,
                        lowTemperatureAlarmSetpoint,
                        temperatureSetpoint,
                        temperatureDeadband));
            } else {
                throw new Exception (string.Format ("Temperature Group: {0} already exists", name));
            }
        }

        public static void RemoveTemperatureGroup (string name) {
            //int temperatureGroupIndex = GetTemperatureGroupIndex (name);
            //RemoveTemperatureGroup (temperatureGroupIndex);
            
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

        //public static void RemoveTemperatureGroup (int groupIndex) {
        //    CheckTemperatureGroupRange (groupIndex);
        //    temperatureGroups.RemoveAt (groupIndex);
        //}

        //protected static void CheckTemperatureGroupRange (int groupIndex) {
        //    if ((groupIndex < 0) || (groupIndex >= temperatureGroups.Count)) {
        //        throw new ArgumentOutOfRangeException ("groupIndex");
        //    }
        //}

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

        /***Getters****************************************************************************************************/
        /***Name***/
        //public static string GetTemperatureGroupName (int groupIndex) {
        //    CheckTemperatureGroupRange (groupIndex);
        //    return temperatureGroups[groupIndex].name;
        //}

        /***Names***/
        public static string[] GetAllTemperatureGroupNames () {
            List<string> names = new List<string> ();
            foreach (var group in temperatureGroups.Values) {
                names.Add (group.name);
            }
            return names.ToArray ();
        }

        /***Index***/
        //public static int GetTemperatureGroupIndex (string name) {
        //    for (int i = 0; i < temperatureGroups.Count; ++i) {
        //        if (string.Equals (name, temperatureGroups[i].name, StringComparison.InvariantCultureIgnoreCase))
        //            return i;
        //    }
        //
        //    throw new ArgumentException (name + " does not exists");
        //}

        //public static int GetTemperatureGroupIndexNoThrow (string name) {
        //    int temperatureGroupIndex;
        //    
        //    try {
        //        temperatureGroupIndex = GetTemperatureGroupIndex (name);
        //    } catch (ArgumentException) {
        //        temperatureGroupIndex = -1;
        //    }
        //
        //    return temperatureGroupIndex;
        //}

        /***Temperature***/
        public static float GetTemperatureGroupTemperature (string name) {
            //var groupIndex = GetTemperatureGroupIndex (name);
            //return GetTemperatureGroupTemperature (groupIndex);
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].temperature;
        }

        //public static float GetTemperatureGroupTemperature (int groupIndex) {
        //    CheckTemperatureGroupRange (groupIndex);
        //    return temperatureGroups[groupIndex].temperature;
        //}

        /***High temperature alarm setpoint***/
        public static float GetTemperatureGroupHighTemperatureAlarmSetpoint (string name) {
            //var groupIndex = GetTemperatureGroupIndex (name);
            //return GetTemperatureGroupHighTemperatureAlarmSetpoint (groupIndex);
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].highTemperatureAlarmSetpoint;
        }

        //public static float GetTemperatureGroupHighTemperatureAlarmSetpoint (int groupIndex) {
        //    CheckTemperatureGroupRange (groupIndex);
        //    return temperatureGroups[groupIndex].highTemperatureAlarmSetpoint;
        //}

        /***Low temperature alarm setpoint***/
        public static float GetTemperatureGroupLowTemperatureAlarmSetpoint (string name) {
            //var groupIndex = GetTemperatureGroupIndex (name);
            //return GetTemperatureGroupLowTemperatureAlarmSetpoint (groupIndex);
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].lowTemperatureAlarmSetpoint;
        }
        
        //public static float GetTemperatureGroupLowTemperatureAlarmSetpoint (int groupIndex) {
        //    CheckTemperatureGroupRange (groupIndex);
        //    return temperatureGroups[groupIndex].lowTemperatureAlarmSetpoint;
        //}

        /***Temperature setpoint***/
        public static float GetTemperatureGroupTemperatureSetpoint (string name) {
            //var groupIndex = GetTemperatureGroupIndex (name);
            //return GetTemperatureGroupTemperatureSetpoint (groupIndex);
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].temperatureSetpoint;
        }
        
        //public static float GetTemperatureGroupTemperatureSetpoint (int groupIndex) {
        //    CheckTemperatureGroupRange (groupIndex);
        //    return temperatureGroups[groupIndex].temperatureSetpoint;
        //}

        /***Temperature deadband***/
        public static float GetTemperatureGroupTemperatureDeadband (string name) {
            //var groupIndex = GetTemperatureGroupIndex (name);
            //return GetTemperatureGroupTemperatureDeadband (groupIndex);
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].temperatureDeadband;
        }
        
        //public static float GetTemperatureGroupTemperatureDeadband (int groupIndex) {
        //    CheckTemperatureGroupRange (groupIndex);
        //    return temperatureGroups[groupIndex].temperatureDeadband;
        //}

        /***High temperature alarm index***/
        public static int GetTemperatureGroupHighTemperatureAlarmIndex (string name) {
            //var groupIndex = GetTemperatureGroupIndex (name);
            //return GetTemperatureGroupHighTemperatureAlarmIndex (groupIndex);
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].highTemperatureAlarmIndex;
        }
        
        //public static int GetTemperatureGroupHighTemperatureAlarmIndex (int groupIndex) {
        //    CheckTemperatureGroupRange (groupIndex);
        //    return temperatureGroups[groupIndex].highTemperatureAlarmIndex;
        //}

        /***Low temperature alarm index***/
        public static int GetTemperatureGroupLowTemperatureAlarmIndex (string name) {
            //var groupIndex = GetTemperatureGroupIndex (name);
            //return GetTemperatureGroupLowTemperatureAlarmIndex (groupIndex);
            CheckTemperatureGroupKey (name);
            return temperatureGroups[name].lowTemperatureAlarmIndex;
        }
        
        //public static int GetTemperatureGroupLowTemperatureAlarmIndex (int groupIndex) {
        //    CheckTemperatureGroupRange (groupIndex);
        //    return temperatureGroups[groupIndex].lowTemperatureAlarmIndex;
        //}

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
            if (HeaterNameOk (name)) {
                heaters.Add (new Heater (name, (byte)powerID, (byte)plugID, temperatureGroupName));
            } else {
                throw new Exception (string.Format ("Heater: {0} already exists", name));
            }
        }

        public static void RemoveHeater (string name) {
            int heaterIndex = GetHeaterIndex (name);
            RemoveHeater (heaterIndex);
        }

        public static void RemoveHeater (int heaterIndex) {
            CheckHeaterRange (heaterIndex);
            Power.RemoveOutlet (heaters[heaterIndex].plug);
            heaters.RemoveAt (heaterIndex);
        }

        public static bool HeaterNameOk (string name) {
            try {
                GetHeaterIndex (name);
                return false;
            } catch {
                return true;
            }
        }

        protected static void CheckHeaterRange (int heaterIndex) {
            if ((heaterIndex < 0) || (heaterIndex >= heaters.Count)) {
                throw new ArgumentOutOfRangeException ("heaterIndex");
            }
        }

        /***Getters****************************************************************************************************/
        /***Name***/
        public static string GetHeaterName (int heaterIndex) {
            CheckHeaterRange (heaterIndex);
            return heaters[heaterIndex].name;
        }

        /***Names***/
        public static string[] GetAllHeaterNames () {
            string[] names = new string[heaters.Count];
            for (int i = 0; i < heaters.Count; ++i)
                names [i] = heaters [i].name;
            return names;
        }

        /***Index***/
        public static int GetHeaterIndex (string name) {
            for (int i = 0; i < heaters.Count; ++i) {
                if (string.Equals (name, heaters[i].name, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException (name + " does not exists");
        }

        /***Individual Control***/
        public static IndividualControl GetHeaterIndividualControl (int heaterIndex) {
            CheckHeaterRange (heaterIndex);
            return heaters[heaterIndex].plug;
        }

        /***Temperature group name***/
        public static string GetHeaterTemperatureGroupName (int heaterIndex) {
            CheckHeaterRange (heaterIndex);
            return heaters[heaterIndex].temperatureGroupName;
        }

        /***Temperature group index***/
        //public static int GetHeaterTemperatureGroupIndex (int heaterIndex) {
        //    CheckHeaterRange (heaterIndex);
        //    return GetTemperatureGroupIndexNoThrow (heaters[heaterIndex].temperatureGroupName);
        //}

        /***Setters****************************************************************************************************/
        /***Name***/
        public static void SetHeaterName (int heaterIndex, string name) {
            CheckHeaterRange (heaterIndex);
            if (HeaterNameOk (name)) {
                heaters[heaterIndex].name = name;
                Power.SetOutletName (heaters[heaterIndex].plug, heaters[heaterIndex].name);
                return;
            } else {
                throw new Exception (string.Format ("Heater: {0} already exists", name));
            }
        }

        /***Individual Control***/
        public static void SetHeaterIndividualControl (int heaterIndex, IndividualControl ic) {
            CheckHeaterRange (heaterIndex);
            Power.RemoveOutlet (heaters[heaterIndex].plug);
            heaters[heaterIndex].plug = ic;
            var coil = Power.AddOutlet (heaters[heaterIndex].plug, heaters[heaterIndex].name, MyState.On, "Temperature");
            coil.ConditionChecker = heaters[heaterIndex].OnPlugControl;
        }

        /***Temperature group***/
        public static void SetHeaterTemperatureGroupName (int heaterIndex, string temperatureGroupName) {
            CheckHeaterRange (heaterIndex);
            heaters[heaterIndex].temperatureGroupName = temperatureGroupName;
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
            if (TemperatureProbeNameOk (name)) {
                probes.Add (
                    new TemperatureProbe (
                        name, 
                        cardID, 
                        channelID, 
                        zeroActual, 
                        zeroValue, 
                        fullScaleActual, 
                        fullScaleValue,
                        temperatureGroupName));
            } else {
                throw new Exception (string.Format ("Probe: {0} already exists", name));
            }
        }

        public static void RemoveTemperatureProbe (int probeId) {
            if ((probeId >= 0) && (probeId < probes.Count)) {
                TemperatureProbe p = probes[probeId];
                AquaPicDrivers.AnalogInput.RemoveChannel (p.channel);
                probes.Remove (p);
            } else {
                throw new ArgumentOutOfRangeException ("probeId");
            }
        }

        public static bool TemperatureProbeNameOk (string name) {
            try {
                GetTemperatureProbeIndex (name);
                return false;
            } catch {
                return true;
            }
        }

        protected static void CheckTemperatureProbeRange (int probeIndex) {
            if ((probeIndex < 0) || (probeIndex >= probes.Count)) {
                throw new ArgumentOutOfRangeException ("probeIndex");
            }
        }

        public static string[] GetAllTemperatureProbeNames () {
            string[] names = new string[probes.Count];
            for (int i = 0; i < names.Length; ++i)
                names [i] = probes [i].name;

            return names;
        }

        public static string GetTemperatureProbeName (int probeIdx) {
            if ((probeIdx >= 0) && (probeIdx < probes.Count))
                return probes [probeIdx].name;

            throw new ArgumentOutOfRangeException ("probeId");
        }

        public static void SetTemperatureProbeName (int probeId, string name) {
            if ((probeId >= 0) && (probeId < probes.Count)) {
                if (TemperatureProbeNameOk (name)) {
                    probes [probeId].name = name;
                    return;
                } else
                    throw new Exception (string.Format ("Probe: {0} already exists", name));
            }

            throw new ArgumentOutOfRangeException ("probeId");
        }

        public static int GetTemperatureProbeIndex (string name) {
            for (int i = 0; i < probes.Count; ++i) {
                if (string.Equals (name, probes [i].name, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException (name + " does not exists");
        }

        public static IndividualControl GetTemperatureProbeIndividualControl (int probeId) {
            if ((probeId >= 0) && (probeId < probes.Count))
                return probes [probeId].channel;

            throw new ArgumentOutOfRangeException ("probeId");
        }

        public static void SetTemperatureProbeIndividualControl (int probeId, IndividualControl ic) {
            if ((probeId >= 0) && (probeId < probes.Count)) {
                AquaPicDrivers.AnalogInput.RemoveChannel (probes [probeId].channel);
                probes [probeId].channel = ic;
                AquaPicDrivers.AnalogInput.AddChannel (probes [probeId].channel, probes [probeId].name);
            }
        }

        public static float GetTemperatureProbeTemperature (int probeId) {
            if ((probeId >= 0) && (probeId < probes.Count))
                return probes [probeId].temperature;

            throw new ArgumentOutOfRangeException ("probeId");
        }

        public static string GetTemperatureProbeTemperatureGroupName (int probeIndex) {
            CheckTemperatureProbeRange (probeIndex);
            return probes[probeIndex].temperatureGroupName;
        }

        public static void SetTemperatureProbeTemperatureGroupName (int probeIndex, string temperatureGroupName) {
            CheckTemperatureProbeRange (probeIndex);
            probes[probeIndex].temperatureGroupName = temperatureGroupName;
        }
    }
}

