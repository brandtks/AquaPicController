using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Gtk;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.SerialBus;
using AquaPic.Utilites;
using MyWidgetLibrary;

namespace AquaPic.Modules
{
    public class WaterLevel
    {
        private static float _waterLevel;
        private static IndividualControl levelSensor;
        private static int lowAlarm;
        private static int highAlarm;
        private static int probeAlarm;

        public static float highLevelAlarmSetpoint;
        public static float lowLevelAlarmSetpoint;

        public static float waterLevel {
            get {
                return _waterLevel;
            }
        }
        
        static WaterLevel () {
            _waterLevel = 0.0f;

            lowAlarm = Alarm.Subscribe ("Low Water Level");
            highAlarm = Alarm.Subscribe ("High Water Level");
            probeAlarm = Alarm.Subscribe ("Water level probe disconnected");
            
            string path = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "waterLevelProperties.json");

            using (StreamReader reader = File.OpenText (path)) {
                JObject jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));
                highLevelAlarmSetpoint = Convert.ToSingle (jo ["highLevelAlarmSetpoint"]);
                lowLevelAlarmSetpoint = Convert.ToSingle (jo ["lowLevelAlarmSetpoint"]);

                string name = Convert.ToString (jo ["sensor"] ["Group"]);
                byte b = (byte)AnalogInput.GetCardIndex (name);
                levelSensor.Group = b;

                levelSensor.Individual = Convert.ToByte (jo ["sensor"] ["Individual"]);
            }

            AnalogInput.AddChannel (levelSensor, AnalogType.Level, "Water Level");

            TaskManager.AddCyclicInterrupt ("Water Level", 1000, Run);
        }
        
        public static void Run () {
            _waterLevel = AnalogInput.GetValue (levelSensor);
            _waterLevel = _waterLevel.Map (819.2f, 4096, 0.0f, 15.0f);
            
            if (_waterLevel <= -1.0f)
                Alarm.Post (probeAlarm);
            else {
                if (Alarm.CheckAlarming (probeAlarm)) {
                    Alarm.Clear (probeAlarm);
                }
            }
            
            if ((_waterLevel <= lowLevelAlarmSetpoint) && (_waterLevel > -1.0f))
                Alarm.Post (lowAlarm);
            else {
                if (Alarm.CheckAlarming (lowAlarm)) {
                    Alarm.Clear (lowAlarm);
                }
            }
                
            if (_waterLevel >= highLevelAlarmSetpoint)
                Alarm.Post (highAlarm);
            else {
                if (Alarm.CheckAlarming (highAlarm)) {
                    Alarm.Clear (highAlarm);
                }
            }
        }
    }
}