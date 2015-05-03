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
        public static TimeDate sunRiseToday;
        public static TimeDate sunSetToday;
        public static TimeDate sunRiseTomorrow;
        public static TimeDate sunSetTomorrow;

        public static Time minSunRise;
        public static Time maxSunRise;

        public static Time minSunSet;
        public static Time mMaxSunSet;

        public static Time defaultSunRise;
        public static Time defaultSunSet;

        static Lighting () {
            fixtures = new List<LightingFixture> ();

            minSunRise = new Time (7, 00, 0);
            maxSunRise = new Time (7, 45, 0);

            minSunSet = new Time (19, 30, 0);
            mMaxSunSet = new Time (21, 00, 0);

            defaultSunRise = new Time (7, 30, 0);
            defaultSunSet = new Time (20, 30, 0);

            RiseSetCalc.GetRiseSetTimes (out sunRiseToday, out sunSetToday);
            sunRiseTomorrow = RiseSetCalc.GetRiseTimeTomorrow ();
            sunSetTomorrow = RiseSetCalc.GetSetTimeTomorrow ();

            if (sunRiseToday.CompareToTime (minSunRise) < 0) // sunrise is before minimum
                sunRiseToday.SetTime (minSunRise);
            if (sunRiseToday.CompareToTime (maxSunRise) > 0) // sunrise is after maximum
                sunRiseToday.SetTime (maxSunRise);

            if (sunSetToday.CompareToTime (minSunSet) < 0) // sunset is before minimum
                sunSetToday.SetTime (minSunSet);
            if (sunSetToday.CompareToTime (mMaxSunSet) > 0) // sunset is after maximum
                sunSetToday.SetTime (mMaxSunSet);

            if (sunRiseTomorrow.CompareToTime (minSunRise) < 0) // sunrise is before minimum
                sunRiseTomorrow.SetTime (minSunRise);
            if (sunRiseTomorrow.CompareToTime (maxSunRise) > 0) // sunrise is after maximum
                sunRiseTomorrow.SetTime (maxSunRise);

            if (sunSetTomorrow.CompareToTime (minSunSet) < 0) // sunset is before minimum
                sunSetTomorrow.SetTime (minSunSet);
            if (sunSetTomorrow.CompareToTime (mMaxSunSet) > 0) // sunset is after maximum
                sunSetTomorrow.SetTime (mMaxSunSet);
        }

        /* Might add reading file for min and max times and default times
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
                onTime = defaultSunRise;
                offTime = defaultSunSet;
            } else {
                onTime = defaultSunSet;
                offTime = defaultSunRise;
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
                onTime = defaultSunRise;
                offTime = defaultSunSet;
            } else {
                onTime = defaultSunSet;
                offTime = defaultSunRise;
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
            if ((now.CompareTo (sunRiseToday) > 0) && (now.CompareTo (sunSetToday) < 0)) {
                // time is after sunrise but before sunset so normal daytime
                if (light.lightingTime == LightingTime.Daytime) { 
                    light.SetOnTime (sunRiseToday);
                    light.SetOffTime (sunSetToday);
                } else {
                    light.SetOnTime (sunSetToday);
                    light.SetOffTime (sunRiseTomorrow);
                }
            } else if (now.CompareTo (sunRiseToday) < 0) { // time is before sunrise today
                if (light.lightingTime == LightingTime.Daytime) { 
                    // lights are supposed to be off, no special funny business required
                    light.SetOnTime (sunRiseToday);
                    light.SetOffTime (sunSetToday);
                } else { // lights are supposed to be on, a little funny bussiness is required
                    light.onTime = now; // no need to worry about offset
                    light.SetOffTime (sunRiseToday); // night time lighting turns off at sunrise
                }
            } else { // time is after sunrise
                if (light.lightingTime == LightingTime.Daytime) { 
                    light.SetOnTime (sunRiseTomorrow);
                    light.SetOffTime (sunRiseTomorrow);
                } else {
                    light.SetOnTime (sunSetToday);
                    light.SetOffTime (sunRiseTomorrow);
                }
            }
        }

        public static void AtMidnight () {
            RiseSetCalc.GetRiseSetTimes (out sunRiseToday, out sunSetToday);
            sunRiseTomorrow = RiseSetCalc.GetRiseTimeTomorrow ();
            sunSetTomorrow = RiseSetCalc.GetSetTimeTomorrow ();

            if (sunRiseToday.CompareToTime (minSunRise) < 0) // sunrise is before minimum
                sunRiseToday.SetTime (minSunRise);
            else if (sunRiseToday.CompareToTime (maxSunRise) > 0) // sunrise is after maximum
                sunRiseToday.SetTime (maxSunRise);

            if (sunSetToday.CompareToTime (minSunSet) < 0) // sunset is before minimum
                sunSetToday.SetTime (minSunSet);
            else if (sunSetToday.CompareToTime (mMaxSunSet) > 0) // sunset is after maximum
                sunSetToday.SetTime (mMaxSunSet);

            if (sunRiseTomorrow.CompareToTime (minSunRise) < 0) // sunrise is before minimum
                sunRiseTomorrow.SetTime (minSunRise);
            else if (sunRiseTomorrow.CompareToTime (maxSunRise) > 0) // sunrise is after maximum
                sunRiseTomorrow.SetTime (maxSunRise);

            if (sunSetTomorrow.CompareToTime (minSunSet) < 0) // sunset is before minimum
                sunSetTomorrow.SetTime (minSunSet);
            else if (sunSetTomorrow.CompareToTime (mMaxSunSet) > 0) // sunset is after maximum
                sunSetTomorrow.SetTime (mMaxSunSet);
        }

        public static int GetLightIndex (string name) {
            for (int i = 0; i < fixtures.Count; ++i) {
                if (fixtures [i].name == name)
                    return i;
            }
            return -1;
        }

        public static string[] GetAllFixtureNames () {
            string[] names = new string[fixtures.Count];
            for (int i = 0; i < fixtures.Count; ++i)
                names [i] = fixtures [i].name;
            return names;
        }

        public static float GetCurrentDimmingLevel (int fixtureID) {
            if (fixtures [fixtureID] is DimmingLightingFixture) {
                var fixture = fixtures [fixtureID] as DimmingLightingFixture;
                return fixture.currentDimmingLevel;
            }
            return 0.0f;
        }

        public static Mode GetDimmingMode (int fixtureID) {
            if (fixtures [fixtureID] is DimmingLightingFixture) {
                var fixture = fixtures [fixtureID] as DimmingLightingFixture;
                return fixture.dimmingMode;
            }
            return Mode.Manual;
        }

        public static bool IsDimmingFixture (int fixtureID) {
            return fixtures [fixtureID] is DimmingLightingFixture;
        }
    }
}