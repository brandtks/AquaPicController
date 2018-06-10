#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;
using AquaPic.Sensors;
using AquaPic.DataLogging;

namespace AquaPic.Modules
{
    public partial class WaterLevel
    {
        private class WaterLevelGroup
        {
            public string name;
            public float level;
            public IDataLogger dataLogger;

            public float highAnalogAlarmSetpoint;
            public bool enableHighAnalogAlarm;
            public int highAnalogAlarmIndex;

            public float lowAnalogAlarmSetpoint;
            public bool enableLowAnalogAlarm;
            public int lowAnalogAlarmIndex;

            public int highSwitchAlarmIndex;
            public int lowSwitchAlarmIndex;

            public WaterLevelGroup (
                string name,
                float highAnalogAlarmSetpoint,
                bool enableHighAnalogAlarm,
                float lowAnalogAlarmSetpoint,
                bool enableLowAnalogAlarm
            ) {
                this.name = name;
                level = 0.0f;
                dataLogger = Factory.GetDataLogger (string.Format ("{0}WaterLevel", this.name.RemoveWhitespace ()));

                this.highAnalogAlarmSetpoint = highAnalogAlarmSetpoint;
                this.enableHighAnalogAlarm = enableHighAnalogAlarm;
                if (this.enableHighAnalogAlarm) {
                    highAnalogAlarmIndex = Alarm.Subscribe (string.Format ("{0} High Water Level (Analog)", this.name));
                }
                Alarm.AddAlarmHandler (highAnalogAlarmIndex, OnHighAlarm);

                this.lowAnalogAlarmSetpoint = lowAnalogAlarmSetpoint;
                this.enableLowAnalogAlarm = enableLowAnalogAlarm;
                if (this.enableLowAnalogAlarm) {
                    lowAnalogAlarmIndex = Alarm.Subscribe (string.Format ("{0} Low Water Level (Analog)", this.name));
                }
                Alarm.AddAlarmHandler (lowAnalogAlarmIndex, OnLowAlarm);

                highSwitchAlarmIndex = Alarm.Subscribe (string.Format ("{0} High Water Level (Switch)", this.name));
                lowSwitchAlarmIndex = Alarm.Subscribe (string.Format ("{0} Low Water Level (Switch)", this.name));
                Alarm.AddAlarmHandler (highSwitchAlarmIndex, OnHighAlarm);
                Alarm.AddAlarmHandler (lowSwitchAlarmIndex, OnLowAlarm);
            }

            public void GroupRun () {
                var analogSensorCount = 0;
                level = 0;
                foreach (var s in analogLevelSensors.Values) {
                    if (s.waterLevelGroupName == name) {
                        s.Get ();
                        if (s.connected) {
                            level += s.level;
                            analogSensorCount++;
                        }
                    }
                }

                if (analogSensorCount > 0) {
                    level /= analogSensorCount;
                    dataLogger.AddEntry (level);

                    if (enableHighAnalogAlarm && (level > highAnalogAlarmSetpoint)) {
                        Alarm.Post (highAnalogAlarmIndex);
                    } else {
                        Alarm.Clear (highAnalogAlarmIndex);
                    }

                    if (enableLowAnalogAlarm && (level < lowAnalogAlarmSetpoint)) {
                        Alarm.Post (lowAnalogAlarmIndex);
                    } else {
                        Alarm.Clear (lowAnalogAlarmIndex);
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
        }
    }
}
