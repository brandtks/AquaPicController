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
            public float minDimmingOutput;
            public float maxDimmingOutput;
            public IndividualControl channel;
            public Mode dimmingMode;
            public RateOfChangeLimiter rocl;

            public DimmingLightingFixture (
                string name,
                IndividualControl plug, 
                Time onTime,
                Time offTime,
                IndividualControl channel, 
                float minDimmingOutput,
                float maxDimmingOutput,
                AnalogType type,
                LightingTime lightingTime,
                bool highTempLockout)
            : base (
                name,
                plug, 
                onTime,
                offTime,
                lightingTime, 
                highTempLockout
            ) {
                currentDimmingLevel = 0.0f;
                autoDimmingLevel = 0.0f;
                requestedDimmingLevel = 0.0f;
                rocl = new RateOfChangeLimiter (1.0f);
                this.channel = channel;
                this.minDimmingOutput = minDimmingOutput;
                this.maxDimmingOutput = maxDimmingOutput;
                dimmingMode = Mode.Auto;
                AquaPicDrivers.AnalogOutput.AddChannel (channel, name);
                AquaPicDrivers.AnalogOutput.SetChannelType (channel, type);
                var valueControl = AquaPicDrivers.AnalogOutput.GetChannelValueControl (channel);
                valueControl.ValueGetter = CalculateDimmingLevel;

                Power.AddHandlerOnModeChange (
                    plug,
                    OnLightingPlugModeChange);
            }

            public float CalculateDimmingLevel () {
                if (lightingOn == MyState.On) {
                    DateSpan now = DateSpan.Now;

                    autoDimmingLevel = Utils.CalcParabola (
                        onTime, 
                        offTime, 
                        now, 
                        minDimmingOutput, 
                        maxDimmingOutput
                    );

                    if (dimmingMode == Mode.Auto) {
                        requestedDimmingLevel = autoDimmingLevel;
                    }

                    currentDimmingLevel = rocl.RateOfChange(requestedDimmingLevel);

                    return currentDimmingLevel;
                }
                 
                autoDimmingLevel = 0.0f;
                requestedDimmingLevel = 0.0f;
                currentDimmingLevel = 0.0f;
                rocl.Reset ();

                return currentDimmingLevel;
            }

            public void OnLightingPlugModeChange (object sender, ModeChangeEventArgs args) {
                if (args.mode == Mode.Auto)
                    dimmingMode = Mode.Auto;
                else
                    dimmingMode = Mode.Manual;
            }
        }
    }
}

