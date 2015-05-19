using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.AlarmRuntime;
using AquaPic.AnalogInputDriver;
using AquaPic.Globals;
using AquaPic.PowerDriver;
using AquaPic.Utilites;

namespace AquaPic.TemperatureModule
{
    public partial class Temperature
    {
        private static float highTempAlarmSetpoint;
        private static float lowTempAlarmSetpoint;
        private static int highTempAlarmIdx;
        private static int lowTempAlarmIdx;

        public static int HighTemperatureAlarmIndex {
            get { return highTempAlarmIdx; }
        }
        public static int LowTemperatureAlarmIndex {
            get { return lowTempAlarmIdx; }
        }

        private static List<Heater> heaters;
        public static List<IndividualControl> channels;

        private static float temperature;
        public static float WaterTemperature {
            get { return temperature; }
        }

        static Temperature () {
            heaters = new List<Heater> ();
            channels = new List<IndividualControl> ();

            string path = string.Format (
                "{0}{1}", 
                Environment.GetEnvironmentVariable ("AquaPic"), 
                @"\AquaPicRuntimeProject\tempProperties.json");

            using (StreamReader reader = File.OpenText (path)) {
                JObject jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));
                highTempAlarmSetpoint = Convert.ToSingle (jo ["highTempAlarmSetpoint"]);
                lowTempAlarmSetpoint = Convert.ToSingle (jo ["lowTempAlarmSetpoint"]);
            }

            highTempAlarmIdx = Alarm.Subscribe ("High temperature", "Water column temperature too high");
            lowTempAlarmIdx = Alarm.Subscribe ("Low temperature", "Water column temperature too low");

            Alarm.AddPostHandler (
                highTempAlarmIdx, 
                (sender) => {
                    foreach (var heater in heaters)
                        Power.AlarmShutdownOutlet (heater.Plug);
                });

            temperature = 32.0f;
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
            bool controlTemp = false, 
            float setpoint = 78.0f, 
            float offset = 0.3f)
        {
            heaters.Add (new Heater ((byte)powerID, (byte)plugID, controlTemp, setpoint, offset, name));
        }

        public static void Run () {
            temperature = 0.0f;
            for (int i = 0; i < channels.Count; ++i)
                temperature += AnalogInput.GetAnalogValue (channels [i]);
            temperature /= channels.Count;

            temperature = temperature.Map (0, 4096, 32.0f, 100.0f);

            if (temperature >= highTempAlarmSetpoint) 
                Alarm.Post (highTempAlarmIdx);

            if (temperature <= lowTempAlarmSetpoint)
                Alarm.Post (lowTempAlarmIdx);
        }
    }
}

