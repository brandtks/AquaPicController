using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Modules
{
    public partial class WaterLevel
    {
        private static AnalogSensor analogSensor;

        public static float highAnalogLevelAlarmSetpoint {
            get {
                return analogSensor.highLevelAlarmSetpoint;
            }
            set {
                analogSensor.highLevelAlarmSetpoint = value;
            }
        }

        public static float lowAnalogLevelAlarmSetpoint {
            get {
                return analogSensor.lowLevelAlarmSetpoint;
            }
            set {
                analogSensor.lowLevelAlarmSetpoint = value;
            }
        }

        public static float analogWaterLevel {
            get {
                return analogSensor.waterLevel;
            }
        }

        public static IndividualControl analogSensorChannel {
            get {
                return analogSensor.sensorChannel;
            }
        }

        public static bool analogSensorEnabled {
            get {
                return analogSensor.enable;
            }
        }
        
        static WaterLevel () {
            string path = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "waterLevelProperties.json");

            using (StreamReader reader = File.OpenText (path)) {
                JObject jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));

                if (Convert.ToBoolean (jo ["enableAnalogSensor"])) {
                    float highAlarm = Convert.ToSingle (jo ["highAnalogLevelAlarmSetpoint"]);
                    float lowAlarm = Convert.ToSingle (jo ["lowAnalogLevelAlarmSetpoint"]);

                    IndividualControl ic;
                    string name = Convert.ToString (jo ["analogSensorChannel"] ["Group"]);
                    int b = AnalogInput.GetCardIndex (name);
                    ic.Group = b;
                    ic.Individual = Convert.ToByte (jo ["analogSensorChannel"] ["Individual"]);

                    analogSensor = new AnalogSensor (true, highAlarm, lowAlarm, ic);
                } else {
                    analogSensor = new AnalogSensor (false);
                }
            }
        }

        public static void Init () {
            Logger.Add ("Initializing Water Level");
        }

        public static void SetAnalogSensorIndividualControl (IndividualControl ic) {
            AnalogInput.RemoveChannel (analogSensor.sensorChannel);
            analogSensor.sensorChannel = ic;
            AnalogInput.AddChannel (analogSensor.sensorChannel, AnalogType.Level, "Water Level");
        }

        public static void SetAnalogSensorEnable (bool enable) {
            analogSensor.enable = enable;
        }
    }
}