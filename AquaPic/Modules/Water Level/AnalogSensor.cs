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
            public bool connected {
                get {
                    return !Alarm.CheckAlarming (sensorDisconnectedAlarmIndex);
                }
            }
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
            public DataLogger dataLogger;

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

                dataLogger = new DataLogger ("WaterLevel");

                if (enable) {
                    AquaPicDrivers.AnalogInput.AddChannel (sensorChannel, "Water Level");
                }
            }

            public void Run () {
                if (enable) {
                    waterLevel = AquaPicDrivers.AnalogInput.GetChannelValue (sensorChannel);
                    waterLevel = waterLevel.Map (zeroValue, fullScaleValue, 0.0f, fullScaleActual);

                    if (waterLevel < 0.0f)
                        Alarm.Post (sensorDisconnectedAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)) {
                            Alarm.Clear (sensorDisconnectedAlarmIndex);
                        }
                    }

                    if ((waterLevel <= lowAlarmStpnt) && (connected))
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

                    dataLogger.AddEntry (waterLevel);
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

