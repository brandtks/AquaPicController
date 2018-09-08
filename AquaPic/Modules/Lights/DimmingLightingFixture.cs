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
using AquaPic.Drivers;
using AquaPic.Globals;

namespace AquaPic.Modules
{
    public partial class Lighting
    {
        public class DimmingLightingFixture : LightingFixture
        {
            public float currentDimmingLevel;
            public float autoDimmingLevel;
            public float requestedDimmingLevel;
            public IndividualControl channel;
            public Mode dimmingMode;
            public RateOfChangeLimiter rocl;

            public DimmingLightingFixture (
                string name,
                IndividualControl plug,
                IndividualControl channel,
                LightingState[] lightingStates,
                bool highTempLockout)
            : base (
                name,
                plug,
                lightingStates,
                highTempLockout) 
            {
                currentDimmingLevel = 0.0f;
                autoDimmingLevel = 0.0f;
                requestedDimmingLevel = 0.0f;
                rocl = new RateOfChangeLimiter (1.0f);
                this.channel = channel;
                dimmingMode = Mode.Auto;
                AquaPicDrivers.AnalogOutput.AddChannel (channel, name);
                var valueControl = AquaPicDrivers.AnalogOutput.GetChannelValueControl (channel);
                valueControl.ValueGetter = CalculateDimmingLevel;

                Power.AddHandlerOnModeChange (
                    plug,
                    OnLightingPlugModeChange);
                
                Power.AddHandlerOnStateChange (
                    plug,
                    OnDimmingLightPlugStateChange);
            }

            public float CalculateDimmingLevel () {
                DateSpan start, end, now = DateSpan.Now;
                if (lightingStates [currentState].startTime.Before (lightingStates [currentState].endTime)) {
                    start = new DateSpan (lightingStates [currentState].startTime);
                    end = new DateSpan (lightingStates [currentState].endTime);
                } else {
                    var otherDay = DateSpan.Now;
                    // its after midnight
                    if (now.Before (lightingStates[currentState].endTime)) {
                        otherDay.AddDays (-1);
                        start = new DateSpan (otherDay, lightingStates[currentState].startTime);
                        end = new DateSpan (lightingStates[currentState].endTime);
                    } else {
                        otherDay.AddDays (1);
                        start = new DateSpan (lightingStates[currentState].startTime);
                        end = new DateSpan (otherDay, lightingStates[currentState].endTime);
                    }
                }

                if (lightingStates [currentState].type == LightingStateType.LinearRamp) {
                    autoDimmingLevel = Utils.CalcLinearRamp (
                        start,
                        end,
                        now,
                        lightingStates [currentState].startingDimmingLevel,
                        lightingStates [currentState].endingDimmingLevel);
                } else if (lightingStates [currentState].type == LightingStateType.HalfParabolaRamp) {
                    autoDimmingLevel = Utils.CalcHalfParabola (
                        start,
                        end,
                        now,
                        lightingStates [currentState].startingDimmingLevel,
                        lightingStates [currentState].endingDimmingLevel);
                } else if (lightingStates [currentState].type == LightingStateType.ParabolaRamp) {
                    autoDimmingLevel = Utils.CalcParabola (
                        start,
                        end,
                        now,
                        lightingStates [currentState].startingDimmingLevel,
                        lightingStates [currentState].endingDimmingLevel);
                } else if (lightingStates [currentState].type == LightingStateType.On) {
                    autoDimmingLevel = lightingStates [currentState].startingDimmingLevel;
                }

                autoDimmingLevel.Constrain (0, 100);
                if (plugState == MyState.On) {
                    if (dimmingMode == Mode.Auto) {
                        requestedDimmingLevel = autoDimmingLevel;
                    }

                    currentDimmingLevel = rocl.RateOfChange (requestedDimmingLevel);
                }

                return currentDimmingLevel;
            }

            public void OnLightingPlugModeChange (object sender, ModeChangeEventArgs args) {
                if (args.mode == Mode.Auto)
                    dimmingMode = Mode.Auto;
                else
                    dimmingMode = Mode.Manual;
            }

            public void OnDimmingLightPlugStateChange (object sender, StateChangeEventArgs args) {
                if (args.state == MyState.Off) {
                    requestedDimmingLevel = 0.0f;
                    currentDimmingLevel = 0.0f;
                    rocl.Reset ();
                }
            }
        }
    }
}

