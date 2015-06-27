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

        private static List<Heater> heaters;
        public static List<IndividualControl> channels;

        private static float temperature;
        public static float WaterTemperature {
            get { return temperature; }
        }

        static Temperature () {
            heaters = new List<Heater> ();
            channels = new List<IndividualControl> ();

            string path = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "tempProperties.json");

            using (StreamReader reader = File.OpenText (path)) {
                JObject jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));
                highTempAlarmSetpoint = Convert.ToSingle (jo ["highTempAlarmSetpoint"]);
                lowTempAlarmSetpoint = Convert.ToSingle (jo ["lowTempAlarmSetpoint"]);
                temperatureSetpoint = Convert.ToSingle (jo ["tempSetpoint"]);
                temperatureDeadband = Convert.ToSingle (jo ["deadband"]) / 2;
            }

            highTempAlarmIdx = Alarm.Subscribe ("High temperature");
            lowTempAlarmIdx = Alarm.Subscribe ("Low temperature");

            Alarm.AddPostHandler (
                highTempAlarmIdx, 
                (sender) => {
                    foreach (var heater in heaters)
                        Power.AlarmShutdownOutlet (heater.plug);
                });

            temperature = 32.0f;

            TaskManager.AddCyclicInterrupt ("Temperature", 1000, Run);

            MainWindowWidgets.linePlots.Add ("Temperature", new LinePlotData (() => {return new TemperatureLinePlot ();}));
        }

        public static void AddTemperatureProbe (int cardID, int channelID, string name) {
            AnalogInput.AddChannel (cardID, channelID, AnalogType.Temperature, name);
            IndividualControl ch = new IndividualControl ();
            ch.Group = (byte)cardID;
            ch.Individual = (byte)channelID;
            channels.Add (ch);
        }

        public static void AddHeater (
            int powerID, 
            int plugID,
            string name,
            bool controlTemp = true, 
            float setpoint = 78.0f, 
            float deadband = 0.4f)
        {
            heaters.Add (new Heater (name, (byte)powerID, (byte)plugID, controlTemp, setpoint, deadband));
        }

        public static void Run () {
            temperature = 0.0f;
            foreach (var ch in channels)
                temperature += AnalogInput.GetValue (ch);
            temperature /= channels.Count;

            temperature = temperature.Map (0, 4096, 32.0f, 100.0f);

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
            return string.Empty;
        }

        public static int GetHeaterIndex (string name) {
            for (int i = 0; i < heaters.Count; ++i) {
                if (string.Equals (name, heaters [i].name, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }
            return -1;
        }

        public static bool ControlsTemperature (int heaterId) {
            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
                return heaters [heaterId].controlWaterTemperature;
            }
            return false;
        }

        public static float GetHeaterSetpoint (int heaterId) {
            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
                return heaters [heaterId].setpoint;
            }
            return 0.0f;
        }

        public static float GetHeaterDeadband (int heaterId) {
            if ((heaterId >= 0) && (heaterId < heaters.Count)) {
                return heaters [heaterId].deadband * 2;
            }
            return 0.0f;
        }
    }
}

