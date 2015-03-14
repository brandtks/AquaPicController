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

        private static List<LightingFixture> fixtures;
        public static TimeDate SunRiseToday;
        public static TimeDate SunSetToday;
        public static TimeDate SunRiseTomorrow;
        public static TimeDate SunSetYesterday;

        static Lighting () {
            fixtures = new List<LightingFixture> ();
            RiseSetCalc.GetRiseSetTimesOut (out SunRiseToday, out SunSetToday);
            RiseSetCalc.GetRiseTimeTomorrowOut (out SunRiseTomorrow);
            RiseSetCalc.GetSetTimeYesterday (out SunSetYesterday);
        }

//        public static void Init () {
//            RiseSetCalc.GetRiseSetTimes (out SunRiseToday, out SunSetToday);
//            RiseSetCalc.GetRiseTimeTomorrow (out SunRiseTomorrow);
//            RiseSetCalc.GetSetTimeYesterday (out SunSetYesterday);
//        }

        public static int AddLight (
            string name,
            int powerID,
            int plugID,         
            int onTimeOffsetMinutes, 
            int ofTimeOffsetMinutes, 
            LightingTime lightingTime = LightingTime.Daytime,
            bool highTempLockout = true
        ) {
            int count = fixtures.Count;
            fixtures.Add (
                new LightingFixture (
                    (byte)powerID, 
                    (byte)plugID, 
                    name, 
                    onTimeOffsetMinutes, 
                    ofTimeOffsetMinutes, 
                    lightingTime,
                    highTempLockout
                ));

            if (fixtures [count].lightingTime == LightingTime.Daytime)
                fixtures [count].SetOnOffTime (SunRiseToday, SunSetToday);
            else
                fixtures [count].SetOnOffTime (SunSetToday, SunRiseTomorrow);

            return count;
        }

        public static int AddLight (
            string name,  
            int powerID,
            int plugID,
            int onTimeOffsetMinutes, 
            int offTimeOffsetMinutes,
            int cardID,
            int channelID,
            AnalogType type,
            float minDimmingOutput,
            float maxDimmingOutput,
            LightingTime lightingTime = LightingTime.Daytime,
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
                    onTimeOffsetMinutes, 
                    offTimeOffsetMinutes, 
                    minDimmingOutput,
                    maxDimmingOutput,
                    lightingTime,
                    highTempLockout
                ));

            if (fixtures [count].lightingTime == LightingTime.Daytime)
                fixtures [count].SetOnOffTime (SunRiseToday, SunSetToday);
            else
                fixtures [count].SetOnOffTime (SunSetToday, SunRiseTomorrow);

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
                                obj.timeOn, 
                                obj.timeOff, 
                                now, 
                                obj.minDimmingOutput, 
                                obj.maxDimmingOutput
                            ));
                }
            }
        }

        public static void AtMidnight () {
            SunSetYesterday.setTimeDate (SunSetToday); // sun set yesterday is sun set today
            RiseSetCalc.GetRiseSetTimesRef (ref SunRiseToday, ref SunSetToday);
            RiseSetCalc.GetRiseTimeTomorrowRef (ref SunRiseTomorrow);

            foreach (var fixture in fixtures) {
                if (fixture.lightingTime == LightingTime.Daytime)
                    fixture.SetOnOffTime (SunRiseToday, SunSetToday);
            }
        }

        public static void AtNoon () {
            foreach (var fixture in fixtures) {
                if (fixture.lightingTime == LightingTime.Nighttime)
                    fixture.SetOnOffTime (SunSetToday, SunRiseTomorrow);
            }
        }
    }
}

