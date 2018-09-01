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

namespace AquaPic.Runtime
{
    public class AbstractTimes
    {
        protected static DateSpan _sunRiseToday;
        public static DateSpan sunRiseToday {
            get {
                return _sunRiseToday;
            }
        }

        protected static DateSpan _sunSetToday;
        public static DateSpan sunSetToday {
            get {
                return _sunSetToday;
            }
        }

        protected static DateSpan _sunRiseTomorrow;
        public static DateSpan sunRiseTomorrow {
            get {
                return _sunRiseTomorrow;
            }
        }

        protected static DateSpan _sunSetTomorrow;
        public static DateSpan sunSetTomorrow {
            get {
                return _sunSetTomorrow;
            }
        }

        public static double latitude = 41.093842;
        public static double longitude = -85.139236;

        public static DateSpan GetDateSpan (string timeDescription) {
            DateSpan time = DateSpan.Zero;
            switch (timeDescription.ToLower ()) {
            case "sunrise":
                time = new DateSpan (sunRiseToday);
                break;
            case "sunset":
                time = new DateSpan (sunSetToday);
                break;
            case "sunrisetomorrow":
                time = new DateSpan (sunRiseTomorrow);
                break;
            case "sunsettomorrow":
                time = new DateSpan (sunSetTomorrow);
                break;
            default:
                throw new IndexOutOfRangeException ();
            }
            return time;
        }

        public static void UpdateRiseSetTimes () {
            var rsCalc = new SunRiseSetCalc (latitude, longitude);
            var today = DateTime.Today;
            var tomorrow = today.AddDays (1);

            _sunRiseToday = rsCalc.GetRiseTime (today);
            _sunSetToday = rsCalc.GetSetTime (today);

            _sunRiseTomorrow = rsCalc.GetRiseTime (tomorrow);
            _sunSetTomorrow = rsCalc.GetSetTime (tomorrow);
        }
    }
}
