#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2018 Goodtime Development

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
using AquaPic.Runtime;

namespace AquaPic.Modules
{
    public enum LightingStateType
    {
        On = 1,
        Off = 2,
        [Description ("Linear Ramp")]
        LinearRamp = 10,
        [Description ("Half Parabola Ramp")]
        HalfParabolaRamp = 11,
        [Description ("Parabola Ramp")]
        ParabolaRamp = 12,
    }

    public class LightingState
    {
        public TimePeriod timePeriod;

        public Time startTime {
            get {
                return timePeriod.startTime;
            }
            set {
                timePeriod.startTime = value;
            }
        }

        public Time endTime {
            get {
                return timePeriod.endTime;
            }
            set {
                timePeriod.endTime = value;
            }
        }

        public string startTimeDescriptor {
            get {
                return timePeriod.startTimeDescriptor;
            }
        }

        public string endTimeDescriptor {
            get {
                return timePeriod.endTimeDescriptor;
            }
        }

        public double lengthInMinutes {
            get {
                return timePeriod.lengthInMinutes;
            }
        }

        public LightingStateType type;
        public float startingDimmingLevel;
        public float endingDimmingLevel;

        public LightingState (string startTimeDescriptor, string endTimeDescriptor, LightingStateType type) {
            timePeriod = new TimePeriod (startTimeDescriptor, endTimeDescriptor);
            this.type = type;
            if (this.type == LightingStateType.Off) {
                startingDimmingLevel = 0;
                endingDimmingLevel = 0;
            } else {
                startingDimmingLevel = 100;
                endingDimmingLevel = 100;
            }
        }

        public LightingState (
            string startTime,
            string endTime,
            LightingStateType type,
            float startingDimmingLevel,
            float endingDimmingLevel)
            : this (startTime, endTime, type)
        {
            this.startingDimmingLevel = startingDimmingLevel;
            this.endingDimmingLevel = endingDimmingLevel;
        }

        public LightingState (LightingState lightingState) {
            timePeriod = new TimePeriod (lightingState.timePeriod);
            type = lightingState.type;
            startingDimmingLevel = lightingState.startingDimmingLevel;
            endingDimmingLevel = lightingState.endingDimmingLevel;
        }

        public LightingState (Time startTime, Time endTime, LightingStateType type) {
            timePeriod = new TimePeriod (startTime, endTime);
            this.type = type;
            if (this.type == LightingStateType.Off) {
                startingDimmingLevel = 0;
                endingDimmingLevel = 0;
            } else {
                startingDimmingLevel = 100;
                endingDimmingLevel = 100;
            }
        }

        public override string ToString () {
            return string.Format ("{4}: {0} at {1} to {2} at {3}",
                                  startTime.ToShortTimeString (),
                                  startingDimmingLevel,
                                  endTime.ToShortTimeString (),
                                  endingDimmingLevel,
                                  Utils.GetDescription (type));
        }
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

        public void Write (LightingState[] value, JObject jobj, string[] keys) {
            var ja = new JArray ();
            foreach (var state in value) {
                JObject jo = new JObject ();
                jo.Add (new JProperty ("startTimeDescriptor", state.startTimeDescriptor));
                jo.Add (new JProperty ("endTimeDescriptor", state.endTimeDescriptor));
                jo.Add (new JProperty ("type", state.type.ToString ()));
                jo.Add (new JProperty ("startingDimmingLevel", state.startingDimmingLevel.ToString ()));
                jo.Add (new JProperty ("endingDimmingLevel", state.endingDimmingLevel.ToString ()));
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