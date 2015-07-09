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
                return analogSensor.highAlarmStpnt;
            }
            set {
                analogSensor.highAlarmStpnt = value;
            }
        }

        public static float lowAnalogLevelAlarmSetpoint {
            get {
                return analogSensor.lowAlarmStpnt;
            }
            set {
                analogSensor.lowAlarmStpnt = value;
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

                bool enable = Convert.ToBoolean (jo ["enableAnalogSensor"]);

                float highAlarmSetpoint;
                string text = (string)jo ["highAnalogLevelAlarmSetpoint"];
                if (string.IsNullOrWhiteSpace (text)) {
                    highAlarmSetpoint = 0.0f;
                    enable = false;
                } else
                    highAlarmSetpoint = Convert.ToSingle (text);


                float lowAlarmSetpoint;
                text = (string)jo ["lowAnalogLevelAlarmSetpoint"];
                if (string.IsNullOrWhiteSpace (text)) {
                    lowAlarmSetpoint = 0.0f;
                    enable = false;
                } else
                    lowAlarmSetpoint = Convert.ToSingle (text);

                IndividualControl ic;
                text = Convert.ToString (jo ["analogSensorChannel"] ["Group"]);
                if (string.IsNullOrWhiteSpace (text)) {
                    ic = IndividualControl.Empty;
                    enable = false;
                } else
                    ic.Group = AnalogInput.GetCardIndex (text);

                text = (string)jo ["analogSensorChannel"] ["Individual"];
                if (string.IsNullOrWhiteSpace (text)) {
                    ic = IndividualControl.Empty;
                    enable = false;
                } else
                    ic.Individual = Convert.ToInt32 (jo ["analogSensorChannel"] ["Individual"]);

                analogSensor = new AnalogSensor (enable, highAlarmSetpoint, lowAlarmSetpoint, ic);
            }
        }

        public static void Init () {
            Logger.Add ("Initializing Water Level");
        }

        public static void SetAnalogSensorIndividualControl (IndividualControl ic) {
            if (analogSensor.sensorChannel.IsNotEmpty ())
                AnalogInput.RemoveChannel (analogSensor.sensorChannel);
            analogSensor.sensorChannel = ic;
            AnalogInput.AddChannel (analogSensor.sensorChannel, AnalogType.Level, "Water Level");
        }

        public static void SetAnalogSensorEnable (bool enable) {
            analogSensor.enable = enable;
            if (enable)
                analogSensor.SubscribeToAlarms ();
            else {
                if (analogSensor.sensorChannel.IsNotEmpty ())
                    AnalogInput.RemoveChannel (analogSensor.sensorChannel);
                analogSensor.sensorChannel = IndividualControl.Empty;

                analogSensor.lowAlarmStpnt = 0.0f;
                analogSensor.highAlarmStpnt = 0.0f;

                Alarm.Clear (analogSensor.highAlarmIndex);
                Alarm.Clear (analogSensor.lowAlarmIndex);
                Alarm.Clear (analogSensor.sensorAlarmIndex);
            }
        }
    }
}