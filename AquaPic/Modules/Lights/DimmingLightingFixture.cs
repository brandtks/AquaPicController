using System;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.AnalogOutputDriver;

namespace AquaPic.LightingModule
{
    public partial class Lighting 
    {
        private class DimmingLightingFixture : LightingFixture
        {
            public float currentDimmingLevel { get; set; }
            public float minDimmingOutput { get; set; }
            public float maxDimmingOutput { get; set; }
            public IndividualControl dimCh;
            public AnalogType type;

            public DimmingLightingFixture (
                byte powerID,
                byte plugID,
                byte cardID,
                byte channelID,
                AnalogType type,
                string name,               
                int timeOnOffsetMinutes,
                int timeOffOffsetMinutes,
                float minDimmingOutput,
                float maxDimmingOutput,
                LightingTime lightingTime,
                bool highTempLockout) 
                : base (powerID, plugID, name, timeOnOffsetMinutes, timeOffOffsetMinutes, lightingTime, highTempLockout)
            {
                this.dimCh.Group = cardID;
                this.dimCh.Individual = channelID;
                this.type = type;
                this.minDimmingOutput = minDimmingOutput;
                this.maxDimmingOutput = maxDimmingOutput;
                AnalogOutput.AddChannel (this.dimCh.Group, this.dimCh.Individual, this.type, name);
            }

            public void SetDimmingLevel (float dimmingLevel) {
                currentDimmingLevel = dimmingLevel;

                if (currentDimmingLevel > maxDimmingOutput)
                    currentDimmingLevel = maxDimmingOutput;

                if (currentDimmingLevel < minDimmingOutput)
                    currentDimmingLevel = minDimmingOutput;

                currentDimmingLevel = currentDimmingLevel.Map (0.0f, 100.0f, 0, 1024); // PIC16F1936 has 10bit PWM
                int level = Convert.ToInt32(currentDimmingLevel);
                    
                AnalogOutput.SetAnalogValue (dimCh, level);
            }
        }
    }
}

