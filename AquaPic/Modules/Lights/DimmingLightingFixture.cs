using System;
using AquaPic.Drivers;
using AquaPic.Utilites;
using AquaPic.Runtime;
using AquaPic.UserInterface;

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
            public IndividualControl dimCh;
            public AnalogType type;
            public Value valueControl;
            public Mode dimmingMode;
            public RateOfChangeLimiter rocl;

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
                currentDimmingLevel = 0.0f;
                autoDimmingLevel = 0.0f;
                requestedDimmingLevel = 0.0f;
                rocl = new RateOfChangeLimiter (1.0f);
                this.dimCh.Group = cardID;
                this.dimCh.Individual = channelID;
                this.type = type;
                this.minDimmingOutput = minDimmingOutput;
                this.maxDimmingOutput = maxDimmingOutput;
                dimmingMode = Mode.Auto;
                valueControl = AnalogOutput.AddChannel (this.dimCh.Group, this.dimCh.Individual, this.type, name);
                valueControl.ValueGetter = SetDimmingLevel;

                // if the Plug is manually turned on or off set dimming to manual
                Power.AddHandlerOnManual (
                    plug,
                    (sender, args) => dimmingMode = Mode.Manual);

                // if the plug is returned to auto set dimming to auto
                Power.AddHandlerOnAuto (
                    plug,
                    (sender, args) => dimmingMode = Mode.Auto);

                MainWindowWidgets.barPlots.Add (name, new BarPlotData (() => {return new DimmingLightBarPlot (name, () => {return currentDimmingLevel;});}));
            }

            public float SetDimmingLevel () {
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
        }
    }
}