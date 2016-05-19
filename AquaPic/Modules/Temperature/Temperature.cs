using System;
using System.Collections.Generic;
using System.IO;
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
        private static DataLogger dataLogger;

        private static float temperature;
        public static float WaterTemperature {
            get { return temperature; }
        }

        public static int heaterCount {
            get {
                return heaters.Count;
            }
        }

        public static int temperatureProbeCount {
            get {
                return probes.Count;
            }
        }

        private static int highTempAlarmIdx;
        private static int lowTempAlarmIdx;
        public static int HighTemperatureAlarmIndex {
            get { return highTempAlarmIdx; }
        }
        public static int LowTemperatureAlarmIndex {
            get { return lowTempAlarmIdx; }
        }

        public static float highTempAlarmSetpoint;
        public static float lowTempAlarmSetpoint;
        public static float temperatureSetpoint;
        public static float temperatureDeadband;

        static Temperature () {
            heaters = new List<Heater> ();
            probes = new List<TemperatureProbe> ();

            string path = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "tempProperties.json");

            using (StreamReader reader = File.OpenText (path)) {
                JObject jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));
                highTempAlarmSetpoint = Convert.ToSingle (jo ["highTempAlarmSetpoint"]);
                lowTempAlarmSetpoint = Convert.ToSingle (jo ["lowTempAlarmSetpoint"]);
                temperatureSetpoint = Convert.ToSingle (jo ["tempSetpoint"]);
                temperatureDeadband = Convert.ToSingle (jo ["deadband"]) / 2;

                JArray ja = (JArray)jo ["temperatureProbes"];
                foreach (var jt in ja) {
                    JObject obj = jt as JObject;
                    string name = (string)obj ["name"];
                    int cardId = AquaPicDrivers.AnalogInput.GetCardIndex ((string)obj ["inputCard"]);
                    int channelId = Convert.ToInt32 (obj ["channel"]);
                    AddTemperatureProbe (name, cardId, channelId);
                }

                ja = (JArray)jo ["heaters"];
                foreach (var jt in ja) {
                    JObject obj = jt as JObject;
                    string name = (string)obj ["name"];
                    int powerStripId = Power.GetPowerStripIndex ((string)obj ["powerStrip"]);
                    int outletId = Convert.ToInt32 (obj ["outlet"]);
                    AddHeater (name, powerStripId, outletId);
                }
            }

            highTempAlarmIdx = Alarm.Subscribe ("High temperature");
            lowTempAlarmIdx = Alarm.Subscribe ("Low temperature");

            temperature = 0.0f;

            dataLogger = new DataLogger ("Temperature");

            TaskManager.AddCyclicInterrupt ("Temperature", 1000, Run);
        }

        public static void Init () {
            Logger.Add ("Initializing Temperature");
        }

        public static void Run () {
            temperature = 0.0f;

            if (probes.Count > 0) {
                foreach (var ch in probes)
                    temperature += ch.GetTemperature ();

                temperature /= probes.Count;

                if (temperature > highTempAlarmSetpoint)
                    Alarm.Post (highTempAlarmIdx);
                else {
                    if (Alarm.CheckAlarming (highTempAlarmIdx))
                        Alarm.Clear (highTempAlarmIdx);
                }

                if (temperature < lowTempAlarmSetpoint)
                    Alarm.Post (lowTempAlarmIdx);
                else {
                    if (Alarm.CheckAlarming (lowTempAlarmIdx))
                        Alarm.Clear (lowTempAlarmIdx);
                }
            }

            dataLogger.AddEntry (temperature);
        }

//        public static void AddHeater (
//            int powerID, 
//            int plugID,
//            string name,
//            bool controlTemp = true, 
//            float setpoint = 78.0f, 
//            float deadband = 0.4f)
//        {
//            heaters.Add (new Heater (name, (byte)powerID, (byte)plugID, controlTemp, setpoint, deadband));
//        }

        public static void AddHeater (
            string name,
            int powerID, 
            int plugID)
        {
            if (HeaterNameOk (name))
                heaters.Add (new Heater (name, (byte)powerID, (byte)plugID));
            else
                throw new Exception (string.Format ("Heater: {0} already exists", name));
        }

        public static bool HeaterNameOk (string name) {
            try {
                GetHeaterIndex (name);
                return false;
            } catch {
                return true;
            }
        }

        public static void RemoveHeater (int heaterId) {
            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
                Heater h = heaters [heaterId];
                Power.RemoveOutlet (h.plug);
                heaters.Remove (h);
                return;
            }

            throw new ArgumentOutOfRangeException ("heaterId");
        }

        public static string[] GetAllHeaterNames () {
            string[] names = new string[heaters.Count];
            for (int i = 0; i < heaters.Count; ++i)
                names [i] = heaters [i].name;
            return names;
        }

        public static string GetHeaterName (int heaterId) {
            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
                return heaters [heaterId].name;
            }

            throw new ArgumentOutOfRangeException ("heaterId");
        }

        public static void SetHeaterName (int heaterId, string name) {
            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
                if (HeaterNameOk (name)) {
                    heaters [heaterId].name = name;
                    Power.SetOutletName (heaters [heaterId].plug, heaters [heaterId].name);
                    return;
                } else
                    throw new Exception (string.Format ("Heater: {0} already exists", name));
            }

            throw new ArgumentOutOfRangeException ("heaterId");
        }

        public static int GetHeaterIndex (string name) {
            for (int i = 0; i < heaters.Count; ++i) {
                if (string.Equals (name, heaters [i].name, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException (name + " does not exists");
        }

        public static IndividualControl GetHeaterIndividualControl (int heaterId) {
            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
                return heaters [heaterId].plug;
            }

            throw new ArgumentOutOfRangeException ("heaterId");
        }

        public static void SetHeaterIndividualControl (int heaterId, IndividualControl ic) {
            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
                Power.RemoveOutlet (heaters [heaterId].plug);
                heaters [heaterId].plug = ic;
                var coil = Power.AddOutlet (heaters [heaterId].plug, heaters [heaterId].name, MyState.On, "Temperature");
                coil.ConditionChecker = heaters [heaterId].OnPlugControl;

                return;
            }

            throw new ArgumentOutOfRangeException ("heaterId");
        }

        public static void AddTemperatureProbe (string name, int cardID, int channelID) {
            if (TemperatureProbeNameOk (name))
                probes.Add (new TemperatureProbe (name, cardID, channelID));
            else
                throw new Exception (string.Format ("Probe: {0} already exists", name));
        }

        public static void RemoveTemperatureProbe (int probeId) {
            if ((probeId >= 0) && (probeId < probes.Count)) {
                TemperatureProbe p = probes [probeId];
                AquaPicDrivers.AnalogInput.RemoveChannel (p.channel);
                probes.Remove (p);
            } else 
                throw new ArgumentOutOfRangeException ("probeId");
        }

        public static bool TemperatureProbeNameOk (string name) {
            try {
                GetTemperatureProbeIndex (name);
                return false;
            } catch {
                return true;
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

//        public static bool ControlsTemperature (int heaterId) {
//            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
//                return heaters [heaterId].controlWaterTemperature;
//            }
//            return false;
//        }

//        public static float GetHeaterSetpoint (int heaterId) {
//            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
//                return heaters [heaterId].setpoint;
//            }
//            return 0.0f;
//        }

//        public static float GetHeaterDeadband (int heaterId) {
//            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
//                return heaters [heaterId].deadband * 2;
//            }
//            return 0.0f;
//        }
    }
}

