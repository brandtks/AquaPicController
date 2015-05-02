using System;
using AquaPic.AnalogOutputDriver;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.ValueRuntime;

namespace AquaPic.LightingModule
{
    public partial class Lighting
    {
        public class DimmingLightingFixture : LightingFixture
        {
            public float currentDimmingLevel;
            public float minDimmingOutput;
            public float maxDimmingOutput;
            public IndividualControl dimCh;
            public AnalogType type;
            public Value valueControl;
            public Mode dimmingMode;

            public DimmingLightingFixture (
                string name,
                byte powerID,
                byte plugID,
                Time onTime,
                Time offTime,
                byte cardID,
                byte channelID,
                float minDimmingOutput,
                float maxDimmingOutput,
                AnalogType type,
                LightingTime lightingTime,
                bool highTempLockout)
            : base (name,
                    powerID, 
                    plugID, 
                    onTime,
                    offTime,
                    lightingTime, 
                    highTempLockout) 
            {
                this.dimCh.Group = cardID;
                this.dimCh.Individual = channelID;
                this.type = type;
                this.minDimmingOutput = minDimmingOutput;
                this.maxDimmingOutput = maxDimmingOutput;
                dimmingMode = Mode.Auto;
                valueControl = AnalogOutput.AddChannel (this.dimCh.Group, this.dimCh.Individual, this.type, name);
                valueControl.ValueGetter = SetDimmingLevel;
            }

            public float SetDimmingLevel () {
                if (lightingOn == MyState.On) {
                    if (dimmingMode == Mode.Auto) {
                        TimeDate now = TimeDate.Now;

                        currentDimmingLevel = Utils.CalcParabola (
                            onTime, 
                            offTime, 
                            now, 
                            minDimmingOutput, 
                            maxDimmingOutput
                        );
                    }

                    return currentDimmingLevel.Map (0.0f, 100.0f, 0, 1024); // PIC16F1936 has 10bit PWM
                }

                return 0.0f;
            }
        }
    }
}