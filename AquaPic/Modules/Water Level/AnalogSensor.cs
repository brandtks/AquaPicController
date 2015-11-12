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

            public float zeroValue;
            public float fullScaleActual;
            public float fullScaleValue;

            public int sensorDisconnectedAlarmIndex;
            public int lowAnalogAlarmIndex;
            public int highAnalogAlarmIndex;

            public float highAlarmStpnt;
            public float lowAlarmStpnt;
            public IndividualControl sensorChannel;

            public AnalogSensor (bool enable, float highAlarmSetpoint, float lowAlarmSetPoint, IndividualControl ic) {
                this.enable = enable;
                waterLevel = 0.0f;

                zeroValue = 819.2f;
                fullScaleActual = 15.0f;
                fullScaleValue = 4096.0f;

                if (this.enable) 
                    SubscribeToAlarms ();
                else {
                    sensorDisconnectedAlarmIndex = -1;
                    lowAnalogAlarmIndex = -1;
                    highAnalogAlarmIndex = -1;
                }

                this.highAlarmStpnt = highAlarmSetpoint;
                this.lowAlarmStpnt = lowAlarmSetPoint;

                sensorChannel = ic;

                if (this.enable)
                    AnalogInput.AddChannel (sensorChannel, "Water Level");
            }

            public void Run () {
                if (enable) {
                    waterLevel = AnalogInput.GetValue (sensorChannel);
                    waterLevel = waterLevel.Map (zeroValue, fullScaleValue, 0.0f, fullScaleActual);

                    if (waterLevel <= -1.0f)
                        Alarm.Post (sensorDisconnectedAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)) {
                            Alarm.Clear (sensorDisconnectedAlarmIndex);
                        }
                    }

                    if ((waterLevel <= lowAlarmStpnt) && (!Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)))
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
                sensorDisconnectedAlarmIndex = Alarm.Subscribe ("Analog water level probe disconnected");
            }
        }
    }
}

