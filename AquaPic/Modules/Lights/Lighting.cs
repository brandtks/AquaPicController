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
//using AquaPic.UserInterface;

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
                using (StreamReader reader = File.OpenText (path)) {
                    var jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));

                    // Very important to update rise/set times before we setup auto on/off for lighting fixtures
                    AbstractTimes.UpdateRiseSetTimes ();

                    var ja = jo["lightingFixtures"] as JArray;
                    foreach (var jt in ja) {
                        JObject obj = jt as JObject;

                        var lightingType = (string)obj["type"];

                        var name = (string)obj["name"];

                        var plug = IndividualControl.Empty;
                        var text = (string)obj["powerStrip"];
                        if (text.IsNotEmpty ()) {
                            try {
                                plug.Group = text;
                            } catch {
                                //
                            }
                        }

                        if (plug.Group.IsNotEmpty ()) {
                            text = (string)obj["outlet"];
                            if (text.IsEmpty ()) {
                                plug = IndividualControl.Empty;
                            } else {
                                try {
                                    plug.Individual = Convert.ToInt32 (text);
                                } catch {
                                    plug = IndividualControl.Empty;
                                }
                            }
                        }

                        bool highTempLockout = true;
                        text = (string)obj["highTempLockout"];
                        if (text.IsNotEmpty ()) {
                            try {
                                highTempLockout = Convert.ToBoolean (text);
                            } catch {
                                //
                            }
                        }

                        var lightingStates = new List<LightingState> ();
                        var jaEvents = obj["events"] as JArray;
                        foreach (var jtEvent in jaEvents) {
                            JObject joEvent = jtEvent as JObject;

                            var startTimeDescriptor = (string)joEvent["startTimeDescriptor"];
                            var endTimeDescriptor = (string)joEvent["endTimeDescriptor"];
   
                            var type = LightingStateType.Off;
                            text = (string)joEvent["type"];
                            if (text.IsNotEmpty ()) {
                                try {
                                    type = (LightingStateType)Enum.Parse (typeof (LightingStateType), text);
                                } catch {
                                    //
                                }
                            }

                            LightingState state;
                            if (joEvent.ContainsKey ("startingDimmingLevel")) {
                                var startingDimmingLevel = 0.0f;
                                text = (string)joEvent["startingDimmingLevel"];
                                if (text.IsNotEmpty ()) {
                                    try {
                                        startingDimmingLevel = Convert.ToSingle (text);
                                    } catch {
                                        //
                                    }
                                }

                                var endingDimmingLevel = 0.0f;
                                text = (string)joEvent["endingDimmingLevel"];
                                if (text.IsNotEmpty ()) {
                                    try {
                                        endingDimmingLevel = Convert.ToSingle (text);
                                    } catch {
                                        //
                                    }
                                }

                                state = new LightingState (
                                    startTimeDescriptor,
                                    endTimeDescriptor,
                                    type,
                                    startingDimmingLevel,
                                    endingDimmingLevel);

                            } else {
                                state = new LightingState (
                                    startTimeDescriptor,
                                    endTimeDescriptor,
                                    type);
                            }
                            lightingStates.Add (state);
                        }

                        if (string.Equals (lightingType, "dimming", StringComparison.InvariantCultureIgnoreCase)) {
                            var channel = IndividualControl.Empty;
                            text = (string)obj["dimmingCard"];
                            if (text.IsNotEmpty ()) {
                                try {
                                    channel.Group = text;
                                } catch {
                                    //
                                }
                            }

                            if (channel.Group.IsNotEmpty ()) {
                                text = (string)obj["channel"];
                                if (text.IsEmpty ()) {
                                    channel = IndividualControl.Empty;
                                } else {
                                    try {
                                        channel.Individual = Convert.ToInt32 (text);
                                    } catch {
                                        channel = IndividualControl.Empty;
                                    }
                                }
                            }

                            float minDimmingOutput = 0.0f;
                            text = (string)obj["minDimmingOutput"];
                            if (text.IsNotEmpty ()) {
                                try {
                                    minDimmingOutput = Convert.ToSingle (text);
                                } catch {
                                    //
                                }
                            }

                            float maxDimmingOutput = 0.0f;
                            text = (string)obj["maxDimmingOutput"];
                            if (text.IsNotEmpty ()) {
                                try {
                                    maxDimmingOutput = Convert.ToSingle (text);
                                } catch {
                                    //
                                }
                            }

                            AddLight (
                                name,
                                plug,
                                channel,
                                lightingStates.ToArray (),
                                minDimmingOutput,
                                maxDimmingOutput,
                                AnalogType.ZeroTen,
                                highTempLockout);
                        } else {
                            AddLight (
                                name,
                                plug,
                                lightingStates.ToArray (),
                                highTempLockout
                            );
                        }
                    }
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
        public static void AddLight (
            string name,
            IndividualControl plug,
            LightingState[] lightingStates,
            bool highTempLockout = true
        ) {
            fixtures[name] = new LightingFixture (
                name,
                plug,
                lightingStates,
                highTempLockout);
        }

        public static void AddLight (
            string name,
            IndividualControl plug,
            IndividualControl channel,
            LightingState[] lightingStates,
            float minDimmingOutput = 0.0f,
            float maxDimmingOutput = 100.0f,
            AnalogType type = AnalogType.ZeroTen,
            bool highTempLockout = true
        ) {
            fixtures[name] = new DimmingLightingFixture (
                name,
                plug,
                channel,
                lightingStates,
                minDimmingOutput,
                maxDimmingOutput,
                type,
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
            }

            fixtures.Remove (fixtureName);
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
            coil.StateGetter = fixtures[fixtureName].OnPlugStateGetter;
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
    }
}

