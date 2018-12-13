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
using System.Linq;
using Newtonsoft.Json.Linq;
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
                }
                return string.Empty;
            }
        }

        const string settingsFile = "lightingProperties";
        const string settingsArrayName = "lightingFixtures";

        static Lighting () { }

        public static void Init () {
            Logger.Add ("Initializing Lighting");

            fixtures = new Dictionary<string, LightingFixture> ();

            if (SettingsHelper.SettingsFileExists (settingsFile)) {
                var fixturesSettings = SettingsHelper.ReadAllSettingsInArray<LightingFixtureSettings> (settingsFile, settingsArrayName);
                foreach (var settings in fixturesSettings) {
                    AddLight (settings, false);
                }
            } else {
                Logger.Add ("Lighting settings file did not exist, created new lighting settings");

                var jo = new JObject {
                    new JProperty (settingsArrayName, new JArray ())
                };

                SettingsHelper.WriteSettingsFile (settingsFile, jo);
            }
        }

        /**************************************************************************************************************/
        /* Lighting fixtures                                                                                          */
        /**************************************************************************************************************/
        public static void AddLight (LightingFixtureSettings settings, bool saveToFile = true) {
            if (!FixtureNameOk (settings.name)) {
                throw new Exception (string.Format ("Lighting Fixture {0} already exists", settings.name));
            }

            if (settings.dimmingChannel.IsNotEmpty ()) {
                AddDimmingLight (
                    settings.name,
                    settings.powerOutlet,
                    settings.dimmingChannel,
                    settings.lightingStates,
                    settings.highTempLockout);
            } else {
                AddLight (
                    settings.name,
                    settings.powerOutlet,
                    settings.lightingStates,
                    settings.highTempLockout);
            }

            if (saveToFile) {
                AddFixtureSettingsToFile (settings);
            }
        }

        protected static void AddLight (
            string name,
            IndividualControl powerOutlet,
            LightingState[] lightingStates,
            bool highTempLockout) 
        {
            fixtures[name] = new LightingFixture (
                name,
                powerOutlet,
                lightingStates,
                highTempLockout);
        }

        protected static void AddDimmingLight (
            string name,
            IndividualControl powerOutlet,
            IndividualControl dimmingOutlet,
            LightingState[] lightingStates,
            bool highTempLockout) 
        {
            fixtures[name] = new LightingFixtureDimming (
                name,
                powerOutlet,
                dimmingOutlet,
                lightingStates,
                highTempLockout);
        }

        public static void UpdateLight (string fixtureName, LightingFixtureSettings settings) {
            if (CheckFixtureKeyNoThrow (fixtureName)) {
                settings.lightingStates = GetLightingFixtureLightingStates (fixtureName);
                RemoveLight (fixtureName);
            }
            AddLight (settings);
        }

        public static void RemoveLight (string fixtureName) {
            CheckFixtureKey (fixtureName);
            fixtures[fixtureName].Remove ();
            fixtures.Remove (fixtureName);
            DeleteFixtureSettingsFromFile (fixtureName);
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

        public static string[] GetAllDimmingFixtureNames () {
            List<string> names = new List<string> ();
            foreach (var fixture in fixtures.Values) {
                if (fixture is LightingFixtureDimming) {
                    names.Add (fixture.name);
                }
            }
            return names.ToArray ();
        }

        /**************************************************************************************************************/
        /* Individual Control                                                                                         */
        /**************************************************************************************************************/
        public static IndividualControl GetFixtureOutletIndividualControl (string fixtureName) {
            CheckFixtureKey (fixtureName);
            return fixtures[fixtureName].powerOutlet;
        }

        public static IndividualControl GetDimmingChannelIndividualControl (string fixtureName) {
            CheckFixtureKey (fixtureName);

            var fixture = fixtures[fixtureName] as LightingFixtureDimming;
            if (fixture != null) {
                return fixture.channel;
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
            if (!temporaryChange) {
                UpdateFixtureSettingsToFile (fixtureName);
            }
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
                settings.dimmingChannel = GetDimmingChannelIndividualControl (fixtureName);
            }
            return settings;
        }

        protected static void AddFixtureSettingsToFile (LightingFixtureSettings settings) {
            SettingsHelper.AddSettingsToArray (settingsFile, settingsArrayName, settings);
        }

        protected static void UpdateFixtureSettingsToFile (string fixtureName) {
            SettingsHelper.UpdateSettingsInArray (settingsFile, settingsArrayName, fixtureName, GetLightingFixtureSettings (fixtureName));
        }

        protected static void DeleteFixtureSettingsFromFile (string fixtureName) {
            SettingsHelper.DeleteSettingsFromArray (settingsFile, settingsArrayName, fixtureName);
        }
    }
}

