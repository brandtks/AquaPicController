#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
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
                foreach (var probe in probes.Values) {
                    if (probe.temperatureGroupName == name) {
                        probe.Get ();
                        if (probe.temperature >= probe.zeroActual) {
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

