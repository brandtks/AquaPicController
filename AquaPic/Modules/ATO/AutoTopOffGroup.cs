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
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Sensors;
using AquaPic.Equipment;
using AquaPic.Operands;

namespace AquaPic.Modules
{
    public partial class AutoTopOff
    {
        class AutoTopOffGroup
        {
            public string name;
            public bool enable;
            public AutoTopOffState state;

            public string requestBitName;
            public string waterLevelGroupName;

            public IntervalTimer timer;
            public uint atoTime {
                get {
                    if (state == AutoTopOffState.Filling)
                        return (maximumRuntime * 60) - timer.secondsRemaining;
                    if (state == AutoTopOffState.Cooldown)
                        return timer.secondsRemaining;

                    return 0;
                }
            }
            public uint maximumRuntime;
            public uint minimumCooldown;

            public bool useAnalogSensors;
            public float analogOnSetpoint;
            public float analogOffSetpoint;

            public bool useFloatSwitches;

            public int failAlarmIndex;

            public AutoTopOffGroup (
                string name,
                bool enable,
                string requestBitName,
                string waterLevelGroupName,
                uint maximumRuntime,
                uint minimumCooldown,
                bool useAnalogSensors,
                float analogOnSetpoint,
                float analogOffSetpoint,
                bool useFloatSwitches
            ) {
                this.name = name;
                this.enable = enable;
                this.requestBitName = requestBitName;
                this.waterLevelGroupName = waterLevelGroupName;
                this.maximumRuntime = maximumRuntime;
                this.minimumCooldown = minimumCooldown;
                this.useAnalogSensors = useAnalogSensors;
                this.analogOnSetpoint = analogOnSetpoint;
                this.analogOffSetpoint = analogOffSetpoint;
                this.useFloatSwitches = useFloatSwitches;

                if (enable) {
                    state = AutoTopOffState.Standby;
                } else {
                    state = AutoTopOffState.Off;
                }

                Bit.Reset (this.requestBitName);

                timer = IntervalTimer.GetTimer (name);
                timer.TimerElapsedEvent += OnTimerElapsed;
                timer.Reset ();

                failAlarmIndex = Alarm.Subscribe (string.Format ("{0}, ATO, failed", name));
            }

            public void GroupRun () {
                if (enable) {
                    bool pumpOnRequest;
                    switch (state) {
                    case AutoTopOffState.Standby:
                        // If the water level group isn't alarming on high allow pump on request
                        pumpOnRequest = !WaterLevel.GetWaterLevelGroupHighAlarming (waterLevelGroupName);

                        if (useAnalogSensors) {
                            if (WaterLevel.GetWaterLevelGroupAnalogSensorConnected (waterLevelGroupName)) {
                                pumpOnRequest &= WaterLevel.GetWaterLevelGroupLevel (waterLevelGroupName) < analogOnSetpoint;
                            } else {
                                pumpOnRequest = false;
                            }
                        }

                        if (useFloatSwitches) {
                            pumpOnRequest &= WaterLevel.GetWaterLevelGroupSwitchesActivated (waterLevelGroupName);
                        }

                        if (pumpOnRequest) {
                            state = AutoTopOffState.Filling;
                            Logger.Add ("Starting auto top off");
                            timer.Reset ();
                            timer.totalSeconds = maximumRuntime * 60;
                            timer.Start ();
                        }

                        break;
                    case AutoTopOffState.Filling:
                        pumpOnRequest = !WaterLevel.GetWaterLevelGroupHighAlarming (waterLevelGroupName); ;

                        // Check analog sensor
                        if (useAnalogSensors) {
                            if (WaterLevel.GetWaterLevelGroupAnalogSensorConnected (waterLevelGroupName)) {
                                // If the level is greater than the off setpoint the request is off
                                pumpOnRequest &= WaterLevel.GetWaterLevelGroupLevel (waterLevelGroupName) < analogOffSetpoint;
                            } else {
                                pumpOnRequest = false;
                            }
                        }

                        // check float switch
                        if ((useFloatSwitches) && (!WaterLevel.GetWaterLevelGroupSwitchesActivated (waterLevelGroupName))) {
                            pumpOnRequest = false;
                        }

                        if (!pumpOnRequest) {
                            state = AutoTopOffState.Cooldown;
                            timer.Reset ();
                            Logger.Add ("Stopping auto top off. Runtime: {0} secs", timer.totalSeconds - timer.secondsRemaining);
                            timer.totalSeconds = minimumCooldown * 60;
                            timer.Start ();
                        }

                        break;
                    case AutoTopOffState.Cooldown:
                    case AutoTopOffState.Error:
                    default:
                        pumpOnRequest = false;
                        break;
                    }

                    if (pumpOnRequest) {
                        Bit.Set (requestBitName);
                    } else {
                        Bit.Reset (requestBitName);
                    }
                } else {
                    state = AutoTopOffState.Off;
                    Bit.Reset (requestBitName);
                }
            }

            protected void OnTimerElapsed (object sender, TimerElapsedEventArgs args) {
                if (state == AutoTopOffState.Filling) {
                    Bit.Reset (requestBitName);
                    state = AutoTopOffState.Error;
                    Alarm.Post (failAlarmIndex);
                } else if (state == AutoTopOffState.Cooldown) {
                    state = AutoTopOffState.Standby;
                }
            }

            public bool ClearAlarm () {
                if (state == AutoTopOffState.Error) {
                    if (Alarm.CheckAcknowledged (failAlarmIndex)) {
                        Alarm.Clear (failAlarmIndex);
                        state = AutoTopOffState.Standby;
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
