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
            public int lowAlarmIndex;
            public int highAlarmIndex;
            public int sensorAlarmIndex;

            public float highAlarmStpnt;
            public float lowAlarmStpnt;
            public IndividualControl sensorChannel;

            public AnalogSensor (bool enable, float highAlarmSetpoint, float lowAlarmSetPoint, IndividualControl ic) {
                waterLevel = 0.0f;
                this.enable = enable;

                if (this.enable) 
                    SubscribeToAlarms ();
                else {
                    lowAlarmIndex = -1;
                    highAlarmIndex = -1;
                    sensorAlarmIndex = -1;
                }

                this.highAlarmStpnt = highAlarmSetpoint;
                this.lowAlarmStpnt = lowAlarmSetPoint;

                sensorChannel = ic;

                if (this.enable)
                    AnalogInput.AddChannel (sensorChannel, AnalogType.Level, "Water Level");

                TaskManager.AddCyclicInterrupt ("Analog Water Level", 1000, Run);
            }

            public void Run () {
                if (enable) {
                    waterLevel = AnalogInput.GetValue (sensorChannel);
                    waterLevel = waterLevel.Map (819.2f, 4096, 0.0f, 15.0f);

                    if (waterLevel <= -1.0f)
                        Alarm.Post (sensorAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (sensorAlarmIndex)) {
                            Alarm.Clear (sensorAlarmIndex);
                        }
                    }

                    if ((waterLevel <= lowAlarmStpnt) && (waterLevel > -1.0f))
                        Alarm.Post (lowAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (lowAlarmIndex)) {
                            Alarm.Clear (lowAlarmIndex);
                        }
                    }

                    if (waterLevel >= highAlarmStpnt)
                        Alarm.Post (highAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (highAlarmIndex)) {
                            Alarm.Clear (highAlarmIndex);
                        }
                    }
                }
            }

            public void SubscribeToAlarms () {
                lowAlarmIndex = Alarm.Subscribe ("Low Water Level");
                highAlarmIndex = Alarm.Subscribe ("High Water Level");
                sensorAlarmIndex = Alarm.Subscribe ("Water level probe disconnected");
            }
        }
    }
}

