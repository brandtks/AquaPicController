using System;
using AquaPic.Drivers;
using AquaPic.Utilites;
using AquaPic.Runtime;

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
                valueControl.ValueGetter = OnSetDimmingLevel;

                Power.AddHandlerOnModeChange (
                    plug,
                    OnLightingPlugModeChange);
            }

            public float OnSetDimmingLevel () {
                if (lightingOn == MyState.On) {
                    TimeDate now = TimeDate.Now;

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

                    return currentDimmingLevel.Map (0.0f, 100.0f, 0, 1024); // PIC16F1936 has 10bit PWM
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