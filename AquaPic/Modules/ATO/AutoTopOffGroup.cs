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
        private class AutoTopOffGroup
        {
            public string name;
            public bool enable;
            public AutoTopOffState state;

            public string requestBitName;
            public string waterLevelGroupName;
            public uint maximumRuntime;
            public uint minimumCooldown;

            public bool useAnalogSensors;
            public float analogOnSetpoint;
            public float analogOffSetpoint;

            public bool useFloatSwitches;

            public IntervalTimer timer;
            public uint atoTime {
                get {
                    if (state == AutoTopOffState.Filling)
                        return maximumRuntime - timer.secondsRemaining;
                    if (state == AutoTopOffState.Cooldown)
                        return timer.secondsRemaining;

                    return 0;
                }
            }

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

                failAlarmIndex = Alarm.Subscribe (string.Format ("{0}, ATO, failed", name));
            }

            public void GroupRun () {
                if (enable) {
                    bool pumpOnRequest;
                    switch (state) {
                    case AutoTopOffState.Standby:
                        // This is true because if the analog sensor isn't used then we want the float switch to control the ATO request
                        pumpOnRequest = true;

                        if (useAnalogSensors && WaterLevel.GetWaterLevelGroupAnalogSensorConnected (waterLevelGroupName)) {
                            pumpOnRequest = WaterLevel.GetWaterLevelGroupLevel (waterLevelGroupName) < analogOnSetpoint;
                        }

                        if (useFloatSwitches) {
                            pumpOnRequest &= WaterLevel.GetWaterLevelGroupAtoSwitchesActivated (waterLevelGroupName);
                        }

                        if (pumpOnRequest) {
                            state = AutoTopOffState.Filling;
                            Logger.Add ("Starting auto top off");
                            timer.Reset ();
                            timer.totalSeconds = maximumRuntime;
                            timer.Start ();
                        }

                        break;
                    case AutoTopOffState.Filling:
                        pumpOnRequest = true;

                        // Check analog sensor
                        if (useAnalogSensors && WaterLevel.GetWaterLevelGroupAnalogSensorConnected (waterLevelGroupName)) {
                            // If the level is greater than the off setpoint the request is off
                            pumpOnRequest = WaterLevel.GetWaterLevelGroupLevel (waterLevelGroupName) < analogOffSetpoint;
                        }

                        // check float switch
                        if ((useFloatSwitches) && (!WaterLevel.GetWaterLevelGroupAtoSwitchesActivated (waterLevelGroupName))) {
                            pumpOnRequest = false;
                        }

                        if (!pumpOnRequest) {
                            state = AutoTopOffState.Cooldown;
                            timer.Reset ();
                            Logger.Add ("Stopping auto top off. Runtime: {0} secs", timer.totalSeconds - timer.secondsRemaining);
                            timer.totalSeconds = minimumCooldown;
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

            public bool ClearAtoAlarm () {
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
