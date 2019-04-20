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
using AquaPic.PubSub;
using AquaPic.Gadgets;

namespace AquaPic.Modules
{
    public partial class Lighting
    {
        public class LightingFixtureDimming : LightingFixture
        {
            public float currentDimmingLevel;
            public float autoDimmingLevel;
            public float requestedDimmingLevel;
            public IndividualControl dimmingChannel;
            public Mode dimmingMode;
            public RateOfChangeLimiter rateOfChangeLimiter;
            protected DimmingEquipment dimmingEquipment;

            public LightingFixtureDimming (LightingFixtureSettings settings) : base (settings) {
                currentDimmingLevel = 0.0f;
                autoDimmingLevel = 0.0f;
                requestedDimmingLevel = 0.0f;
                rateOfChangeLimiter = new RateOfChangeLimiter (1.0f);

                dimmingChannel = settings.dimmingChannel;
                dimmingMode = Mode.Auto;
                dimmingEquipment = new DimmingEquipment (settings, this);
                AquaPicDrivers.AnalogOutput.AddOutputChannel (dimmingChannel, name, dimmingEquipment.key);
            }

            public float GetDimmingLevel () {
                if (currentState == -1) {
                    autoDimmingLevel = 0;
                } else {
                    DateSpan start, end, now = DateSpan.Now;
                    if (lightingStates[currentState].startTime.Before (lightingStates[currentState].endTime)) {
                        start = new DateSpan (lightingStates[currentState].startTime);
                        end = new DateSpan (lightingStates[currentState].endTime);
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

                    if (lightingStates[currentState].type == LightingStateType.LinearRamp) {
                        autoDimmingLevel = Utils.CalcLinearRamp (
                            start,
                            end,
                            now,
                            lightingStates[currentState].startingDimmingLevel,
                            lightingStates[currentState].endingDimmingLevel);
                    } else if (lightingStates[currentState].type == LightingStateType.HalfParabolaRamp) {
                        autoDimmingLevel = Utils.CalcHalfParabola (
                            start,
                            end,
                            now,
                            lightingStates[currentState].startingDimmingLevel,
                            lightingStates[currentState].endingDimmingLevel);
                    } else if (lightingStates[currentState].type == LightingStateType.On) {
                        autoDimmingLevel = lightingStates[currentState].startingDimmingLevel;
                    }
                }

                autoDimmingLevel = autoDimmingLevel.Constrain (0, 100);
                if (plugState == MyState.On) {
                    if (dimmingMode == Mode.Auto) {
                        requestedDimmingLevel = autoDimmingLevel;
                    }

                    currentDimmingLevel = rateOfChangeLimiter.RateOfChange (requestedDimmingLevel);
                }

                return currentDimmingLevel;
            }

            public override void OnModeChangedAction (object parm) {
                var args = parm as ModeChangedEvent;
                if (args.mode == Mode.Auto) {
                    dimmingMode = Mode.Auto;
                } else {
                    dimmingMode = Mode.Manual;
                }
            }

            public override void OnValueChangedAction (object parm) {
                var args = parm as ValueChangedEvent;
                var state = Convert.ToBoolean (args.newValue);
                if (state) {
                    plugState = MyState.On;
                } else {
                    plugState = MyState.Off;
                    requestedDimmingLevel = 0.0f;
                    currentDimmingLevel = 0.0f;
                    rateOfChangeLimiter.Reset ();
                }
            }

            public override void Dispose () {
                base.Dispose ();
                AquaPicDrivers.AnalogOutput.RemoveChannel (dimmingChannel);
            }

            protected class DimmingEquipment : GenericEquipment
            {
                protected LightingFixtureDimming fixture;

                public DimmingEquipment (LightingFixtureSettings settings, LightingFixtureDimming fixture) : base (settings) {
                    channel = settings.dimmingChannel;
                    this.fixture = fixture;
                }

                protected override ValueType OnRun () {
                    return fixture.GetDimmingLevel ();
                }
            }
        }
    }
}

