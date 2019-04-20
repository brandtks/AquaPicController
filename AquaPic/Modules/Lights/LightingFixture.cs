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
using AquaPic.Globals;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Gadgets.Device;
using AquaPic.PubSub;

namespace AquaPic.Modules
{
    public partial class Lighting
    {
        public class LightingFixture : GenericDevice
        {
            public MyState plugState;
            public bool highTempLockout;
            public LightingState[] lightingStates;
            public LightingState[] savedLightingStates;
            public int currentState;

            public LightingFixture (LightingFixtureSettings settings) : base (settings) {
                highTempLockout = settings.highTempLockout;
                plugState = MyState.Off;
                AquaPicDrivers.Power.AddOutlet (channel, name, MyState.Off, key);
                Subscribe (AquaPicDrivers.Power.GetChannelEventPublisherKey (channel));
                UpdateLightingStates (settings.lightingStates, false);
            }

            protected override ValueType OnRun () {
                if (currentState == -1) {
                    return false;
                }

                if (highTempLockout && Alarm.CheckAlarming (Temperature.Temperature.defaultHighTemperatureAlarmIndex)) {
                    return false;
                }

                CheckCurrentState ();

                if (lightingStates[currentState].type == LightingStateType.Off) { // State is off
                    return false;
                }

                // State is anything but off
                return true;
            }

            public void UpdateLightingStates (LightingState[] lightingStates, bool temporaryChange) {
                savedLightingStates = temporaryChange ? this.lightingStates : (new LightingState[0]);
                this.lightingStates = lightingStates;
                EnsureLastStateIsOff ();
                currentState = -1;
                CheckCurrentState ();
            }

            protected void EnsureLastStateIsOff () {
                if (lightingStates.Length == 0) {
                    return;
                }

                bool atLeastOneOffStateExists = false;
                foreach (var state in lightingStates) {
                    atLeastOneOffStateExists |= state.type == LightingStateType.Off;
                }

                if (!atLeastOneOffStateExists) {
                    return;
                }

                while (lightingStates[lightingStates.Length - 1].type != LightingStateType.Off) {

                    var lastState = lightingStates[lightingStates.Length - 1];
                    for (var i = lightingStates.Length - 1; i > 0; --i) {
                        lightingStates[i] = lightingStates[i - 1];
                    }
                    lightingStates[0] = lastState;
                }
            }

            public void CheckCurrentState () {
                currentState = CheckCurrentState (lightingStates, currentState);

                // There are saved states
                if (savedLightingStates.Length > 0) {
                    // Get the current saved state
                    var savedCurrentState = CheckCurrentState (savedLightingStates, -1);

                    if (savedLightingStates[savedCurrentState].type == LightingStateType.Off) {
                        if (lightingStates[currentState].type == LightingStateType.Off) {
                            // If both the saved and in use states are off, return the saved states to service
                            UpdateLightingStates (savedLightingStates, false);
                        }
                    }
                }
            }

            public int CheckCurrentState (LightingState[] states, int current) {
                if (current != -1) {
                    var state = states[current];
                    var now = Time.TimeNow;

                    if (state.startTime.Before (state.endTime)) { // current state doesn't go over midnight
                        if (now.Before (state.endTime)) { // Still in current lighting state
                            return current;
                        }
                    } else { // current state crosses midnight
                        var midnight = new Time (23, 59, 59);
                        if ((now.After (state.startTime) && now.Before (midnight)) ||
                            (now.After (Time.TimeZero) && now.Before (state.endTime))) { // Still in current lighting state
                            return current;
                        }
                    }

                    return ++current % states.Length;
                }

                if (states.Length > 0) {
                    var now = Time.TimeNow;
                    for (int i = 0; i < states.Length; ++i) {
                        // Check if the start time is before the end time, 
                        if (states[i].startTime.Before (states[i].endTime)) {
                            if (now.After (states[i].startTime) && now.Before (states[i].endTime)) {
                                return i;
                            }
                            // If start is after end then that means that the start is next day
                        } else {
                            var midnight = new Time (23, 59, 59);

                            if ((now.After (states[i].startTime) && now.Before (midnight)) ||
                                (now.After (Time.TimeZero) && now.Before (states[i].endTime))) {
                                return i;
                            }
                        }
                    }
                }

                return -1;
            }

            public override void OnValueChangedAction (object parm) {
                var args = parm as ValueChangedEvent;
                var state = Convert.ToBoolean (args.newValue);
                if (state) {
                    plugState = MyState.On;
                } else {
                    plugState = MyState.Off;
                }
            }

            public override void Dispose () {
                base.Dispose ();
                AquaPicDrivers.Power.RemoveChannel (channel);
                Unsubscribe ();
            }
        }
    }
}

