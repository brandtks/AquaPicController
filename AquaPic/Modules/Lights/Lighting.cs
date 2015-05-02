using System;
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

        public static Time DefaultSunRise;
        public static Time DefaultSunSet;

        static Lighting () {
            fixtures = new List<LightingFixture> ();
            RiseSetCalc.GetRiseSetTimes (out SunRiseToday, out SunSetToday);
            SunRiseTomorrow = RiseSetCalc.GetRiseTimeTomorrow ();
            SunSetTomorrow = RiseSetCalc.GetSetTimeTomorrow ();

            MinSunRise = new Time (6, 30, 0);
            MaxSunSet = new Time (7, 45, 0);

            MinSunSet = new Time (19, 30, 0);
            MaxSunSet = new Time (21, 00, 0);

            DefaultSunRise = new Time (7, 30, 0);
            DefaultSunSet = new Time (20, 30, 0);
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
            LightingTime lightingTime = LightingTime.Daytime,
            bool highTempLockout = true)
        {
            Time onTime, offTime;

            if (lightingTime == LightingTime.Daytime) {
                onTime = DefaultSunRise;
                offTime = DefaultSunSet;
            } else {
                onTime = DefaultSunSet;
                offTime = DefaultSunRise;
            }

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

        public static int AddLight (
            string name,
            int powerID,
            int plugID,
            int cardID,
            int channelID,
            float minDimmingOutput = 0.0f,
            float maxDimmingOutput = 100.0f,
            AnalogType type = AnalogType.ZeroTen,
            LightingTime lightingTime = LightingTime.Daytime,
            bool highTempLockout = true)
        {
            Time onTime, offTime;

            if (lightingTime == LightingTime.Daytime) {
                onTime = DefaultSunRise;
                offTime = DefaultSunSet;
            } else {
                onTime = DefaultSunSet;
                offTime = DefaultSunRise;
            }

            fixtures.Add (new DimmingLightingFixture (
                name,
                (byte)powerID,
                (byte)plugID,
                onTime,
                offTime,
                (byte)cardID,
                (byte)channelID,
                minDimmingOutput,
                maxDimmingOutput,
                type,
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
            if ((now.CompareTo (SunRiseToday) > 0) && (now.CompareTo (SunSetToday) < 0)) {
                // time is after sunrise but before sunset so normal daytime
                if (light.lightingTime == LightingTime.Daytime) { 
                    light.SetOnTime (SunRiseToday);
                    light.SetOffTime (SunSetToday);
                } else {
                    light.SetOnTime (SunSetToday);
                    light.SetOffTime (SunRiseTomorrow);
                }
            } else if (now.CompareTo (SunRiseToday) < 0) { // time is before sunrise today
                if (light.lightingTime == LightingTime.Daytime) { 
                    // lights are supposed to be off, no special funny business required
                    light.SetOnTime (SunRiseToday);
                    light.SetOffTime (SunSetToday);
                } else { // lights are supposed to be on, a little funny bussiness is required
                    light.onTime = now; // no need to worry about offset
                    light.SetOffTime (SunRiseToday); // night time lighting turns off at sunrise
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
            SunRiseTomorrow = RiseSetCalc.GetRiseTimeTomorrow ();
            SunSetTomorrow = RiseSetCalc.GetSetTimeTomorrow ();

            if (SunRiseToday.CompareToTime (MinSunRise) < 0) // sunrise is before minimum
                SunRiseToday.SetTime (MinSunRise);
            else if (SunRiseToday.CompareToTime (MaxSunRise) > 0) // sunrise is after maximum
                SunRiseToday.SetTime (MaxSunRise);

            if (SunSetToday.CompareToTime (MinSunSet) < 0) // sunset is before minimum
                SunSetToday.SetTime (MinSunSet);
            else if (SunSetToday.CompareToTime (MaxSunSet) > 0) // sunset is after maximum
                SunSetToday.SetTime (MaxSunSet);

            if (SunRiseTomorrow.CompareToTime (MinSunRise) < 0) // sunrise is before minimum
                SunRiseTomorrow.SetTime (MinSunRise);
            else if (SunRiseTomorrow.CompareToTime (MaxSunRise) > 0) // sunrise is after maximum
                SunRiseTomorrow.SetTime (MaxSunRise);

            if (SunSetTomorrow.CompareToTime (MinSunSet) < 0) // sunset is before minimum
                SunSetTomorrow.SetTime (MinSunSet);
            else if (SunSetTomorrow.CompareToTime (MaxSunSet) > 0) // sunset is after maximum
                SunSetTomorrow.SetTime (MaxSunSet);

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