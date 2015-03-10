using System;
using System.Collections.Generic;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.PowerDriver;

namespace AquaPic.LightingModule
{
    public partial class Lighting
    {
        //public static Lighting Main = new Lighting ();

        private static List<LightingFixture> fixtures = new List<LightingFixture> ();

        //private Lighting () {
            //fixtures = new List<LightingFixture> ();
        //}

        public static int AddLight (
            int powerID,
            int plugID,
            string name,               
            int sunRiseOffset, 
            int sunSetOffset, 
            Time minSunRise, 
            Time maxSunSet,  
            bool highTempLockout = true
        ) {
            int count = fixtures.Count;
            fixtures.Add (
                new LightingFixture (
                    (byte)powerID, 
                    (byte)plugID, 
                    name, 
                    sunRiseOffset, 
                    sunSetOffset, 
                    minSunRise, 
                    maxSunSet, 
                    highTempLockout
                ));
            TimeDate rise, sSet;
            RiseSetCalc.GetRiseSetTimes (out rise, out sSet);
            fixtures [count].SetSunRiseSet (rise, sSet);
            return count;
        }

        public static int AddLight (
            int powerID,
            int plugID,
            int cardID,
            int channelID,
            AnalogType type,
            string name,               
            int sunRiseOffset, 
            int sunSetOffset, 
            Time minSunRise, 
            Time maxSunSet,
            float minDimmingOutput,
            float maxDimmingOutput,
            bool highTempLockout = true
        ) {
            int count = fixtures.Count;
            fixtures.Add (
                new DimmingLightingFixture (
                    (byte)powerID, 
                    (byte)plugID,
                    (byte)cardID,
                    (byte)channelID,
                    type,
                    name, 
                    sunRiseOffset, 
                    sunSetOffset, 
                    minSunRise, 
                    maxSunSet,
                    minDimmingOutput,
                    maxDimmingOutput,
                    highTempLockout
                ));
            //Commented out for testing 
            //TimeDate rise, sSet;
            //RiseSetCalc.GetRiseSetTimes (out rise, out sSet);
            //fixtures [count].SetSunRiseSet (rise, sSet);
            return count;
        }

        public static void Run () {
            TimeDate now = TimeDate.Now;

            foreach (var fixture in fixtures) {
                if (fixture is DimmingLightingFixture) {
                    DimmingLightingFixture obj = (DimmingLightingFixture)fixture;
                    if ((obj.lightingOn == MyState.On) && ((obj.mode == Mode.Auto) || (obj.mode == Mode.AutoAuto)))
                        obj.SetDimmingLevel (
                            Utils.CalcParabola (
                                obj.sunRise, 
                                obj.sunSet, 
                                now, 
                                obj.minDimmingOutput, 
                                obj.maxDimmingOutput
                            ));
                }
            }
        }

        public static void AtMidnight () {
            TimeDate rise, sSet;
            RiseSetCalc.GetRiseSetTimes (out rise, out sSet);
            for (int i = 0; i < fixtures.Count; ++i)
                fixtures [i].SetSunRiseSet (rise, sSet);
        }
    }
}

