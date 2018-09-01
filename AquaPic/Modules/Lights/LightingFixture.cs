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
using AquaPic.Globals;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Modules;

namespace AquaPic.Modules
{
    public partial class Lighting
    {
        public class LightingFixture
        {
            public string name;
            public MyState plugState;
            public bool highTempLockout;
            public IndividualControl powerOutlet;
            public LightingState[] lightingStates;
            public int currentState;

            public LightingFixture (
                string name,
                IndividualControl powerOutlet,
                LightingState[] lightingStates,
                bool highTempLockout)
            {
                this.name = name;
                this.powerOutlet = powerOutlet;
                this.highTempLockout = highTempLockout;

                this.lightingStates = lightingStates;
                if (this.lightingStates.Length > 0) {
                    var now = DateSpan.Now;
                    for (int i = 0; i < this.lightingStates.Length; ++i) {
                        if (now.After (this.lightingStates[i].startTime) && now.Before (this.lightingStates[i].endTime)) {
                            currentState = i;
                            break;
                        }
                    }
                } else {
                    currentState = -1;
                }

                var plugControl = Power.AddOutlet (this.powerOutlet, this.name, MyState.Off, "Lighting");
                plugControl.StateGetter = OnPlugStateGetter;
                Power.AddHandlerOnStateChange (this.powerOutlet, OnLightingPlugStateChange);
            }

            public bool OnPlugStateGetter () {
                if (highTempLockout && Alarm.CheckAlarming (Temperature.defaultHighTemperatureAlarmIndex))
                    return false;

                if (currentState == -1) {
                    return false;
                }

                DateSpan now = DateSpan.Now;
                if (now.Before (lightingStates[currentState].endTime)) { // Still in current lighting state
                    if (lightingStates[currentState].type == LightingStateType.Off) { // State is off
                        return false;
                    }

                    // State is anything but off
                    return true;
                }

                // Now in next state
                currentState = currentState++ % lightingStates.Length;
                var nextState = currentState++ % lightingStates.Length;
                lightingStates[nextState].ParseTimeDescriptors ();
                if (lightingStates[nextState].startTime.Before (lightingStates[currentState].endTime)) {
                    // The next state starts before the current state ends
                    // This happens if the next state is supposed to be tomorrow
                    lightingStates[nextState].ParseTimeDescriptors (true);
                }

                if (lightingStates[currentState].type == LightingStateType.Off) { // State is off
                    return false;
                }

                // State is anything but off
                return true;
            }

            public void OnLightingPlugStateChange (object sender, StateChangeEventArgs args) {
                plugState = args.state;
            }
        }
    }
}

