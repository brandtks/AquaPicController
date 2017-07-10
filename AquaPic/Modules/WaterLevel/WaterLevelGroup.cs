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

using System;
using System.Collections.Generic;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.Sensors;

namespace AquaPic.Modules
{
    public partial class WaterLevel
    {
        private class WaterLevelGroup
        {
            public string name;
            public float level;
            public DataLogger dataLogger;
            public string analogLevelSensorName;

            public int highSwitchAlarmIndex;
            public int lowSwitchAlarmIndex;

            public WaterLevelGroup (string name, string analogLevelSensorName) {
                this.name = name;
                level = 0.0f;
                dataLogger = new DataLogger (string.Format ("{0}WaterLevel", this.name.RemoveWhitespace ()));

                highSwitchAlarmIndex = Alarm.Subscribe ("High Water Level, Float Switch");
                lowSwitchAlarmIndex = Alarm.Subscribe ("Low Water Level, Float Switch");
                Alarm.AddAlarmHandler (highSwitchAlarmIndex, OnHighAlarm);
                Alarm.AddAlarmHandler (lowSwitchAlarmIndex, OnLowAlarm);

                if (CheckAnalogLevelSensorKeyNoThrow (analogLevelSensorName)) {
                    this.analogLevelSensorName = analogLevelSensorName;
                } else {
                    this.analogLevelSensorName = string.Empty;
                }

                ConnectAnalogSensorAlarmsToDataLogger ();
            }

            public void Run () {
                if (analogLevelSensorName.IsNotEmpty ()) {
                    var sensor = analogLevelSensors[analogLevelSensorName];
                    if (sensor.connected) {
                        level = sensor.level;
                        dataLogger.AddEntry (level);
                    } else {
                        level = 0f;
                        dataLogger.AddEntry ("probe disconnected");
                    }
                } else {
                    dataLogger.AddEntry ("probe disconnected");
                }

                foreach (var s in floatSwitches.Values) {
                    if (s.waterLevelGroupName == name) {
                        s.Get ();

                        if (s.function == SwitchFunction.HighLevel) {
                            if (s.activated)
                                Alarm.Post (highSwitchAlarmIndex);
                            else {
                                Alarm.Clear (highSwitchAlarmIndex);
                            }
                        } else if (s.function == SwitchFunction.LowLevel) {
                            if (s.activated)
                                Alarm.Post (lowSwitchAlarmIndex);
                            else {
                                Alarm.Clear (lowSwitchAlarmIndex);
                            }
                        }
                    }
                }
            }

            public void ConnectAnalogSensorAlarmsToDataLogger () {
                if (analogLevelSensorName.IsNotEmpty ()) {
                    var sensor = analogLevelSensors[analogLevelSensorName];
                    Alarm.AddAlarmHandler (sensor.highAlarmIndex, OnHighAlarm);
                    Alarm.AddAlarmHandler (sensor.lowAlarmIndex, OnLowAlarm);
                    Alarm.AddAlarmHandler (sensor.disconnectedAlarmIndex, OnDisconnectedAlarm);
                }
            }

            public void DisconnectAnalogSensorAlarmsFromDataLogger () {
                if (analogLevelSensorName.IsNotEmpty ()) {
                    var sensor = analogLevelSensors[analogLevelSensorName];
                    Alarm.RemoveAlarmHandler (sensor.highAlarmIndex, OnHighAlarm);
                    Alarm.RemoveAlarmHandler (sensor.lowAlarmIndex, OnLowAlarm);
                    Alarm.RemoveAlarmHandler (sensor.disconnectedAlarmIndex, OnDisconnectedAlarm);
                }
            }

            protected void OnHighAlarm (object sender, AlarmEventArgs args) {
                if (args.type == AlarmEventType.Posted) {
                    dataLogger.AddEntry ("high alarm");
                }
            }

            protected void OnLowAlarm (object sender, AlarmEventArgs args) {
                if (args.type == AlarmEventType.Posted) {
                    dataLogger.AddEntry ("low alarm");
                }
            }

            protected void OnDisconnectedAlarm (object sender, AlarmEventArgs args) {
                if (args.type == AlarmEventType.Posted) {
                    dataLogger.AddEntry ("disconnected alarm");
                }
            }
        }
    }
}