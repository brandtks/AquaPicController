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
        private class AnalogSensor
        {
            public bool enable;
            public float waterLevel;
            public int lowAlarm;
            public int highAlarm;
            public int sensorAlarm;

            public float highLevelAlarmSetpoint;
            public float lowLevelAlarmSetpoint;
            public IndividualControl sensorChannel;

            public AnalogSensor (bool enable) {
                waterLevel = 0.0f;
                this.enable = enable;
                TaskManager.AddCyclicInterrupt ("Water Level", 1000, Run);

                lowAlarm = -1;
                highAlarm = -1;
                sensorAlarm = -1;

                highLevelAlarmSetpoint = 0.0f;
                lowLevelAlarmSetpoint = 0.0f;

                sensorChannel = new IndividualControl ();
            }

            public AnalogSensor (bool enable, float highAlarmSetpoint, float lowAlarmSetPoint, IndividualControl ic) 
                : this (enable) 
            {
                lowAlarm = Alarm.Subscribe ("Low Water Level");
                highAlarm = Alarm.Subscribe ("High Water Level");
                sensorAlarm = Alarm.Subscribe ("Water level probe disconnected");

                highLevelAlarmSetpoint = highAlarmSetpoint;
                lowLevelAlarmSetpoint = lowAlarmSetPoint;

                sensorChannel = ic;

                AnalogInput.AddChannel (sensorChannel, AnalogType.Level, "Water Level");
            }

            public void Run () {
                if (enable) {
                    waterLevel = AnalogInput.GetValue (sensorChannel);
                    waterLevel = waterLevel.Map (819.2f, 4096, 0.0f, 15.0f);

                    if (waterLevel <= -1.0f)
                        Alarm.Post (sensorAlarm);
                    else {
                        if (Alarm.CheckAlarming (sensorAlarm)) {
                            Alarm.Clear (sensorAlarm);
                        }
                    }

                    if ((waterLevel <= lowLevelAlarmSetpoint) && (waterLevel > -1.0f))
                        Alarm.Post (lowAlarm);
                    else {
                        if (Alarm.CheckAlarming (lowAlarm)) {
                            Alarm.Clear (lowAlarm);
                        }
                    }

                    if (waterLevel >= highLevelAlarmSetpoint)
                        Alarm.Post (highAlarm);
                    else {
                        if (Alarm.CheckAlarming (highAlarm)) {
                            Alarm.Clear (highAlarm);
                        }
                    }
                }
            }
        }
    }
}

