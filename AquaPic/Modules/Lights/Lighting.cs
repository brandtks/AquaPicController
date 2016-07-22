using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static Dictionary<string,LightingFixture> fixtures;
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

        public static string defaultFixture {
            get {
                if (fixtures.Count > 0) {
                    var first = fixtures.First ();
                    return first.Key;
                } else {
                    return string.Empty;
                }
            }
        }

        static Lighting () {
            fixtures = new Dictionary<string,LightingFixture> ();

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

                JArray ja = jo["lightingFixtures"] as JArray;
                foreach (var jt in ja) {
                    JObject obj = jt as JObject;
                    string type = (string)obj ["type"];

                    string name = (string)obj ["name"];
                    IndividualControl plug ;
                    plug.Group = Power.GetPowerStripIndex ((string)obj["powerStrip"]);
                    plug.Individual = Convert.ToInt32 (obj["outlet"]);
                    bool highTempLockout = Convert.ToBoolean (obj ["highTempLockout"]);

                    string lTime = (string)obj ["lightingTime"];
                    LightingTime lightingTime;
                    if (string.Equals (lTime, "night", StringComparison.InvariantCultureIgnoreCase)) {
                        lightingTime = LightingTime.Nighttime;
                    } else {
                        lightingTime = LightingTime.Daytime;
                    }

                    if (string.Equals (type, "dimming", StringComparison.InvariantCultureIgnoreCase)) {
                        IndividualControl channel;
                        channel.Group = AquaPicDrivers.AnalogOutput.GetCardIndex ((string)obj["dimmingCard"]);
                        channel.Individual = Convert.ToInt32 (obj ["channel"]);
                        float minDimmingOutput = Convert.ToSingle (obj ["minDimmingOutput"]);
                        float maxDimmingOutput = Convert.ToSingle (obj ["maxDimmingOutput"]);

                        string aType = (string)obj ["analogType"];
                        AnalogType analogType;
                        if (string.Equals (aType, "ZeroTen", StringComparison.InvariantCultureIgnoreCase)) {
                            analogType = AnalogType.ZeroTen;
                        } else {
                            analogType = AnalogType.PWM;
                        }

                        AddLight (
                            name,
                            plug,
                            channel,
                            minDimmingOutput,
                            maxDimmingOutput,
                            analogType,
                            lightingTime,
                            highTempLockout
                        );
                    } else {
                        AddLight (
                            name,
                            plug,
                            lightingTime,
                            highTempLockout
                        );
                    }

                    if (Convert.ToBoolean (obj ["autoTimeUpdate"])) {
                        int onTimeOffset = Convert.ToInt32 (obj ["onTimeOffset"]);
                        int offTimeOffset = Convert.ToInt32 (obj ["offTimeOffset"]);
                        SetupAutoOnOffTime (name, onTimeOffset, offTimeOffset);
                    }
                }
            }

            TaskManager.AddTimeOfDayInterrupt ("RiseSetUpdate", new Time (0, 0), () => UpdateRiseSetTimes ());
        }

        public static void Init () {
            Logger.Add ("Initializing Lighting");
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
        /* Lighting fixtures                                                                                          */
        /**************************************************************************************************************/
        public static void AddLight (
            string name, 
            IndividualControl plug, 
            LightingTime lightingTime = LightingTime.Daytime,
            bool highTempLockout = true
        ) {
            Time onTime, offTime;

            if (lightingTime == LightingTime.Daytime) {
                onTime = defaultSunRise;
                offTime = defaultSunSet;
            } else {
                onTime = defaultSunSet;
                offTime = defaultSunRise;
            }

            fixtures [name] = new LightingFixture (
                name, 
                plug,
                onTime,
                offTime,
                lightingTime,
                highTempLockout);
        }

        public static void AddLight (
            string name, 
            IndividualControl plug, 
            IndividualControl channel, 
            float minDimmingOutput = 0.0f,
            float maxDimmingOutput = 100.0f,
            AnalogType type = AnalogType.ZeroTen,
            LightingTime lightingTime = LightingTime.Daytime,
            bool highTempLockout = true
        ) {
            Time onTime, offTime;

            if (lightingTime == LightingTime.Daytime) {
                onTime = defaultSunRise;
                offTime = defaultSunSet;
            } else {
                onTime = defaultSunSet;
                offTime = defaultSunRise;
            }

            fixtures[name] = new DimmingLightingFixture (
                name,
                plug,
                onTime,
                offTime,
                channel,
                minDimmingOutput,
                maxDimmingOutput,
                type,
                lightingTime,
                highTempLockout);
        }

        public static void RemoveLight (string fixtureName) {
            CheckFixtureKey (fixtureName);

            LightingFixture fixture = fixtures[fixtureName];
            Power.RemoveOutlet (fixture.plug);
            Power.RemoveHandlerOnStateChange (fixture.plug, fixture.OnLightingPlugStateChange);

            DimmingLightingFixture dimmingFixture = fixture as DimmingLightingFixture;
            if (dimmingFixture != null) {
                Power.RemoveHandlerOnModeChange (dimmingFixture.plug, dimmingFixture.OnLightingPlugModeChange);
                AquaPicDrivers.AnalogOutput.RemoveChannel (dimmingFixture.channel);
                HomeWindowWidgets.barPlots.Remove (dimmingFixture.name);
            }

            fixtures.Remove (fixtureName);
        }

        public static void SetupAutoOnOffTime (
            string fixtureName,
            int onTimeOffset = 0,
            int offTimeOffset = 0
        ) {
            CheckFixtureKey (fixtureName);
            var light = fixtures[fixtureName];

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

        public static void CheckFixtureKey (string fixtureName) {
            if (!fixtures.ContainsKey (fixtureName)) {
                throw new ArgumentException ("fixtureName");
            }
        }

        public static bool CheckFixtureKeyNoThrow (string fixtureName) {
            try {
                CheckFixtureKey (fixtureName);
                return true;
            } catch (ArgumentException) {
                return false;
            }
        }

        public static bool FixtureNameOk (string fixtureName) {
            return !CheckFixtureKeyNoThrow (fixtureName);
        }

        /**************************************************************************************************************/
        /* Name                                                                                                       */
        /**************************************************************************************************************/
        public static string[] GetAllFixtureNames () {
            List<string> names = new List<string> ();
            foreach (var fixture in fixtures.Values) {
                names.Add (fixture.name);
            }
            return names.ToArray ();
        }

        public static void SetFixtureName (string oldFixtureName, string newFixtureName) {
            CheckFixtureKey (oldFixtureName);
            if (!FixtureNameOk (newFixtureName)) {
                throw new Exception (string.Format ("Lighting Fixture: {0} already exists", newFixtureName));
            }

            var fixture = fixtures[oldFixtureName];
            
            fixture.name = newFixtureName;
            Power.SetOutletName (fixture.plug, fixture.name);
            DimmingLightingFixture dimmingFixture = fixture as DimmingLightingFixture;
            if (dimmingFixture != null) {
                AquaPicDrivers.AnalogOutput.SetChannelName (dimmingFixture.channel, fixture.name);
            }

            fixtures.Remove (oldFixtureName);
            fixtures[newFixtureName] = fixture;
        }

        /**************************************************************************************************************/
        /* Individual Control                                                                                         */
        /**************************************************************************************************************/
        public static IndividualControl GetFixtureOutletIndividualControl (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].plug;
        }

        public static void SetFixtureOutletIndividualControl (string fixtureName, IndividualControl ic) {
            CheckFixtureKey (fixtureName);
            Power.RemoveOutlet (fixtures[fixtureName].plug);
            fixtures[fixtureName].plug = ic;
            var coil = Power.AddOutlet (fixtures[fixtureName].plug, fixtures[fixtureName].name, MyState.On, "Heater");
            coil.ConditionChecker = fixtures[fixtureName].OnPlugControl;
        }

        public static IndividualControl GetDimmingChannelIndividualControl (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null) {
                return fixture.channel;
            }

            throw new ArgumentException ("fixtureName");
        }

        public static void SetDimmingChannelIndividualControl (string fixtureName, IndividualControl ic) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null) {
                AnalogType type = AquaPicDrivers.AnalogOutput.GetChannelType (fixture.channel);
                AquaPicDrivers.AnalogOutput.RemoveChannel (fixture.channel);
                fixture.channel = ic;
                AquaPicDrivers.AnalogOutput.AddChannel (fixture.channel, fixture.name);
                AquaPicDrivers.AnalogOutput.SetChannelType (fixture.name, type);
                var valueControl = AquaPicDrivers.AnalogOutput.GetChannelValueControl (fixture.channel);
                valueControl.ValueGetter = fixture.OnSetDimmingLevel;
                return;
            }

            throw new ArgumentException ("fixtureName");
        }

        /**************************************************************************************************************/
        /* Lighting Time                                                                                              */
        /**************************************************************************************************************/
        public static LightingTime GetFixtureLightingTime (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].lightingTime;
        }

        public static void SetFixtureLightingTime (string fixtureName, LightingTime lightingTime) {
            CheckFixtureKey (fixtureName);
            
            fixtures[fixtureName].lightingTime = lightingTime;

            if (fixtures[fixtureName].lightingTime == LightingTime.Daytime) {
                fixtures[fixtureName].SetOnTime (new TimeDate (defaultSunRise));
                fixtures[fixtureName].SetOffTime (new TimeDate (defaultSunSet));
            } else {
                fixtures[fixtureName].SetOnTime (new TimeDate (defaultSunSet));
                TimeDate defRiseTom = new TimeDate (defaultSunRise);
                defRiseTom.AddDay (1);
                fixtures[fixtureName].SetOffTime (defRiseTom);
            }
        }

        /**************************************************************************************************************/
        /* High Temperature Lockout                                                                                   */
        /**************************************************************************************************************/
        public static bool GetFixtureTemperatureLockout (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].highTempLockout;
        }

        public static void SetFixtureTemperatureLockout (string fixtureName, bool highTempLockout) {
            CheckFixtureKey (fixtureName);
            fixtures[fixtureName].highTempLockout = highTempLockout;
        }

        /**************************************************************************************************************/
        /* Check Dimming Fixture                                                                                      */
        /**************************************************************************************************************/
        public static bool IsDimmingFixture (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName] is DimmingLightingFixture;
        }

        /**************************************************************************************************************/
        /* Dimming levels                                                                                             */
        /**************************************************************************************************************/
        public static float GetCurrentDimmingLevel (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.currentDimmingLevel;

            throw new ArgumentException ("fixtureName");
        }

        public static float GetAutoDimmingLevel (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.autoDimmingLevel;

            throw new ArgumentException ("fixtureName");
        }

        public static float GetRequestedDimmingLevel (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.requestedDimmingLevel;

            throw new ArgumentException ("fixtureName");
        }

        public static void SetDimmingLevel (string fixtureName, float level) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null) {
                if (fixture.dimmingMode == Mode.Manual)
                    fixture.requestedDimmingLevel = level;
                return;
            }

            throw new ArgumentException ("fixtureName");
        }

        public static float GetMaxDimmingLevel (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.maxDimmingOutput;

            throw new ArgumentException ("fixtureName");
        }

        public static void SetMaxDimmingLevel (string fixtureName, float maxDimmingLevel) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null) {
                fixture.maxDimmingOutput = maxDimmingLevel;
                return;
            }

            throw new ArgumentException ("fixtureName");
        }

        public static float GetMinDimmingLevel (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.minDimmingOutput;

            throw new ArgumentException ("fixtureName");
        }

        public static void SetMinDimmingLevel (string fixtureName, float minDimmingLevel) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null) {
                fixture.minDimmingOutput = minDimmingLevel;
                return;
            }

            throw new ArgumentException ("fixtureName");
        }

        /**************************************************************************************************************/
        /* Dimming Modes                                                                                              */
        /**************************************************************************************************************/
        public static Mode GetDimmingMode (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null)
                return fixture.dimmingMode;

            throw new ArgumentException ("fixtureName");
        }

        public static void SetDimmingMode (string fixtureName, Mode mode) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null) {
                fixture.dimmingMode = mode;
                return;
            }

            throw new ArgumentException ("fixtureName");
        }

        /**************************************************************************************************************/
        /* Auto Update Time / Mode                                                                                    */
        /**************************************************************************************************************/
        public static Mode GetFixtureMode (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].mode;
        }

        /**************************************************************************************************************/
        /* Auto Time Offsets                                                                                          */
        /**************************************************************************************************************/
        public static int GetFixtureOnTimeOffset (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].onTimeOffset;
        }

        public static int GetFixtureOffTimeOffset (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].offTimeOffset;
        }

        /**************************************************************************************************************/
        /* Dimming Types                                                                                              */
        /**************************************************************************************************************/
        public static AnalogType GetDimmingType (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null)
                return AquaPicDrivers.AnalogOutput.GetChannelType (fixture.channel);

            throw new ArgumentException ("fixtureName");
        }

        public static void SetDimmingType (string fixtureName, AnalogType analogType) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as DimmingLightingFixture;
            if (fixture != null) {
                AquaPicDrivers.AnalogOutput.SetChannelType (fixture.channel, analogType);
                return;
            }

            throw new ArgumentException ("fixtureName");
        }

        /**************************************************************************************************************/
        /* On/Off Times                                                                                               */
        /**************************************************************************************************************/
        public static TimeDate GetFixtureOnTime (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].onTime;
        }

        public static void SetFixtureOnTime (string fixtureName, TimeDate newOnTime) {
            CheckFixtureKey (fixtureName);
            fixtures[fixtureName].SetOnTime (newOnTime);
        }

        public static TimeDate GetFixtureOffTime (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].offTime;
        }

        public static void SetFixtureOffTime (string fixtureName, TimeDate newOffTime) {
            CheckFixtureKey (fixtureName);
            fixtures[fixtureName].SetOffTime (newOffTime);
        }
    }
}