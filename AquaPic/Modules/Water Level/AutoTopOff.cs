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
using AquaPic.Utilites;
using AquaPic.Sensors;
using AquaPic.Equipment;

namespace AquaPic.Modules
{
    public enum AutoTopOffState {
        Off,
        Standby,
        Filling,
        Cooldown,
        Error
    }

    public partial class WaterLevel
    {
        private class AutoTopOff
        {
            public bool enable;

            public bool useAnalogSensor;
            public float analogOnSetpoint;
            public float analogOffSetpoint;

            public bool useFloatSwitch;
            public bool floatSwitchActivated;

            public Pump pump;
            public bool pumpOnRequest;
            public IntervalTimer atoTimer;
            public uint maxPumpOnTime;
            public uint minPumpOffTime;

            public WaterLevelSensor reservoirLevel;
            public bool disableOnLowResevoirLevel;
            public float reservoirLowLevelAlarmSetpoint;
            public int reservoirLowLevelAlarmIndex;

            public AutoTopOffState state;
            public int atoFailAlarmIndex;

            public AutoTopOff (
                bool enable,
                bool useAnalogSensor,
                float analogOnSetpoint,
                float analogOffSetpoint,
                bool useFloatSwitch,
                IndividualControl pumpOutlet,
                uint maxPumpOnTime,
                uint minPumpOffTime
            ) {
                this.enable = enable;
                if (this.enable) {
                    state = AutoTopOffState.Standby;
                } else {
                    state = AutoTopOffState.Off;
                }

                atoFailAlarmIndex = Alarm.Subscribe ("Auto top off failed");

                this.useAnalogSensor = useAnalogSensor;
                this.analogOnSetpoint = analogOnSetpoint;
                this.analogOffSetpoint = analogOffSetpoint;

                this.useFloatSwitch = useFloatSwitch;
                floatSwitchActivated = false;

                atoTimer = IntervalTimer.GetTimer ("ATO");
                atoTimer.TimerElapsedEvent += OnTimerElapsed;

                pump = new Pump (pumpOutlet, "ATO pump", MyState.Off, "ATO");
                if (pump.outlet.IsNotEmpty ()) {
                    pump.SetGetter (() => pumpOnRequest);
                }
                pumpOnRequest = false;
                this.maxPumpOnTime = maxPumpOnTime;
                this.minPumpOffTime = minPumpOffTime;

                disableOnLowResevoirLevel = false;
                reservoirLevel = new WaterLevelSensor ("Top Off Reservoir", IndividualControl.Empty);
                reservoirLowLevelAlarmIndex = Alarm.Subscribe ("Low ATO Reservoir Level");
            }

            public void Run () {
                if (enable) {
                    if (reservoirLevel.channel.IsNotEmpty ()) {
                        reservoirLevel.Get ();
                        if (reservoirLevel.level < reservoirLowLevelAlarmSetpoint) {
                            Alarm.Post (reservoirLowLevelAlarmIndex);
                        }
                    }
                    
                    if (!Alarm.CheckAlarming (highSwitchAlarmIndex) || !Alarm.CheckAlarming (analogSensor.highAnalogAlarmIndex)) {
                        switch (state) {
                        case AutoTopOffState.Standby:
                            {
                                pumpOnRequest = false;
                                bool usedAnalog = false;

                                if ((analogSensor.enable) && (useAnalogSensor)) {
                                    if (analogSensor.connected) {
                                        usedAnalog = true;

                                        if (analogSensor.level < analogOnSetpoint) {
                                            pumpOnRequest = true;
                                        }
                                    }
                                } 

                                if (useFloatSwitch) {
                                    // floatSwitchActivated is set by water level run function
                                    if (usedAnalog) {
                                        pumpOnRequest &= floatSwitchActivated; 
                                    } else {
                                        pumpOnRequest = floatSwitchActivated;
                                    }
                                }

                                if (disableOnLowResevoirLevel && Alarm.CheckAlarming (reservoirLowLevelAlarmIndex)) {
                                    pumpOnRequest = false;
                                    state = AutoTopOffState.Error;
                                    Alarm.Post (atoFailAlarmIndex);
                                }

                                if (pumpOnRequest) {
                                    state = AutoTopOffState.Filling;
                                    Logger.Add ("Starting auto top off");
                                    dataLogger.AddEntry ("ato started"); 
                                    atoTimer.Reset ();
                                    atoTimer.totalSeconds = maxPumpOnTime;
                                    atoTimer.Start ();
                                }

                                break;
                            }
                        case AutoTopOffState.Filling:
                            pumpOnRequest = true;

                            // check analog sensor
                            if ((analogSensor.enable) && (useAnalogSensor)) {
                                if (!Alarm.CheckAlarming (analogSensor.sensorDisconnectedAlarmIndex)) { 
                                    if (analogSensor.level > analogOffSetpoint)
                                        pumpOnRequest = false;
                                }
                            }

                            // check float switch
                            if ((useFloatSwitch) && (!floatSwitchActivated)) {
                                pumpOnRequest = false;
                            }
                            
                            if (!pumpOnRequest) {
                                state = AutoTopOffState.Cooldown;
                                atoTimer.Reset ();
                                Logger.Add ("Stopping auto top off. Runtime: {0} secs", atoTimer.totalSeconds - atoTimer.secondsRemaining);
                                dataLogger.AddEntry ("ato stopped"); 
                                atoTimer.totalSeconds = minPumpOffTime;
                                atoTimer.Start ();
                            }

                            break;
                        case AutoTopOffState.Cooldown:
                        case AutoTopOffState.Error:
                        default:
                            pumpOnRequest = false;
                            break;
                        }
                    } else {
                        state = AutoTopOffState.Standby;
                        pumpOnRequest = false;
                    }
                } else {
                    state = AutoTopOffState.Off;
                    pumpOnRequest = false;
                }
            }

            protected void OnTimerElapsed (object sender, TimerElapsedEventArgs args) {
                if (state == AutoTopOffState.Filling) {
                    pumpOnRequest = false;
                    state = AutoTopOffState.Error;
                    Alarm.Post (atoFailAlarmIndex);
                } else if (state == AutoTopOffState.Cooldown) {
                    state = AutoTopOffState.Standby;
                }
            }
        }
    }
}

