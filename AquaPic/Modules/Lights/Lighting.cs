#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;
using AquaPic.Globals;
using AquaPic.Drivers;
using AquaPic.UserInterface;

namespace AquaPic.Modules
{
    public partial class Lighting
    {
        static Dictionary<string,LightingFixture> fixtures;
        public static DateSpan sunRiseToday;
        public static DateSpan sunSetToday;
        public static DateSpan sunRiseTomorrow;
        public static DateSpan sunSetTomorrow;

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

        static Lighting () { }

        public static void Init () {
            Logger.Add ("Initializing Lighting");

            fixtures = new Dictionary<string, LightingFixture> ();

            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, "lightingProperties.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    var jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));

                    try {
                        RiseSetCalc.latitude = Convert.ToDouble (jo["latitude"]);
                    } catch {
                        RiseSetCalc.latitude = 37.0902;
                        Logger.AddWarning ("Error parsing latitude setting, using default");
                    }

                    try {
                        RiseSetCalc.longitude = Convert.ToDouble (jo["longitude"]);
                    } catch {
                        RiseSetCalc.longitude = -95.7129;
                        Logger.AddWarning ("Error parsing longitude setting, using default");
                    }

                    try {
                        defaultSunRise = new Time (
							Convert.ToInt32 (jo["defaultSunRise"]["hour"]), 
							Convert.ToInt32 (jo["defaultSunRise"]["minute"])
                        );
                    } catch {
                        defaultSunRise = new Time (7, 30);
                        Logger.AddWarning ("Error parsing default sun rise setting, using default");
                    }

                    try {
                        defaultSunSet = new Time (
							Convert.ToInt32 (jo["defaultSunRise"]["hour"]),
							Convert.ToInt32 (jo["defaultSunRise"]["minute"])
                        );
                    } catch {
                        defaultSunSet = new Time (20, 30);
                        Logger.AddWarning ("Error parsing default sun set setting, using default");
                    }

                    try {
                        minSunRise = new Time (
							Convert.ToInt32 (jo["minSunRise"]["hour"]),
							Convert.ToInt32 (jo["minSunRise"]["minute"])
                        );
                    } catch {
                        minSunRise = new Time (7, 15);
                        Logger.AddWarning ("Error parsing min sun rise setting, using default");
                    }

                    try {
                        maxSunRise = new Time (
							Convert.ToInt32 (jo["maxSunRise"]["hour"]),
							Convert.ToInt32 (jo["maxSunRise"]["minute"])
                        );
                    } catch {
                        maxSunRise = new Time (8, 00);
                        Logger.AddWarning ("Error parsing max sun rise setting, using default");
                    }

                    try {
                        minSunSet = new Time (
							Convert.ToInt32 (jo["minSunSet"]["hour"]),
							Convert.ToInt32 (jo["minSunSet"]["minute"])
                        );
                    } catch {
                        minSunSet = new Time (19, 30);
                        Logger.AddWarning ("Error parsing min sun set setting, using default");
                    }


                    try {
                        maxSunSet = new Time (
                            Convert.ToByte (jo["maxSunSet"]["hour"]),
                            Convert.ToByte (jo["maxSunSet"]["minute"])
                        );
                    } catch {
                        maxSunSet = new Time (21, 00);
                        Logger.AddWarning ("Error parsing max sun set setting, using default");
                    }

                    // Very important to update rise/set times before we setup auto on/off for lighting fixtures
                    UpdateRiseSetTimes ();

                    JArray ja = jo["lightingFixtures"] as JArray;
                    foreach (var jt in ja) {
                        JObject obj = jt as JObject;
                        string type = (string)obj["type"];

                        string name = (string)obj["name"];
                        var plug = IndividualControl.Empty;
                        plug.Group = (string)obj["powerStrip"];
                        plug.Individual = Convert.ToInt32 (obj["outlet"]);
                        bool highTempLockout = Convert.ToBoolean (obj["highTempLockout"]);

                        string lTime = (string)obj["lightingTime"];
                        LightingTime lightingTime;
                        if (string.Equals (lTime, "night", StringComparison.InvariantCultureIgnoreCase)) {
                            lightingTime = LightingTime.Nighttime;
                        } else {
                            lightingTime = LightingTime.Daytime;
                        }

                        if (string.Equals (type, "dimming", StringComparison.InvariantCultureIgnoreCase)) {
                            var channel = IndividualControl.Empty;
                            channel.Group = (string)obj["dimmingCard"];
                            channel.Individual = Convert.ToInt32 (obj["channel"]);
                            float minDimmingOutput = Convert.ToSingle (obj["minDimmingOutput"]);
                            float maxDimmingOutput = Convert.ToSingle (obj["maxDimmingOutput"]);

                            AddLight (
                                name,
                                plug,
                                channel,
                                minDimmingOutput,
                                maxDimmingOutput,
                                AnalogType.ZeroTen,
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

                        if (Convert.ToBoolean (obj["autoTimeUpdate"])) {
                            int onTimeOffset = Convert.ToInt32 (obj["onTimeOffset"]);
                            int offTimeOffset = Convert.ToInt32 (obj["offTimeOffset"]);
                            SetupAutoOnOffTime (name, onTimeOffset, offTimeOffset);
                        }
                    }
                }
            } else {
                RiseSetCalc.latitude = 37.0902;
                RiseSetCalc.longitude = -95.7129;
                defaultSunRise = new Time (7, 45);
                defaultSunSet = new Time (21, 00);
                minSunRise = new Time (7, 15);
                maxSunRise = new Time (8, 00);
                minSunSet = new Time (20, 15);
                maxSunSet = new Time (21, 00);

                UpdateRiseSetTimes ();

                Logger.Add ("Temperature settings file did not exist, created new temperature settings");
                var file = File.Create (path);
                file.Close ();

                var jo = new JObject ();
                jo.Add (new JProperty ("latitude", RiseSetCalc.latitude.ToString ()));
                jo.Add (new JProperty ("longitude", RiseSetCalc.longitude.ToString ()));

                var jot = new JObject ();
                jot.Add ("hour", defaultSunRise.hour.ToString ());
                jot.Add ("minute", defaultSunRise.minute.ToString ());
                jo.Add (new JProperty ("defaultSunRise", jot));

                jot["hour"] = defaultSunSet.hour.ToString ();
                jot["minute"] = defaultSunSet.minute.ToString ();
                jo.Add (new JProperty ("defaultSunSet", jot));

                jot["hour"] = minSunRise.hour.ToString ();
                jot["minute"] = minSunRise.minute.ToString ();
                jo.Add (new JProperty ("minSunRise", jot));

                jot["hour"] = maxSunRise.hour.ToString ();
                jot["minute"] = maxSunRise.minute.ToString ();
                jo.Add (new JProperty ("maxSunRise", jot));

                jot["hour"] = minSunSet.hour.ToString ();
                jot["minute"] = minSunSet.minute.ToString ();
                jo.Add (new JProperty ("minSunSet", jot));

                jot["hour"] = maxSunSet.hour.ToString ();
                jot["minute"] = maxSunSet.minute.ToString ();
                jo.Add (new JProperty ("maxSunSet", jot));

                jo.Add (new JProperty ("lightingFixtures", new JArray ()));

                File.WriteAllText (path, jo.ToString ());
            }

            TaskManager.AddTimeOfDayInterrupt ("RiseSetUpdate", new Time (0, 0), () => UpdateRiseSetTimes ());
        }

        public static void UpdateRiseSetTimes () {
            RiseSetCalc.GetRiseSetTimes (out sunRiseToday, out sunSetToday);
            sunRiseTomorrow = RiseSetCalc.GetRiseTimeTomorrow ();
            sunSetTomorrow = RiseSetCalc.GetSetTimeTomorrow ();

            if (sunRiseToday.Before (minSunRise))
                sunRiseToday.UpdateTime (minSunRise);
            else if (sunRiseToday.After (maxSunRise))
                sunRiseToday.UpdateTime (maxSunRise);

            if (sunSetToday.Before (minSunSet))
                sunSetToday.UpdateTime (minSunSet);
            else if (sunSetToday.After (maxSunSet))
                sunSetToday.UpdateTime (maxSunSet);

            if (sunRiseTomorrow.Before (minSunRise))
                sunRiseTomorrow.UpdateTime (minSunRise);
            else if (sunRiseTomorrow.After (maxSunRise))
                sunRiseTomorrow.UpdateTime (maxSunRise);

            if (sunSetTomorrow.Before (minSunSet))
                sunSetTomorrow.UpdateTime (minSunSet);
            else if (sunSetTomorrow.After (maxSunSet))
                sunSetTomorrow.UpdateTime (maxSunSet);
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
            Power.RemoveOutlet (fixture.powerOutlet);
            Power.RemoveHandlerOnStateChange (fixture.powerOutlet, fixture.OnLightingPlugStateChange);

            DimmingLightingFixture dimmingFixture = fixture as DimmingLightingFixture;
            if (dimmingFixture != null) {
                Power.RemoveHandlerOnModeChange (dimmingFixture.powerOutlet, dimmingFixture.OnLightingPlugModeChange);
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

            DateSpan now = DateSpan.Now;
            if (now.After (sunRiseToday) && now.Before (sunSetToday)) {
                // time is after sunrise but before sunset so normal daytime
                if (light.lightingTime == LightingTime.Daytime) { 
                    light.SetOnTime (sunRiseToday);
                    light.SetOffTime (sunSetToday);
                } else {
                    light.SetOnTime (sunSetToday);
                    light.SetOffTime (sunRiseTomorrow);
                }
            } else if (now.Before (sunRiseToday)) { // time is before sunrise today
                if (light.lightingTime == LightingTime.Daytime) { 
                    // lights are supposed to be off, no special funny business required
                    light.SetOnTime (sunRiseToday);
                    light.SetOffTime (sunSetToday);
                } else { // lights are supposed to be on, a little funny bussiness is required
                    DateSpan sunSetYesterday = new DateSpan (sunSetToday);
                    sunSetYesterday.AddDays (-1);
                    light.SetOnTime (sunSetYesterday);
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
            Power.SetOutletName (fixture.powerOutlet, fixture.name);
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
            return fixtures[fixtureName].powerOutlet;
        }

        public static void SetFixtureOutletIndividualControl (string fixtureName, IndividualControl ic) {
            CheckFixtureKey (fixtureName);
            Power.RemoveOutlet (fixtures[fixtureName].powerOutlet);
            fixtures[fixtureName].powerOutlet = ic;
            var coil = Power.AddOutlet (fixtures[fixtureName].powerOutlet, fixtures[fixtureName].name, MyState.On, "Heater");
            coil.ConditionGetter = fixtures[fixtureName].OnPlugControl;
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
                valueControl.ValueGetter = fixture.CalculateDimmingLevel;
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
                fixtures[fixtureName].SetOnTime (new DateSpan (defaultSunRise));
                fixtures[fixtureName].SetOffTime (new DateSpan (defaultSunSet));
            } else {
                fixtures[fixtureName].SetOnTime (new DateSpan (defaultSunSet));
                DateSpan defRiseTom = new DateSpan (defaultSunRise);
                defRiseTom.AddDays (1);
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
        public static DateSpan GetFixtureOnTime (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].onTime;
        }

        public static void SetFixtureOnTime (string fixtureName, DateSpan newOnTime) {
            CheckFixtureKey (fixtureName);
            fixtures[fixtureName].SetOnTime (newOnTime);
        }

        public static DateSpan GetFixtureOffTime (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].offTime;
        }

        public static void SetFixtureOffTime (string fixtureName, DateSpan newOffTime) {
            CheckFixtureKey (fixtureName);
            fixtures[fixtureName].SetOffTime (newOffTime);
        }
    }
}

