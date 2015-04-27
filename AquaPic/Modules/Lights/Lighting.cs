
using System.Collections.Generic;
using AquaPic.Globals;
using AquaPic.Utilites;

namespace AquaPic.LightingModule
{
    public partial class Lighting
    {
        //public static Lighting Main = new Lighting ();

        private static List<LightingFixture> fixtures;
        public static TimeDate SunRiseToday;
        public static TimeDate SunSetToday;
        public static TimeDate SunRiseTomorrow;
        public static TimeDate SunSetTomorrow;

        public static Time MinSunRise;
        public static Time MaxSunRise;
        public static Time MinSunSet;
        public static Time MaxSunSet;

        static Lighting () {
            fixtures = new List<LightingFixture> ();
            RiseSetCalc.GetRiseSetTimes (out SunRiseToday, out SunSetToday);
            SunRiseTomorrow = RiseSetCalc.GetRiseTimeTomorrow ();
            SunSetTomorrow = RiseSetCalc.GetSetTimeTomorrow ();
        }

        /*
        public static void Init () {
            RiseSetCalc.GetRiseSetTimes (out SunRiseToday, out SunSetToday);
            RiseSetCalc.GetRiseTimeTomorrow (out SunRiseTomorrow);
            RiseSetCalc.GetSetTimeYesterday (out SunSetYesterday);
        }*/

        public static int AddLight (
            string name,
            int powerID,
            int plugID,
            Time onTime = new Time (7, 30, 0),
            Time offTime = new Time (20, 30, 0),
            LightingTime lightingTime = LightingTime.Daytime,
            bool highTempLockout = true)
        {
            fixtures.Add (new LightingFixture (
                name, 
                (byte)powerID,
                (byte)plugID,
                onTime,
                offTime,
                lightingTime,
                highTempLockout));
            return fixtures.Count - 1;
        }

        public static void SetupAutoOnOffTime (
            int lightID,
            int onTimeOffset = 0,
            int offTimeOffset = 0)
        {
            var light = fixtures [lightID];

            light.onTimeOffset = onTimeOffset;
            light.offTimeOffset = offTimeOffset;
            light.mode = Mode.Auto;

            TimeDate now = TimeDate.Now;
            if (now.compareTo (SunRiseToday) < 0) { // time is before sunrise today
                if (light.lightingTime == LightingTime.Daytime) { 
                    // lights are supposed to be off, no special funny business required
                    light.SetOnTime (SunRiseToday);
                    light.SetOffTime (SunSetToday);
                } else { // lights are supposed to be on, a little funny bussiness is required
                    light.onTime = now; // no need to worry about offset
                    light.SetOffTime (SunRiseToday); // night time lighting turns off at sunrise
                }
            } else if ((now.compareTo (SunSetToday) > 0) && (now.compareTo (SunSetToday) < 0)) {
                // time is after sunrise but befor sunset so normal daytime
                if (light.lightingTime == LightingTime.Daytime) { 
                    light.SetOnTime (SunRiseToday);
                    light.SetOffTime (SunSetToday);
                } else {
                    light.SetOnTime (SunSetToday);
                    light.SetOffTime (SunRiseTomorrow);
                }
            } else { // time is after sunrise
                if (light.lightingTime == LightingTime.Daytime) { 
                    light.SetOnTime (SunRiseTomorrow);
                    light.SetOffTime (SunRiseTomorrow);
                } else {
                    light.SetOnTime (SunSetToday);
                    light.SetOffTime (SunRiseTomorrow);
                }
            }
        }

        public static void AtMidnight () {
            RiseSetCalc.GetRiseSetTimes (out SunRiseToday, out SunSetToday);
            RiseSetCalc.GetRiseTimeTomorrow (out SunRiseTomorrow);

            // update on/off time for daytime lighting
            /*
            foreach (var fixture in fixtures) {
                if (fixture.lightingTime == LightingTime.Daytime)
                    fixture.SetOnOffTime (SunRiseToday, SunSetToday);
            }*/
        }

        public static int GetLightIndex (string name) {
            for (int i = 0; i < fixtures.Count; ++i) {
                if (fixtures [i].name == name)
                    return i;
            }
            return -1;
        }
    }
}

/* OLD STUFF
public static int AddLight (string name,
                            int powerID,
                            int plugID,         
                            int onTimeOffsetMinutes, 
                            int offTimeOffsetMinutes,
                            Time minOnTime = new Time (7, 0, 0),
                            Time maxOnTime = new Time (7, 30, 0),
                            Time minOffTime = new Time (19, 0, 0),
                            Time maxOffTime = new Time (21, 0, 0),
                            LightingTime lightingTime = LightingTime.Daytime,
                            bool highTempLockout = true) 
{
    int count = fixtures.Count;
    fixtures.Add (
        new LightingFixture ((byte)powerID, 
                             (byte)plugID, 
                             name, 
                             onTimeOffsetMinutes, 
                             offTimeOffsetMinutes, 
                             minOnTime,
                             maxOnTime,
                             minOffTime,
                             maxOffTime,
                             lightingTime,
                             highTempLockout));

    if (fixtures [count].lightingTime == LightingTime.Daytime)
        fixtures [count].SetOnOffTime (SunRiseToday, SunSetToday);
    else
        fixtures [count].SetOnOffTime (SunSetToday, SunRiseTomorrow);

    return count;
}

public static int AddLight (string name,  
                            int powerID,
                            int plugID,
                            int onTimeOffsetMinutes, 
                            int offTimeOffsetMinutes,
                            int cardID,
                            int channelID,
                            AnalogType type,
                            float minDimmingOutput,
                            float maxDimmingOutput,
                            Time minOnTime = new Time (7, 0, 0),
                            Time maxOnTime = new Time (7, 30, 0),
                            Time minOffTime = new Time (19, 0, 0),
                            Time maxOffTime = new Time (21, 0, 0),
                            LightingTime lightingTime = LightingTime.Daytime,
                            bool highTempLockout = true) 
{
    int count = fixtures.Count;
    fixtures.Add (
        new DimmingLightingFixture ((byte)powerID, 
                                    (byte)plugID,
                                    (byte)cardID,
                                    (byte)channelID,
                                    type,
                                    name, 
                                    onTimeOffsetMinutes, 
                                    offTimeOffsetMinutes, 
                                    minOnTime,
                                    maxOnTime,
                                    minOffTime,
                                    maxOffTime,
                                    minDimmingOutput,
                                    maxDimmingOutput,
                                    lightingTime,
                                    highTempLockout));

    if (fixtures [count].lightingTime == LightingTime.Daytime)
        fixtures [count].SetOnOffTime (SunRiseToday, SunSetToday);
    else
        fixtures [count].SetOnOffTime (SunSetToday, SunRiseTomorrow);

    return count;
}

public static void Run () {
    TimeDate now = TimeDate.Now;

    foreach (var fixture in fixtures) {
        var obj = fixture as DimmingLightingFixture;
            if (obj != null) {
                if ((obj.lightingOn == MyState.On) && (obj.mode == Mode.Auto))
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

public static void AtSunrise () {
    // update on/off time for nighttime lighting
    foreach (var fixture in fixtures) {
        if (fixture.lightingTime == LightingTime.Nighttime)
            fixture.SetOnOffTime (SunSetToday, SunRiseTomorrow);
        }
}*/