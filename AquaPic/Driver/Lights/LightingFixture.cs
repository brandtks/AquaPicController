using System;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.PowerDriver;
using AquaPic.SerialBus;
using AquaPic.Alarm;
using AquaPic.TemperatureDriver;

namespace AquaPic.LightingDriver
{
    public partial class Lighting 
    {
        private class lightingChannel
    	{
            public string name { get; set; }
            public int sunRiseOffset { get; set; }
            public int sunSetOffset { get; set; }
            public Time minSunRise { get; set; }
            public Time maxSunSet { get; set; }
            public TimeDate sunRise;
            public TimeDate sunSet;
            public Mode mode { get; set; }
            public pwrPlug plug;
            public bool lightingOn { get; set; }
            public bool highTempLockout { get; set; }
            private bool requestedState;

            public lightingChannel (
                byte powerID,
                byte plugID,
                string name,               
                int sunRiseOffset, 
                int sunSetOffset, 
                Time minSunRise, 
                Time maxSunSet,  
                bool highTempLockout = true) 
            {
                this.plug.powerID = powerID;
                this.plug.plugID = plugID;
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
                    int alarmIdx = Temperature.Main.highTempAlarmIdx;
                    if (alarmIdx == -1)
                        return;
                    alarm.addPostHandler (alarmIdx, sender => this.turnLightsOff ());
                    alarm.addClearHandler (alarmIdx, sender => {
                        if (this.requestedState)
                            this.turnLightsOn ();
                    });
                }

                Power.Main.addHandlerOnStateChange (this.plug, lightingPlugStateChange);
    		}

            public void setSunRiseSet (TimeDate rise, TimeDate sSet) {
                if (mode == Mode.AutoAuto) {
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
                } else if (mode == Mode.Auto) {
                    sunRise.updateDateToToday ();
                    sunSet.updateDateToToday ();
                }
            }

            public void setSunRise (TimeDate rise) {
                if (rise.compareToTime (minSunRise) < 0) // rise is before min
                    sunRise.setTimeDate (minSunRise);
                else
                    sunRise.setTimeDate (rise);

                sunRise.addMinToDate (sunRiseOffset);
            }

            public void setSunSet (TimeDate sSet) {
                if (sSet.compareToTime (maxSunSet) > 0) // sSet is after max
                    sunSet.setTimeDate (maxSunSet);
                else
                    sunSet.setTimeDate (sSet);

                sunSet.addMinToDate (sunSetOffset);
            }

            public void turnLightsOn () {
                if ((highTempLockout) && (alarm.checkAlarming (Temperature.Main.highTempAlarmIdx)))
                    requestedState = true;
                else
                    Power.Main.setPlug (plug, true);
            }

            public void turnLightsOff () {
                Power.Main.setPlug (plug, false);
            }

            public void lightingPlugStateChange (object sender, stateChangeEventArgs args) {
                lightingOn = args.state;
            }
    	}
    }
}

