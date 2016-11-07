using System;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Sensors
{
    public class AnalogLevelSensor
    {
        private bool _enable;
        public bool enable {
            get {
                return _enable;
            }
            set {
                _enable = value;
                if (_enable) {
                    sensorDisconnectedAlarmIndex = Alarm.Subscribe ("Probe disconnected, " + name);
                } else {
                    Alarm.Clear (highAnalogAlarmIndex);
                    Alarm.Clear (lowAnalogAlarmIndex);
                    Alarm.Clear (sensorDisconnectedAlarmIndex);
                    sensorDisconnectedAlarmIndex = -1;
                }
            }
        }

        public bool connected {
            get {
                return !Alarm.CheckAlarming (sensorDisconnectedAlarmIndex);
            }
        }
        public float level;

        public float zeroValue;
        public float fullScaleActual;
        public float fullScaleValue;

        public string name;

        public float highAlarmSetpoint;
        public int highAnalogAlarmIndex;
        public bool enableHighAlarm {
            get {
                return highAnalogAlarmIndex != -1;
            }
            set {
                if (value) {
                    highAnalogAlarmIndex = Alarm.Subscribe ("High level, " + name);
                } else {
                    highAnalogAlarmIndex = -1;
                }
            }
        }

        public float lowAlarmSetpoint;
        public int lowAnalogAlarmIndex;
        public bool enableLowAlarm {
            get {
                return lowAnalogAlarmIndex != -1;
            }
            set {
                if (value) {
                    lowAnalogAlarmIndex = Alarm.Subscribe ("Low level, " + name);
                } else {
                    lowAnalogAlarmIndex = -1;
                }
            }
        }

        public int sensorDisconnectedAlarmIndex;
        public bool enableDisconnectedAlarm {
            get {
                return sensorDisconnectedAlarmIndex != -1;
            }
            set {
                if (value) {
                    sensorDisconnectedAlarmIndex = Alarm.Subscribe ("Probe disconnected, " + name);
                } else {
                    sensorDisconnectedAlarmIndex = -1;
                }
            }
        }

        public IndividualControl sensorChannel;
        public DataLogger dataLogger;

        public AnalogLevelSensor (
            string name,
            IndividualControl ic,
            float highAlarmSetpoint, 
            float lowAlarmSetpoint, 
            bool enable,
            bool enableHighAlarm,
            bool enableLowAlarm
        ) {
            this.name = name;

            this.enable = enable;
            level = 0.0f;

            zeroValue = 819.2f;
            fullScaleActual = 15.0f;
            fullScaleValue = 4096.0f;
        
            if (enableHighAlarm && enable) {
                highAnalogAlarmIndex = Alarm.Subscribe ("High level, " + name);
            } else {
                highAnalogAlarmIndex = -1;
            }

            if (enableLowAlarm && enable) {
                lowAnalogAlarmIndex = Alarm.Subscribe ("Low level, " + name);
            } else {
                lowAnalogAlarmIndex = -1;
            }

            if (enable) {
                sensorDisconnectedAlarmIndex = Alarm.Subscribe ("Probe disconnected, " + name);
            } else {
                sensorDisconnectedAlarmIndex = -1;
            }
            
            this.highAlarmSetpoint = highAlarmSetpoint;
            this.lowAlarmSetpoint = lowAlarmSetpoint;

            sensorChannel = ic;

            dataLogger = new DataLogger ("WaterLevel" + name);

            if (sensorChannel.IsNotEmpty ()) {
                AquaPicDrivers.AnalogInput.AddChannel (sensorChannel, name);
            }
        }

        public AnalogLevelSensor (string name, IndividualControl ic)
            : this (name, ic, 0.0f, 0.0f, true, false, false) { }

        public AnalogLevelSensor (string name, IndividualControl ic, float highAlarmSetpoint)
            : this (name, ic, highAlarmSetpoint, 0.0f, true, true, false) { }

        public AnalogLevelSensor (string name, IndividualControl ic, float highAlarmSetpoint, float lowAlarmSetpoint)
            : this (name, ic, highAlarmSetpoint, lowAlarmSetpoint, true, true, true) { }

        public AnalogLevelSensor (string name, IndividualControl ic, float highAlarmSetpoint, float lowAlarmSetpoint, bool enable)
            : this (name, ic, highAlarmSetpoint, lowAlarmSetpoint, enable, true, true) { }

        public void Run () {
            if (enable) {
                level = AquaPicDrivers.AnalogInput.GetChannelValue (sensorChannel);
                level = level.Map (zeroValue, fullScaleValue, 0.0f, fullScaleActual);

                if (level < 0.0f) {
                    if (!Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)) {
                        Alarm.Post (sensorDisconnectedAlarmIndex);
                        dataLogger.AddEntry ("disconnected alarm");
                    }
                } else {
                    if (Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)) {
                        Alarm.Clear (sensorDisconnectedAlarmIndex);
                    }
                }

                if ((level <= lowAlarmSetpoint) && (connected)) {
                    if (!Alarm.CheckAlarming (lowAnalogAlarmIndex)) {
                        Alarm.Post (lowAnalogAlarmIndex);
                        dataLogger.AddEntry ("low alarm");
                    }
                } else {
                    if (Alarm.CheckAlarming (lowAnalogAlarmIndex)) {
                        Alarm.Clear (lowAnalogAlarmIndex);
                    }
                }

                if (level >= highAlarmSetpoint) {
                    if (!Alarm.CheckAlarming (highAnalogAlarmIndex)) {
                        Alarm.Post (highAnalogAlarmIndex);
                        dataLogger.AddEntry ("high alarm");
                    }
                } else {
                    if (Alarm.CheckAlarming (highAnalogAlarmIndex)) {
                        Alarm.Clear (highAnalogAlarmIndex);
                    }
                }

                if (level < 0.0f) {
                    dataLogger.AddEntry ("probe disconnected");
                } else {
                    dataLogger.AddEntry (level);
                }
            } else {
                if (Alarm.CheckAlarming (lowAnalogAlarmIndex)) {
                    Alarm.Clear (lowAnalogAlarmIndex);
                }

                if (Alarm.CheckAlarming (highAnalogAlarmIndex)) {
                    Alarm.Clear (highAnalogAlarmIndex);
                }

                if (Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)) {
                    Alarm.Clear (sensorDisconnectedAlarmIndex);
                }
            }
        }
    }
}

