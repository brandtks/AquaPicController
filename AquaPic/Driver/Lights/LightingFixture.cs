using System;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.PowerDriver;
using AquaPic.SerialBus;
using AquaPic.AlarmDriver;
using AquaPic.TemperatureDriver;

namespace AquaPic.LightingDriver
{
    public partial class Lighting 
    {
        private class LightingFixture
    	{
            public string name { get; set; }
            public int sunRiseOffset { get; set; }
            public int sunSetOffset { get; set; }
            public Time minSunRise { get; set; }
            public Time maxSunSet { get; set; }
            public TimeDate sunRise;
            public TimeDate sunSet;
            public Mode mode { get; set; }
            public bool lightingOn { get; set; }
            public bool highTempLockout { get; set; }

            private IndividualControl plug;
            private bool requestedState;

            public LightingFixture (
                byte powerID,
                byte plugID,
                string name,               
                int sunRiseOffset, 
                int sunSetOffset, 
                Time minSunRise, 
                Time maxSunSet,  
                bool highTempLockout
            ) {
                this.plug.Group = powerID;
                this.plug.Individual = plugID;
                Power.AddPlug (this.plug.Group, this.plug.Individual, name, true);
                this.name = name;
                this.sunRiseOffset = sunRiseOffset;
                this.sunSetOffset = sunSetOffset;
                this.minSunRise = minSunRise;
                this.maxSunSet = maxSunSet;
                this.sunRise = TimeDate.Zero;
                this.sunSet = TimeDate.Zero;
                this.mode = Mode.Manual;
                this.lightingOn = false;
                this.highTempLockout = highTempLockout;
                if (this.highTempLockout) {
                    int alarmIdx = Temperature.highTempAlarmIdx;
                    if (alarmIdx == -1)
                        return;
                    Alarm.AddPostHandler (alarmIdx, sender => this.TurnLightsOff ());
                    Alarm.AddClearHandler (alarmIdx, sender => {
                        if (this.requestedState)
                            this.TurnLightsOn ();
                    });
                }
                Power.AddHandlerOnStateChange (this.plug, LightingPlugStateChange);
    		}

            public void SetSunRiseSet (TimeDate rise, TimeDate sSet) {
                if (mode == Mode.Auto) {
                    sunRise.updateDateToToday ();
                    sunSet.updateDateToToday ();
                    return;
                }

                if (rise.compareToTime (minSunRise) < 0) // rise is before min
                    sunRise.setTimeDate (minSunRise);
                else
                    sunRise.setTimeDate (rise);

                if (sSet.compareToTime (maxSunSet) > 0) // sSet is after max
                    sunSet.setTimeDate (maxSunSet);
                else
                    sunSet.setTimeDate (sSet);

                sunRise.addMinToDate (sunRiseOffset);
                sunSet.addMinToDate (sunSetOffset);
            }

            public void SetSunRise (TimeDate rise) {
                if (mode == Mode.Auto) {
                    sunRise.updateDateToToday ();
                    sunSet.updateDateToToday ();
                    return;
                }
                    
                if (rise.compareToTime (minSunRise) < 0) // rise is before min
                    sunRise.setTimeDate (minSunRise);
                else
                    sunRise.setTimeDate (rise);

                sunRise.addMinToDate (sunRiseOffset);
            }

            public void SetSunSet (TimeDate sSet) {
                if (mode == Mode.Auto) {
                    sunRise.updateDateToToday ();
                    sunSet.updateDateToToday ();
                    return;
                }

                if (sSet.compareToTime (maxSunSet) > 0) // sSet is after max
                    sunSet.setTimeDate (maxSunSet);
                else
                    sunSet.setTimeDate (sSet);

                sunSet.addMinToDate (sunSetOffset);

            }

            public virtual void TurnLightsOn () {
                requestedState = true;
                if (highTempLockout && !Alarm.CheckAlarming (Temperature.highTempAlarmIdx))
                    Power.SetPlug (plug, true);
            }

            public virtual void TurnLightsOff () {
                requestedState = false;
                Power.SetPlug (plug, false);
            }

            public void LightingPlugStateChange (object sender, stateChangeEventArgs args) {
                lightingOn = args.state;
            }
    	}
    }
}

