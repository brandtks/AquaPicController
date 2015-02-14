using System;
using System.Collections.Generic;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.PowerDriver;

namespace AquaPic.LightingDriver
{
    public partial class Lighting
    {
        public static Lighting Main = new Lighting ();

        public List<lightingChannel> lightCh;

        private Lighting () {
            lightCh = new List<lightingChannel> ();
        }

        public void init () {
            TimeDate sunRise, sunSet;
            riseSetCalc.getRiseSetTimes (out sunRise, out sunSet);
            for (int i = 0; i < lightCh.Count; ++i)
                lightCh [i].setSunRiseSet (sunRise, sunSet);
        }

        public void addLight (int powerID,
                              int plugID,
                              string name,               
                              int sunRiseOffset, 
                              int sunSetOffset, 
                              Time minSunRise, 
                              Time maxSunSet,  
                              bool highTempLockout = true) 
        {
            lightCh.Add (new lightingChannel ((byte)powerID, (byte)plugID, name, sunRiseOffset, sunSetOffset, minSunRise, maxSunSet));
        }

        public void run () {
            TimeDate now = TimeDate.Now;

            for (int i = 0; i < lightCh.Count; ++i) {
                if ((lightCh [i].mode == Mode.Auto) || (lightCh [i].mode == Mode.AutoAuto)) {
                    if (!lightCh [i].lightingOn) { // lighting is off check conditions to turn on
                        if ((lightCh [i].sunRise.compareTo (now) > 1) && (lightCh [i].sunSet.compareTo (now) < 1)) {
                            //time is after sun rise and before sun set
                            lightCh [i].turnLightsOn ();
                        }
                    } else {
                        if ((lightCh [i].sunRise.compareTo (now) < 1) && (lightCh [i].sunSet.compareTo (now) > 1)) {
                            lightCh [i].turnLightsOff ();
                        }
                    }
                }
            }
        }

        public void atMidnight () {
            TimeDate sunRise, sunSet;
            riseSetCalc.getRiseSetTimes (out sunRise, out sunSet);
            for (int i = 0; i < lightCh.Count; ++i)
                lightCh [i].setSunRiseSet (sunRise, sunSet);
        }
    }
}

