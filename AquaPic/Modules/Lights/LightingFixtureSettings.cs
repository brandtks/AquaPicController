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
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using AquaPic.Globals;
using AquaPic.Runtime;

namespace AquaPic.Modules
{
    public class LightingFixtureSettings : IEntitySettings
    {
        [EntitySetting (typeof (StringMutator), "name")]
        public string name { get; set; }

        [EntitySetting (typeof (IndividualControlMutator), new string[] { "powerStrip", "outlet" })]
        public IndividualControl powerOutlet { get; set; }

        [EntitySetting (typeof (BoolMutatorDefaultTrue), "highTempLockout")]
        public bool highTempLockout { get; set; }

        [EntitySetting (typeof (LightingStatesMutator))]
        public LightingState[] lightingStates { get; set; }

        [EntitySetting (typeof (IndividualControlMutator), new string[] { "dimmingCard", "channel" }, true)]
        public IndividualControl dimmingOutlet { get; set; }
    }

    public class LightingStatesMutator : ISettingMutator<LightingState[]>
    {
        public LightingState[] Read (JObject jobj, string[] keys) {
            var lightingStates = new List<LightingState> ();
            var jaEvents = jobj["events"] as JArray;
            foreach (var jtEvent in jaEvents) {
                JObject joEvent = jtEvent as JObject;

                var startTimeDescriptor = (string)joEvent["startTimeDescriptor"];
                var endTimeDescriptor = (string)joEvent["endTimeDescriptor"];

                var type = LightingStateType.Off;
                var text = (string)joEvent["type"];
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

            return lightingStates.ToArray ();
        }

        public void Write(LightingState[] value, JObject jobj, string[] keys) {
            var fixtureName = string.Empty;
            var fixtures = Lighting.GetAllFixtureNames ();
            foreach (var fixture in fixtures) {
                var lightingStates = Lighting.GetLightingFixtureLightingStates (fixture);
                if (value.Equals (lightingStates)) {
                    fixtureName = fixture;
                }
            }

            var dimmingFixture = fixtureName.IsNotEmpty () ? Lighting.IsDimmingFixture (fixtureName) : false;

            var ja = new JArray ();
            foreach (var state in value) {
                JObject jo = new JObject ();
                jo.Add (new JProperty ("startTimeDescriptor", state.startTimeDescriptor));
                jo.Add (new JProperty ("endTimeDescriptor", state.endTimeDescriptor));
                jo.Add (new JProperty ("type", state.type.ToString ()));
                if (dimmingFixture) {
                    jo.Add (new JProperty ("startingDimmingLevel", state.startingDimmingLevel.ToString ()));
                    jo.Add (new JProperty ("endingDimmingLevel", state.endingDimmingLevel.ToString ()));
                }
                ja.Add (jo);
            }
            jobj["events"] = ja;
        }

        public bool Valid (LightingState[] states) {
            return true;
        }

        public LightingState[] Default () {
            return new LightingState[0];
        }
    }
}
