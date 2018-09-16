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
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Modules
{
    public enum LightingStateType
    {
        On,
        Off,
        [Description ("Linear Ramp")]
        LinearRamp,
        [Description ("Half Parabola Ramp")]
        HalfParabolaRamp,
        [Description ("Parabola Ramp")]
        ParabolaRamp,
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
            startingDimmingLevel = 100f;
            endingDimmingLevel = 100f;
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

        public override string ToString () {
            return string.Format ("{0} at {1} to {2} at {3}",
                                  startTime.ToShortTimeString (),
                                  startingDimmingLevel,
                                  endTime.ToShortTimeString (),
                                  endingDimmingLevel);
        }
    }
}