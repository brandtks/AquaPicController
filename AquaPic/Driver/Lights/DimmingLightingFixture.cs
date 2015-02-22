using System;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.AnalogOutputDriver;

namespace AquaPic.LightingDriver
{
    public partial class Lighting 
    {
        private class DimmingLightingFixture : LightingFixture
        {
            public float currentDimmingLevel { get; set; }
            public float minDimmingOutput { get; set; }
            public float maxDimmingOutput { get; set; }
            public AnalogType type {
                get { return _type; }
            }

            private IndividualControl dimCh;
            private AnalogType _type;

            public DimmingLightingFixture (
                byte powerID,
                byte plugID,
                byte cardID,
                byte channelID,
                AnalogType type,
                string name,               
                int sunRiseOffset, 
                int sunSetOffset, 
                Time minSunRise, 
                Time maxSunSet,
                float minDimmingOutput,
                float maxDimmingOutput,
                bool highTempLockout) 
                : base (powerID, plugID, name, sunRiseOffset, sunSetOffset, minSunRise, maxSunSet, highTempLockout)
            {
                this.dimCh.Group = cardID;
                this.dimCh.Individual = channelID;
                this._type = type;
                AnalogOutput.AddChannel (this.dimCh.Group, this.dimCh.Individual, this._type, name);
            }

            public void SetDimmingLevel (float dimmingLevel) {
                if (dimmingLevel > maxDimmingOutput)
                    dimmingLevel = maxDimmingOutput;
                else if (dimmingLevel < minDimmingOutput)
                    dimmingLevel = minDimmingOutput;

                dimmingLevel.Map (0.0, 100.0, 0.0, 1024.0); // PIC16F1936 has 10bit PWM
                int level = (int)dimmingLevel;
                    
                AnalogOutput.SetAnalogValue (dimCh, level);
            }

            public override void TurnLightsOn () {
                SetDimmingLevel (minDimmingOutput);
                base.TurnLightsOn ();
            }
        }
    }
}

