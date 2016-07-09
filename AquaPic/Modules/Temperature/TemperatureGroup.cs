using System;
using AquaPic.Runtime;

namespace AquaPic.Modules
{
    public partial class Temperature
    {
        private class TemperatureGroup
        {
            public string name;
            public float temperature;
            public DataLogger dataLogger;

            public float highTemperatureAlarmSetpoint;
            public float lowTemperatureAlarmSetpoint;
            public float temperatureSetpoint;
            public float temperatureDeadband;

            public int highTemperatureAlarmIndex;
            public int lowTemperatureAlarmIndex;

            public TemperatureGroup (
                string name, 
                float highTemperatureAlarmSetpoint, 
                float lowTemperatureAlarmSetpoint,
                float temperatureSetpoint,
                float temperatureDeadband
            ) {
                this.name = name;
                this.highTemperatureAlarmSetpoint = highTemperatureAlarmSetpoint;
                this.lowTemperatureAlarmSetpoint = lowTemperatureAlarmSetpoint;
                this.temperatureSetpoint = temperatureSetpoint;
                this.temperatureDeadband = temperatureDeadband;

                temperature = 0.0f;
                dataLogger = new DataLogger (string.Format ("{0}Temperature", this.name));

                highTemperatureAlarmIndex = Alarm.Subscribe (string.Format("{0} high temperature", name));
                lowTemperatureAlarmIndex = Alarm.Subscribe (string.Format ("{0} low temperature", name));
            }

            public void Run () {
                var probeCount = 0;

                temperature = 0.0f;
                foreach (var probe in probes) {
                    if (probe.temperatureGroupName == name) {
                        if (probe.GetTemperature ()) {
                            temperature += probe.temperature;
                            ++probeCount;
                        }
                    }
                }

                if (probeCount != 0) {
                    temperature /= probeCount;

                    if (temperature > highTemperatureAlarmSetpoint) {
                        if (!Alarm.CheckAlarming (highTemperatureAlarmIndex)) {
                            Alarm.Post (highTemperatureAlarmIndex);
                            dataLogger.AddEntry ("high alarm");
                        }
                    } else {
                        if (Alarm.CheckAlarming (highTemperatureAlarmIndex)) {
                            Alarm.Clear (highTemperatureAlarmIndex);
                        }
                    }

                    if (temperature < lowTemperatureAlarmSetpoint) {
                        if (!Alarm.CheckAlarming (lowTemperatureAlarmIndex)) {
                            Alarm.Post (lowTemperatureAlarmIndex);
                            dataLogger.AddEntry ("low alarm");
                        }
                    } else {
                        if (Alarm.CheckAlarming (lowTemperatureAlarmIndex)) {
                            Alarm.Clear (lowTemperatureAlarmIndex);
                        }
                    }

                    dataLogger.AddEntry (temperature);
                } else {
                    dataLogger.AddEntry ("no probes");
                }
            }
        }
    }
}

