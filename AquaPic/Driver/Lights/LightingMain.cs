using System;
using System.Collections.Generic;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.Power;

namespace AquaPic.Lighting
{
    public static class lighting
    {
        public static List<lightingChannel> lights;

        static lighting () {
            lights = new List<lightingChannel> ();
        }

        public static void init (
            int powerID,
            int plugID,
            string name,               
            int sunRiseOffset, 
            int sunSetOffset, 
            Time minSunRise, 
            Time maxSunSet,  
            bool highTempLockout = true) 
        {
            lights.Add (new lightingChannel ((byte)powerID, (byte)plugID, name, sunRiseOffset, sunSetOffset, minSunRise, maxSunSet));
            TimeDate sunRise, sunSet;
            riseSetCalc.getRiseSetTimes (out sunRise, out sunSet);
            for (int i = 0; i < lights.Count; ++i)
                lights [i].setSunRiseSet (sunRise, sunSet);
        }

        public static void addLight (
            int powerID,
            int plugID,
            string name,               
            int sunRiseOffset, 
            int sunSetOffset, 
            Time minSunRise, 
            Time maxSunSet,  
            bool highTempLockout = true) 
        {
            lights.Add (new lightingChannel ((byte)powerID, (byte)plugID, name, sunRiseOffset, sunSetOffset, minSunRise, maxSunSet));
        }

        public static void run () {
            TimeDate now = TimeDate.Now;

            for (int i = 0; i < lights.Count; ++i) {
                if ((lights [i].mode == Mode.Auto) || (lights [i].mode == Mode.AutoAuto)) {
                    if (!lights [i].lightingOn) { // lighting is off check conditions to turn on
                        if ((lights [i].sunRise.compareTo (now) > 1) && (lights [i].sunSet.compareTo (now) < 1)) {
                            //time is after sun rise and before sun set
                            //lights [i].turnLightsOn ();
                        }
                    } else {
                        if ((lights [i].sunRise.compareTo (now) < 1) && (lights [i].sunSet.compareTo (now) > 1)) {
                            //lights [i].turnLightsOff ();
                        }
                    }
                }
            }
        }

        public static void atMidnight () {
            TimeDate sunRise, sunSet;
            riseSetCalc.getRiseSetTimes (out sunRise, out sunSet);
        }
    }
}

