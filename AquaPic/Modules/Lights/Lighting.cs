using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.Drivers;
using AquaPic.UserInterface;

namespace AquaPic.Modules
{
    public partial class Lighting
    {
        private static List<LightingFixture> fixtures;
        public static TimeDate sunRiseToday;
        public static TimeDate sunSetToday;
        public static TimeDate sunRiseTomorrow;
        public static TimeDate sunSetTomorrow;

        public static Time minSunRise;
        public static Time maxSunRise;

        public static Time minSunSet;
        public static Time maxSunSet;

        public static Time defaultSunRise;
        public static Time defaultSunSet;

        public static double latitude {
            get {
                return RiseSetCalc.latitude;
            }
            set {
                RiseSetCalc.latitude = value;
            }
        }

        public static double longitude {
            get {
                return RiseSetCalc.longitude;
            }
            set {
                RiseSetCalc.longitude = value;
            }
        }

        public static int timeZone {
            get {
                return RiseSetCalc.timeZone;
            }
        }

        public static int fixtureCount {
            get {
                return fixtures.Count;
            }
        }

        static Lighting () {
            fixtures = new List<LightingFixture> ();

            string path = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "lightingProperties.json");

            using (StreamReader reader = File.OpenText (path)) {
                JObject jo = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                RiseSetCalc.latitude = Convert.ToDouble (jo ["latitude"]);
                RiseSetCalc.longitude = Convert.ToDouble (jo ["longitude"]);

                defaultSunRise = new Time (
                    Convert.ToByte (jo ["defaultSunRise"] ["hour"]),
                    Convert.ToByte (jo ["defaultSunRise"] ["minute"]));
                defaultSunSet = new Time (
                    Convert.ToByte (jo ["defaultSunSet"] ["hour"]),
                    Convert.ToByte (jo ["defaultSunSet"] ["minute"]));

                minSunRise = new Time (
                    Convert.ToByte (jo ["minSunRise"] ["hour"]),
                    Convert.ToByte (jo ["minSunRise"] ["minute"]));
                maxSunRise = new Time (
                    Convert.ToByte (jo ["maxSunRise"] ["hour"]),
                    Convert.ToByte (jo ["maxSunRise"] ["minute"]));

                minSunSet = new Time (
                    Convert.ToByte (jo ["minSunSet"] ["hour"]),
                    Convert.ToByte (jo ["minSunSet"] ["minute"]));

                maxSunSet = new Time (
                    Convert.ToByte (jo ["maxSunSet"] ["hour"]),
                    Convert.ToByte (jo ["maxSunSet"] ["minute"]));

                // Very important to update rise/set times before we setup auto on/off for lighting fixtures
                UpdateRiseSetTimes ();

                JArray ja = (JArray)jo ["lightingFixtures"];
                foreach (var jt in ja) {
                    JObject obj = jt as JObject;
                    string type = (string)obj ["type"];

                    string name = (string)obj ["name"];
                    int powerStripId = Power.GetPowerStripIndex ((string)obj ["powerStrip"]);
                    int outletId = Convert.ToInt32 (obj ["outlet"]);
                    bool highTempLockout = Convert.ToBoolean (obj ["highTempLockout"]);

                    string lTime = (string)obj ["lightingTime"];
                    LightingTime lightingTime;
                    if (string.Equals (lTime, "night", StringComparison.InvariantCultureIgnoreCase)) {
                        lightingTime = LightingTime.Nighttime;
                    } else {
                        lightingTime = LightingTime.Daytime;
                    }

                    int lightingId;
                    if (string.Equals (type, "dimming", StringComparison.InvariantCultureIgnoreCase)) {
                        int cardId = AquaPicDrivers.AnalogOutput.GetCardIndex ((string)obj ["dimmingCard"]);
                        int channelId = Convert.ToInt32 (obj ["channel"]);
                        float minDimmingOutput = Convert.ToSingle (obj ["minDimmingOutput"]);
                        float maxDimmingOutput = Convert.ToSingle (obj ["maxDimmingOutput"]);

                        string aType = (string)obj ["analogType"];
                        AnalogType analogType;
                        if (string.Equals (aType, "ZeroTen", StringComparison.InvariantCultureIgnoreCase)) {
                            analogType = AnalogType.ZeroTen;
                        } else {
                            analogType = AnalogType.PWM;
                        }

                        lightingId = AddLight (
                            name,
                            powerStripId,
                            outletId,
                            cardId,
                            channelId,
                            minDimmingOutput,
                            maxDimmingOutput,
                            analogType,
                            lightingTime,
                            highTempLockout
                        );
                    } else {
                        lightingId = AddLight (
                            name,
                            powerStripId,
                            outletId,
                            lightingTime,
                            highTempLockout
                        );
                    }

                    if (Convert.ToBoolean (obj ["autoTimeUpdate"])) {
                        int onTimeOffset = Convert.ToInt32 (obj ["onTimeOffset"]);
                        int offTimeOffset = Convert.ToInt32 (obj ["offTimeOffset"]);
                        SetupAutoOnOffTime (lightingId, onTimeOffset, offTimeOffset);
                    }
                }
            }

            TaskManager.AddTimeOfDayInterrupt ("RiseSetUpdate", new Time (0, 0), () => UpdateRiseSetTimes ());
        }

        public static void Init () {
            Logger.Add ("Initializing Lighting");
        }

        public static int AddLight (
            string name, 
            IndividualControl outlet, 
            LightingTime lightingTime = LightingTime.Daytime,
            bool highTempLockout = true)
        {
            return AddLight (
                name, 
                outlet.Group, 
                outlet.Individual, 
                lightingTime,
                highTempLockout);
        }

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
            IndividualControl outlet, 
            IndividualControl channel, 
            float minDimmingOutput = 0.0f,
            float maxDimmingOutput = 100.0f,
            AnalogType type = AnalogType.ZeroTen,
            LightingTime lightingTime = LightingTime.Daytime,
            bool highTempLockout = true)
        {
            return AddLight (
                name, 
                outlet.Group, 
                outlet.Individual, 
                channel.Group, 
                channel.Individual, 
                minDimmingOutput,
                maxDimmingOutput,
                type,
                lightingTime,
                highTempLockout);
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

            return GetFixtureIndex (name);
        }

        public static void RemoveLight (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            LightingFixture l = fixtures [fixtureID];
            DimmingLightingFixture dl = l as DimmingLightingFixture;
            Power.RemoveHandlerOnStateChange (l.plug, l.OnLightingPlugStateChange);
            if (dl != null) {
                Power.RemoveHandlerOnModeChange (dl.plug, dl.OnLightingPlugModeChange);
                AquaPicDrivers.AnalogOutput.RemoveChannel (dl.dimCh);
                HomeWindowWidgets.barPlots.Remove (dl.name);
            }

            Power.RemoveOutlet (l.plug);
            fixtures.Remove (l);
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
                    light.SetOffTime (sunSetTomorrow);
                } else {
                    light.SetOnTime (sunSetToday);
                    light.SetOffTime (sunRiseTomorrow);
                }
            }
        }

        public static void UpdateRiseSetTimes () {
            RiseSetCalc.GetRiseSetTimes (out sunRiseToday, out sunSetToday);
            sunRiseTomorrow = RiseSetCalc.GetRiseTimeTomorrow ();
            sunSetTomorrow = RiseSetCalc.GetSetTimeTomorrow ();

            if (sunRiseToday.CompareToTime (minSunRise) < 0) // sunrise is before minimum
                sunRiseToday.SetTime (minSunRise);
            else if (sunRiseToday.CompareToTime (maxSunRise) > 0) // sunrise is after maximum
                sunRiseToday.SetTime (maxSunRise);

            if (sunSetToday.CompareToTime (minSunSet) < 0) // sunset is before minimum
                sunSetToday.SetTime (minSunSet);
            else if (sunSetToday.CompareToTime (maxSunSet) > 0) // sunset is after maximum
                sunSetToday.SetTime (maxSunSet);

            if (sunRiseTomorrow.CompareToTime (minSunRise) < 0) // sunrise is before minimum
                sunRiseTomorrow.SetTime (minSunRise);
            else if (sunRiseTomorrow.CompareToTime (maxSunRise) > 0) // sunrise is after maximum
                sunRiseTomorrow.SetTime (maxSunRise);

            if (sunSetTomorrow.CompareToTime (minSunSet) < 0) // sunset is before minimum
                sunSetTomorrow.SetTime (minSunSet);
            else if (sunSetTomorrow.CompareToTime (maxSunSet) > 0) // sunset is after maximum
                sunSetTomorrow.SetTime (maxSunSet);
        }

        /**************************************************************************************************************/
        /* Name                                                                                                       */
        /**************************************************************************************************************/
        public static int GetFixtureIndex (string name) {
            for (int i = 0; i < fixtures.Count; ++i) {
                if (string.Equals (name, fixtures [i].name, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException (name + " does not exists");
        }

        public static string[] GetAllFixtureNames () {
            string[] names = new string[fixtures.Count];
            for (int i = 0; i < fixtures.Count; ++i)
                names [i] = fixtures [i].name;
            return names;
        }

        public static string GetFixtureName (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            return fixtures [fixtureID].name;
        }

        public static void SetFixtureName (int fixtureID, string name) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            if (FixtureNameOk (name)) {
                fixtures [fixtureID].name = name;
                Power.SetOutletName (fixtures [fixtureID].plug, name);
            } else
                throw new Exception (string.Format ("Lighting Fixture: {0} already exists", name));
        }

        public static bool FixtureNameOk (string name) {
            try {
                GetFixtureIndex (name);
                return false;
            } catch {
                return true;
            }
        }

        /**************************************************************************************************************/
        /* Individual Control                                                                                         */
        /**************************************************************************************************************/
        public static IndividualControl GetFixtureOutletIndividualControl (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            return fixtures [fixtureID].plug;
        }

        public static void SetFixtureOutletIndividualControl (int fixtureID, IndividualControl ic) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            Power.RemoveHandlerOnStateChange (fixtures [fixtureID].plug, fixtures [fixtureID].OnLightingPlugStateChange);

            DimmingLightingFixture dFix = fixtures [fixtureID] as DimmingLightingFixture;
            if (dFix != null)
                Power.RemoveHandlerOnModeChange (dFix.plug, dFix.OnLightingPlugModeChange);

            Power.RemoveOutlet (fixtures [fixtureID].plug);

            fixtures [fixtureID].plug = ic;
            var coil = Power.AddOutlet (fixtures [fixtureID].plug, fixtures [fixtureID].name, MyState.On, "Heater");
            coil.ConditionChecker = fixtures [fixtureID].OnPlugControl;
            Power.AddHandlerOnStateChange (fixtures [fixtureID].plug, fixtures [fixtureID].OnLightingPlugStateChange);
            if (dFix != null)
                Power.AddHandlerOnModeChange (dFix.plug, dFix.OnLightingPlugModeChange);
        }

        public static IndividualControl GetDimmingChannelIndividualControl (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.dimCh;

            throw new ArgumentException ("fixtureID not Dimming");
        }

        public static void SetDimmingChannelIndividualControl (int fixtureID, IndividualControl ic) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null) {
                AnalogType type = AquaPicDrivers.AnalogOutput.GetChannelType (fixture.dimCh);
                AquaPicDrivers.AnalogOutput.RemoveChannel (fixture.dimCh);
                fixture.dimCh = ic;
                AquaPicDrivers.AnalogOutput.AddChannel (fixture.dimCh, fixture.name);
                AquaPicDrivers.AnalogOutput.SetChannelType (fixture.name, type);
                var valueControl = AquaPicDrivers.AnalogOutput.GetChannelValueControl (fixture.dimCh);
                valueControl.ValueGetter = fixture.OnSetDimmingLevel;
                return;
            }

            throw new ArgumentException ("fixtureID not Dimming");
        }

        /**************************************************************************************************************/
        /* Lighting Time                                                                                              */
        /**************************************************************************************************************/
        public static LightingTime GetFixtureLightingTime (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            return fixtures [fixtureID].lightingTime;
        }

        public static void SetFixtureLightingTime (int fixtureID, LightingTime lightingTime) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            fixtures [fixtureID].lightingTime = lightingTime;

            if (fixtures [fixtureID].lightingTime == LightingTime.Daytime) {
                fixtures [fixtureID].SetOnTime (new TimeDate (defaultSunRise));
                fixtures [fixtureID].SetOffTime (new TimeDate (defaultSunSet));
            } else {
                fixtures [fixtureID].SetOnTime (new TimeDate (defaultSunSet));
                TimeDate defRiseTom = new TimeDate (defaultSunRise);
                defRiseTom.AddDay (1);
                fixtures [fixtureID].SetOffTime (defRiseTom);
            }
        }

        /**************************************************************************************************************/
        /* High Temperature Lockout                                                                                   */
        /**************************************************************************************************************/
        public static bool GetFixtureTemperatureLockout (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            return fixtures [fixtureID].highTempLockout;
        }

        public static void SetFixtureTemperatureLockout (int fixtureID, bool highTempLockout) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            fixtures [fixtureID].highTempLockout = highTempLockout;
        }

        /**************************************************************************************************************/
        /* Check Dimming Fixture                                                                                      */
        /**************************************************************************************************************/
        public static bool IsDimmingFixture (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            return fixtures [fixtureID] is DimmingLightingFixture;
        }

        /**************************************************************************************************************/
        /* Dimming levels                                                                                             */
        /**************************************************************************************************************/
        public static float GetCurrentDimmingLevel (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.currentDimmingLevel;

            throw new ArgumentException ("fixtureID not Dimming");
        }

        public static float GetAutoDimmingLevel (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.autoDimmingLevel;
            
            throw new ArgumentException ("fixtureID not Dimming");
        }

        public static float GetRequestedDimmingLevel (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.requestedDimmingLevel;

            throw new ArgumentException ("fixtureID not Dimming");
        }

        public static void SetDimmingLevel (int fixtureID, float level) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null) {
                if (fixture.dimmingMode == Mode.Manual)
                    fixture.requestedDimmingLevel = level;
                return;
            }

            throw new ArgumentException ("fixtureID not Dimming");
        }

        public static float GetMaxDimmingLevel (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.maxDimmingOutput;

            throw new ArgumentException ("fixtureID not Dimming");
        }

        public static void SetMaxDimmingLevel (int fixtureID, float maxDimmingLevel) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null) {
                fixture.maxDimmingOutput = maxDimmingLevel;
                return;
            }

            throw new ArgumentException ("fixtureID not Dimming");
        }

        public static float GetMinDimmingLevel (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.minDimmingOutput;

            throw new ArgumentException ("fixtureID not Dimming");
        }

        public static void SetMinDimmingLevel (int fixtureID, float minDimmingLevel) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null) {
                fixture.minDimmingOutput = minDimmingLevel;
                return;
            }

            throw new ArgumentException ("fixtureID not Dimming");
        }

        /**************************************************************************************************************/
        /* Dimming Modes                                                                                              */
        /**************************************************************************************************************/
        public static Mode GetDimmingMode (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.dimmingMode;

            throw new ArgumentException ("fixtureID not Dimming");
        }

        public static void SetDimmingMode (int fixtureID, Mode mode) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null) {
                fixture.dimmingMode = mode;
                return;
            }

            throw new ArgumentException ("fixtureID not Dimming");
        }

        /**************************************************************************************************************/
        /* Auto Update Time / Mode                                                                                    */
        /**************************************************************************************************************/
        public static Mode GetFixtureMode(int fixtureID) {
            if ((fixtureID < 0) && (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            return fixtures [fixtureID].mode;
        }

        /**************************************************************************************************************/
        /* Auto Time Offsets                                                                                          */
        /**************************************************************************************************************/
        public static int GetFixtureOnTimeOffset (int fixtureID) {
            if ((fixtureID < 0) && (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            return fixtures [fixtureID].onTimeOffset;
        }

        public static int GetFixtureOffTimeOffset (int fixtureID) {
            if ((fixtureID < 0) && (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            return fixtures [fixtureID].offTimeOffset;
        }

        /**************************************************************************************************************/
        /* Dimming Types                                                                                              */
        /**************************************************************************************************************/
        public static AnalogType GetDimmingType (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null)
                return AquaPicDrivers.AnalogOutput.GetChannelType (fixture.dimCh);

            throw new ArgumentException ("fixtureID not Dimming");
        }

        public static void SetDimmingType (int fixtureID, AnalogType analogType) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            var fixture = fixtures [fixtureID] as DimmingLightingFixture;
            if (fixture != null) {
                AquaPicDrivers.AnalogOutput.SetChannelType (fixture.dimCh, analogType);
                return;
            }

            throw new ArgumentException ("fixtureID not Dimming");
        }

        /**************************************************************************************************************/
        /* On/Off Times                                                                                               */
        /**************************************************************************************************************/
        public static TimeDate GetFixtureOnTime (int fixtureID) {
            if ((fixtureID < 0) && (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            return fixtures [fixtureID].onTime;
        }

        public static void SetFixtureOnTime (int fixtureID, TimeDate newOnTime) {
            if ((fixtureID < 0) && (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            fixtures [fixtureID].SetOnTime (newOnTime);
        }

        public static TimeDate GetFixtureOffTime (int fixtureID) {
            if ((fixtureID < 0) || (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            return fixtures [fixtureID].offTime;
        }

        public static void SetFixtureOffTime (int fixtureID, TimeDate newOffTime) {
            if ((fixtureID < 0) && (fixtureID >= fixtures.Count))
                throw new ArgumentOutOfRangeException ("fixtureID");

            fixtures [fixtureID].SetOffTime (newOffTime);
        }
    }
}