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
using System.Collections.Generic;
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;
using AquaPic.Sensors;
using AquaPic.DataLogging;
using AquaPic.PubSub;

namespace AquaPic.Modules
{
    public partial class WaterLevel
    {
        class WaterLevelGroup : SensorConsumer
        {
            public string name;
            public float highAnalogAlarmSetpoint;
            public float lowAnalogAlarmSetpoint;
            public bool enableHighAnalogAlarm;
            public bool enableLowAnalogAlarm;
            public List<string> floatSwitches;
            public Dictionary<string, InternalWaterLevelSensorState> waterLevelSensors;

            public float level;
            public IDataLogger dataLogger;
            public int highAnalogAlarmIndex;
            public int lowAnalogAlarmIndex;
            public int highSwitchAlarmIndex;
            public int lowSwitchAlarmIndex;

            public WaterLevelGroup (
                string name,
                float highAnalogAlarmSetpoint,
                bool enableHighAnalogAlarm,
                float lowAnalogAlarmSetpoint,
                bool enableLowAnalogAlarm,
                IEnumerable<string> floatSwitches,
                IEnumerable<string> waterLevelSensors) 
            {
                this.name = name;
                level = 0f;
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

                this.floatSwitches = new List<string> (floatSwitches);
                foreach (var floatSwitch in this.floatSwitches) {
                    AquaPicSensors.FloatSwitches.SubscribeConsumer (floatSwitch, this);
                }

                this.waterLevelSensors = new Dictionary<string, InternalWaterLevelSensorState> ();
                foreach (var waterLevelSensor in waterLevelSensors) {
                    this.waterLevelSensors.Add (waterLevelSensor, new InternalWaterLevelSensorState ());
                    AquaPicSensors.WaterLevelSensors.SubscribeConsumer (waterLevelSensor, this);
                }
            }

            public void GroupRun () {
                if (waterLevelSensors.Count > 0) {
                    dataLogger.AddEntry (level);
                } else {
                    dataLogger.AddEntry ("probe disconnected");
                }
            }

            // Using event handler because either float switches or water level sensors can trip a high alarm
            protected void OnHighAlarm (object sender, AlarmEventArgs args) {
                if (args.type == AlarmEventType.Posted) {
                    dataLogger.AddEntry ("high alarm");
                }
            }

            // Using event handler because either float switches or water level sensors can trip a low alarm
            protected void OnLowAlarm (object sender, AlarmEventArgs args) {
                if (args.type == AlarmEventType.Posted) {
                    dataLogger.AddEntry ("low alarm");
                }
            }

            public override void OnSensorUpdatedAction (object parm) {
                var args = parm as SensorUpdatedEvent;
                if (waterLevelSensors.ContainsKey (args.name)) {
                    if (args.name != args.settings.name) {
                        var sensorState = waterLevelSensors[args.name];
                        waterLevelSensors.Remove (args.name);
                        waterLevelSensors[args.settings.name] = sensorState;
                    }
                } else if (floatSwitches.Contains (args.name)) {
                    if (args.name != args.settings.name) {
                        var index = floatSwitches.IndexOf (args.name);
                        floatSwitches[index] = args.settings.name;
                    }
                }
                UpdateWaterLevelGroupSettingsInFile (name);
            }

            public override void OnSensorRemovedAction (object parm) {
                var args = parm as SensorRemovedEvent;
                if (waterLevelSensors.ContainsKey (args.name)) {
                    waterLevelSensors.Remove (args.name);
                    CalculateWaterLevel ();
                } else if (floatSwitches.Contains (args.name)) {
                    floatSwitches.Remove (args.name);
                }
                UpdateWaterLevelGroupSettingsInFile (name);
            }

            public override void OnValueChangedAction (object parm) {
                var args = parm as ValueChangedEvent;
                if (waterLevelSensors.ContainsKey (args.name)) {
                    var waterLevelSensor = (WaterLevelSensor)AquaPicSensors.WaterLevelSensors.GetSensor (args.name);
                    waterLevelSensors[waterLevelSensor.name].connected = waterLevelSensor.connected;
                    waterLevelSensors[waterLevelSensor.name].level = waterLevelSensor.level;
                    CalculateWaterLevel ();
                } else {
                    if (floatSwitches.Contains (args.name)) {
                        var floatSwitch = (FloatSwitch)AquaPicSensors.FloatSwitches.GetSensor (args.name);
                        if (floatSwitch.switchFuntion == SwitchFunction.HighLevel) {
                            if (floatSwitch.activated)
                                Alarm.Post (highSwitchAlarmIndex);
                            else {
                                Alarm.Clear (highSwitchAlarmIndex);
                            }
                        } else if (floatSwitch.switchFuntion == SwitchFunction.LowLevel) {
                            if (floatSwitch.activated)
                                Alarm.Post (lowSwitchAlarmIndex);
                            else {
                                Alarm.Clear (lowSwitchAlarmIndex);
                            }
                        }
                    }
                }
            }

            protected void CalculateWaterLevel () {
                level = 0;
                var connectedWaterLevelSensors = 0;
                foreach (var internalWaterLevelSensor in waterLevelSensors.Values) {
                    if (internalWaterLevelSensor.connected) {
                        level += internalWaterLevelSensor.level;
                        connectedWaterLevelSensors++;
                    }
                }
                level /= connectedWaterLevelSensors;

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
            }

            public class InternalWaterLevelSensorState
            {
                public bool connected;
                public float level;

                public InternalWaterLevelSensorState () {
                    connected = false;
                    level = 0f;
                }
            }
        }
    }
}
