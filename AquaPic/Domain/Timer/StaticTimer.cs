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
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Runtime
{
    public partial class Timer
    {
        protected static Dictionary<string, OnDelayTimer> staticOnDelayTimers = new Dictionary<string, OnDelayTimer> ();

        public static bool OnDelay (string name, string time, bool enable) {
            if (!staticOnDelayTimers.ContainsKey (name)) {
                uint timeDelay = ParseTime (time);
                staticOnDelayTimers.Add (name, new OnDelayTimer (timeDelay));
            }

            return staticOnDelayTimers[name].Evaluate (enable);
        }

        public static uint ParseTime (string timeString) {
            var seperator = new char[1] { ':' };
            string[] t = timeString.Split (seperator, 3);

            uint time = 0;
            if (t.Length == 3) {
                //milliseconds
                time = Convert.ToUInt32 (t[2]);

                //seconds
                time += (Convert.ToUInt32 (t[1]) * 1000);

                //minutes
                time += (Convert.ToUInt32 (t[0]) * 60000);
            }

            return time;
        }

        public static Time Parse (string timeString) {
            int i = 0, hours = 0, minutes = 0, seconds = 0, milliseconds = 0;
            while (i < timeString.Length) { // loop until i points to the end of the string
                var previous = i;
                while (char.IsNumber (timeString[i])) { // increment i until we reach something that isn't a number
                    ++i;
                    if (i >= timeString.Length) { // i has reached the end of the string
                        break;
                    }
                }

                if (i == timeString.Length) { // there isn't a time period after the number
                    throw new Exception ("Can not parse string");
                }

                var inum = i; // store the index of start of the time period
                while (char.IsLetter (timeString[i])) { // now increment until we reach the next number
                    ++i;
                    if (i >= timeString.Length) { // i has reached the end of the string
                        break;
                    }
                }

                var number = Convert.ToInt32 (timeString.Substring (previous, inum - previous));
                var period = timeString.Substring (inum, i - inum);

                switch (period.ToLower ()) {
                case "hr":
                    hours = number;
                    break;
                case "min":
                    minutes = number;
                    break;
                case "sec":
                    seconds = number;
                    break;
                case "ms":
                    milliseconds = number;
                    break;
                default:
                    throw new Exception ("Can not parse string");
                }
            }

            return new Time (hours, minutes, seconds, milliseconds);
        }
    }
}

