using System;
using AquaPic.Sensors;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Modules
{
    public partial class WaterLevel
    {
        public class WaterLevelSensor : AnalogLevelSensor
        {
            private bool _enable;
            public bool enable {
                get {
                    return _enable;
                }
                set {
                    _enable = value;
                    if (!_enable) {
                        if (lowAnalogAlarmIndex != -1) {
                            Alarm.Clear (highAnalogAlarmIndex);
                        }

                        if (lowAnalogAlarmIndex != -1) {
                            Alarm.Clear (lowAnalogAlarmIndex);
                        }

                        if (sensorDisconnectedAlarmIndex != -1) {
                            Alarm.Clear (sensorDisconnectedAlarmIndex);
                        }
                    }
                }
            }

            public bool connected {
                get {
                    return !Alarm.CheckAlarming (sensorDisconnectedAlarmIndex);
                }
            }

            public float highAlarmSetpoint;
            public int highAnalogAlarmIndex = -1;
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
            public int lowAnalogAlarmIndex = -1;
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

            public DataLogger dataLogger;

            public WaterLevelSensor (
                string name,
                IndividualControl ic,
                float highAlarmSetpoint,
                float lowAlarmSetpoint,
                bool enable,
                bool enableHighAlarm,
                bool enableLowAlarm)
                : base (name, ic) {
                this.enable = enable;

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

                this.highAlarmSetpoint = highAlarmSetpoint;
                this.lowAlarmSetpoint = lowAlarmSetpoint;

                dataLogger = new DataLogger ("WaterLevel" + name);
            }

            public WaterLevelSensor (string name, IndividualControl ic)
            : this (name, ic, 0.0f, 0.0f, true, false, false) { }

            public WaterLevelSensor (string name, IndividualControl ic, float highAlarmSetpoint)
            : this (name, ic, highAlarmSetpoint, 0.0f, true, true, false) { }

            public WaterLevelSensor (string name, IndividualControl ic, float highAlarmSetpoint, float lowAlarmSetpoint)
            : this (name, ic, highAlarmSetpoint, lowAlarmSetpoint, true, true, true) { }

            public WaterLevelSensor (string name, IndividualControl ic, float highAlarmSetpoint, float lowAlarmSetpoint, bool enable)
            : this (name, ic, highAlarmSetpoint, lowAlarmSetpoint, enable, true, true) { }

            public void UpdateWaterLevel () {
                if (enable) {
                    Get ();

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
                    if (enableLowAlarm) {
                        if (Alarm.CheckAlarming (lowAnalogAlarmIndex)) {
                            Alarm.Clear (lowAnalogAlarmIndex);
                        }
                    }

                    if (enableHighAlarm) {
                        if (Alarm.CheckAlarming (highAnalogAlarmIndex)) {
                            Alarm.Clear (highAnalogAlarmIndex);
                        }
                    }

                    if (enableDisconnectedAlarm) {
                        if (Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)) {
                            Alarm.Clear (sensorDisconnectedAlarmIndex);
                        }
                    }
                }
            }
        }
    }
}
