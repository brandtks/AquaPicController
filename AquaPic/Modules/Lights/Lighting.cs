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

namespace AquaPic.Modules
{
    public partial class Lighting
    {
        static Dictionary<string, LightingFixture> fixtures;

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
                var fixturesSettings = SettingsHelper.ReadAllSettingsInArray< LightingFixtureSettings> ("lightingProperties", "lightingFixtures");
                foreach (var settings in fixturesSettings) {
                    AddLight (settings, false);
                }
            } else {
                Logger.Add ("Lighting settings file did not exist, created new lighting settings");
                var file = File.Create (path);
                file.Close ();

                var jo = new JObject ();
                jo.Add (new JProperty ("lightingFixtures", new JArray ()));

                File.WriteAllText (path, jo.ToString ());
            }
        }

        /**************************************************************************************************************/
        /* Lighting fixtures                                                                                          */
        /**************************************************************************************************************/
        public static void AddLight (LightingFixtureSettings settings, bool saveToFile = true) {
            if (settings.dimmingOutlet.IsNotEmpty ()) {
                AddLight (
                    settings.name,
                    settings.powerOutlet,
                    settings.dimmingOutlet,
                    settings.lightingStates,
                    settings.highTempLockout,
                    saveToFile);
            } else {
                AddLight (
                    settings.name,
                    settings.powerOutlet,
                    settings.lightingStates,
                    settings.highTempLockout,
                    saveToFile);
            }
        }

        public static void AddLight (
            string name,
            IndividualControl powerOutlet,
            LightingState[] lightingStates,
            bool highTempLockout,
            bool saveToFile = true) 
        {
            fixtures[name] = new LightingFixture (
                name,
                powerOutlet,
                lightingStates,
                highTempLockout);

            if (saveToFile) {
                AddFixtureSettingsToFile (name);
            }
        }

        public static void AddLight (
            string name,
            IndividualControl powerOutlet,
            IndividualControl channel,
            LightingState[] lightingStates,
            bool highTempLockout,
            bool saveToFile = true) 
        {
            fixtures[name] = new LightingFixtureDimming (
                name,
                powerOutlet,
                channel,
                lightingStates,
                highTempLockout);

            if (saveToFile) {
                AddFixtureSettingsToFile (name);
            }
        }

        public static void RemoveLight (string fixtureName) {
            CheckFixtureKey (fixtureName);

            LightingFixture fixture = fixtures[fixtureName];
            Power.RemoveOutlet (fixture.powerOutlet);
            Power.RemoveHandlerOnStateChange (fixture.powerOutlet, fixture.OnLightingPlugStateChange);

            LightingFixtureDimming dimmingFixture = fixture as LightingFixtureDimming;
            if (dimmingFixture != null) {
                Power.RemoveHandlerOnModeChange (dimmingFixture.powerOutlet, dimmingFixture.OnLightingPlugModeChange);
                AquaPicDrivers.AnalogOutput.RemoveChannel (dimmingFixture.channel);
            }

            fixtures.Remove (fixtureName);

            SettingsHelper.DeleteSettingsFromArray ("lightingProperties", "lightingFixtures", fixtureName);

            // Now remove any main screen widgets associated with the fixture
            var ja = SettingsHelper.OpenSettingsFile ("mainScreen") as JArray;
            var arrayIndex = SettingsHelper.FindSettingsInArray (ja, fixtureName);
            if (arrayIndex != -1) {
                ja.RemoveAt (arrayIndex);
                SettingsHelper.SaveSettingsFile ("mainScreen", ja);
            }
        }

        protected static void AddFixtureSettingsToFile (string fixtureName) {
            CheckFixtureKey (fixtureName);
            SettingsHelper.AddSettingsToArray ("lightingProperties", "lightingFixtures", GetLightingFixtureSettings (fixtureName));
        }

        public static void UpdateFixtureSettingsToFile (string fixtureName) {
            UpdateFixtureSettingsToFile (fixtureName, fixtureName);
        }

        public static void UpdateFixtureSettingsToFile (string fixtureName, string savedFixtureName) {
            CheckFixtureKey (fixtureName);
            SettingsHelper.UpdateSettingsInArray ("lightingProperties", "lightingFixtures", savedFixtureName, GetLightingFixtureSettings (fixtureName));
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
            LightingFixtureDimming dimmingFixture = fixture as LightingFixtureDimming;
            if (dimmingFixture != null) {
                AquaPicDrivers.AnalogOutput.SetChannelName (dimmingFixture.channel, fixture.name);
            }

            fixtures.Remove (oldFixtureName);
            fixtures[newFixtureName] = fixture;

            // Rename the main screen widget if it exists
            var ja = SettingsHelper.OpenSettingsFile ("mainScreen") as JArray;
            var arrIdx = SettingsHelper.FindSettingsInArray (ja, oldFixtureName);
            if (arrIdx != -1) {
                ja[arrIdx]["name"] = newFixtureName;
                SettingsHelper.SaveSettingsFile ("mainScreen", ja);
            }
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
            coil.StateGetter = fixtures[fixtureName].OnPlugStateGetter;
        }

        public static IndividualControl GetDimmingChannelIndividualControl (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as LightingFixtureDimming;
            if (fixture != null) {
                return fixture.channel;
            }

            throw new ArgumentException ("fixtureName");
        }

        public static void SetDimmingChannelIndividualControl (string fixtureName, IndividualControl ic) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as LightingFixtureDimming;
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
            return fixtures[fixtureName] is LightingFixtureDimming;
        }

        /**************************************************************************************************************/
        /* Dimming levels                                                                                             */
        /**************************************************************************************************************/
        public static float GetCurrentDimmingLevel (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as LightingFixtureDimming;
            if (fixture != null)
                return fixture.currentDimmingLevel;

            throw new ArgumentException ("fixtureName");
        }

        public static float GetAutoDimmingLevel (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as LightingFixtureDimming;
            if (fixture != null)
                return fixture.autoDimmingLevel;

            throw new ArgumentException ("fixtureName");
        }

        public static float GetRequestedDimmingLevel (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as LightingFixtureDimming;
            if (fixture != null)
                return fixture.requestedDimmingLevel;

            throw new ArgumentException ("fixtureName");
        }

        public static void SetDimmingLevel (string fixtureName, float level) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as LightingFixtureDimming;
            if (fixture != null) {
                if (fixture.dimmingMode == Mode.Manual)
                    fixture.requestedDimmingLevel = level;
                return;
            }

            throw new ArgumentException ("fixtureName");
        }

        /**************************************************************************************************************/
        /* Dimming Modes                                                                                              */
        /**************************************************************************************************************/
        public static Mode GetDimmingMode (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as LightingFixtureDimming;
            if (fixture != null)
                return fixture.dimmingMode;

            throw new ArgumentException ("fixtureName");
        }

        public static void SetDimmingMode (string fixtureName, Mode mode) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as LightingFixtureDimming;
            if (fixture != null) {
                fixture.dimmingMode = mode;
                return;
            }

            throw new ArgumentException ("fixtureName");
        }

        /**************************************************************************************************************/
        /* Lighting States                                                                                            */
        /**************************************************************************************************************/
        public static LightingState[] GetLightingFixtureLightingStates (string fixtureName) {
            CheckFixtureKey (fixtureName);
            var lightingStates = new List<LightingState> ();
            foreach (var state in fixtures[fixtureName].lightingStates) {
                lightingStates.Add (new LightingState (state));
            }
            return lightingStates.ToArray ();
        }

        public static void SetLightingFixtureLightingStates (string fixtureName, LightingState[] lightingStates, bool temporaryChange = true) {
            CheckFixtureKey (fixtureName);
            fixtures[fixtureName].UpdateLightingStates (lightingStates, temporaryChange);
        }

        /**************************************************************************************************************/
        /* Settings                                                                                                   */
        /**************************************************************************************************************/
        public static LightingFixtureSettings GetLightingFixtureSettings (string fixtureName) {
            CheckFixtureKey (fixtureName);
            var settings = new LightingFixtureSettings ();
            settings.name = fixtureName;
            settings.powerOutlet = GetFixtureOutletIndividualControl (fixtureName);
            settings.highTempLockout = GetFixtureTemperatureLockout (fixtureName);
            settings.lightingStates = GetLightingFixtureLightingStates (fixtureName);
            if (IsDimmingFixture (fixtureName)) {
                settings.dimmingOutlet = GetDimmingChannelIndividualControl (fixtureName);
            }
            return settings;
        }
    }
}

