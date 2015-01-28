using System;
using AquaPic.Utilites;
using AquaPic.Power;
using AquaPic.SerialBus;
using AquaPic.Alarm;
using AquaPic.Temp;

namespace AquaPic.Lighting
{
    public class lightingChannel
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

            if (highTempLockout) {
                int alarmIdx = temperature.highTempAlarmIdx;
                if (alarmIdx == -1)
                    return;
                alarm.addPostHandler (alarmIdx, sender => turnLightsOff ());
                alarm.addClearHandler (alarmIdx, sender => turnLightsOn ());
            }

            power.addHandlerOnStateChange (this.plug, new stateChangeHandler (lightingPlugStateChange));
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

        public bool turnLightsOn () {
            return power.setPlug (plug, true);
        }

        public bool turnLightsOff () {
            return power.setPlug (plug, false);
        }

        public void lightingPlugStateChange (object sender, stateChangeEventArgs args) {
            lightingOn = args.state;
        }
	}
}

