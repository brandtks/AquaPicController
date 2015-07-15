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

            public int sensorAlarmIndex;
            public int lowAnalogAlarmIndex;
            public int highAnalogAlarmIndex;

            public float highAlarmStpnt;
            public float lowAlarmStpnt;
            public IndividualControl sensorChannel;

            public AnalogSensor (bool enable, float highAlarmSetpoint, float lowAlarmSetPoint, IndividualControl ic) {
                waterLevel = 0.0f;
                this.enable = enable;

                if (this.enable) 
                    SubscribeToAlarms ();
                else 
                    sensorAlarmIndex = -1;

                this.highAlarmStpnt = highAlarmSetpoint;
                this.lowAlarmStpnt = lowAlarmSetPoint;

                sensorChannel = ic;

                if (this.enable)
                    AnalogInput.AddChannel (sensorChannel, "Water Level");
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
                        Alarm.Post (lowAnalogAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (lowAnalogAlarmIndex)) {
                            Alarm.Clear (lowAnalogAlarmIndex);
                        }
                    }

                    if (waterLevel >= highAlarmStpnt)
                        Alarm.Post (highAnalogAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (highAnalogAlarmIndex)) {
                            Alarm.Clear (highAnalogAlarmIndex);
                        }
                    }
                }
            }

            public void SubscribeToAlarms () {
                lowAnalogAlarmIndex = Alarm.Subscribe ("Low Water Level, Analog Sensor");
                highAnalogAlarmIndex = Alarm.Subscribe ("High Water Level, Analog Sensor");
                sensorAlarmIndex = Alarm.Subscribe ("Analog water level probe disconnected");
            }
        }
    }
}

