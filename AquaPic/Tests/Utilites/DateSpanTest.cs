#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

using System;
using NUnit.Framework;

namespace AquaPic.Utilites.Test
{
    [TestFixture]
    public class DateSpanTest
    {
        [Test]
        public void BeforeAfterTest () {

        }

        public bool Before (DateSpan dateSpan) {
            return Before (dateSpan.ToDateTime ());
        }

        public bool Before (DateTime dateTime) {
            return CompareTo (dateTime) == -1;
        }

        public bool After (DateSpan dateSpan) {
            return After (dateSpan.ToDateTime ());
        }

        public bool After (DateTime dateTime) {
            return CompareTo (dateTime) == 1;
        }

        public bool BeforeTime (DateSpan dateSpan) {
            return CompareTo (dateSpan) == -1;
        }

        public bool AfterTime (DateSpan dateSpan) {
            return CompareTo (dateSpan) == 1;
        }

        // returns 1 if after value, -1 if before value
        protected int CompareTo (DateSpan dateSpan) {
            return ToDateTime ().CompareTo (dateSpan.ToDateTime ());
        }

        // returns 1 if after value, -1 if before value
        protected int CompareTo (DateTime value) {
            return ToDateTime ().CompareTo (value);
        }

        // returns 1 if after value, -1 if before value
        protected int CompareToTime (DateSpan dateSpan) {
            return TimeSpan.Compare (ToTimeSpan (), dateSpan.ToTimeSpan ());
        }

        public double DifferenceInMinutes (DateSpan dateSpan) {
            var span = ToDateTime ().Subtract (dateSpan.ToDateTime ());
            return span.TotalMinutes;
        }

        public double DifferenceInSeconds (DateSpan dateSpan) {
            var span = ToDateTime ().Subtract (dateSpan.ToDateTime ());
            return span.TotalSeconds;
        }

        public DateTime ToDateTime () {
            var val = new DateTime (_year, _month, _day, _hour, _minute, _second, _millisecond);
            return val;
        }

        public void AddDays (int numberOfDays) {
            var val = ToDateTime ();
            val = val.AddDays (numberOfDays);
            _year = val.Year;
            _month = val.Month;
            _day = val.Day;
        }

        public void UpdateTime (TimeSpan value) {
            _hour = value.Hours;
            _minute = value.Minutes;
            _second = value.Seconds;
            _millisecond = value.Milliseconds;
        }

        public void UpdateTime (Time value) {
            _hour = value.hour;
            _minute = value.minute;
            _second = value.second;
            _millisecond = value.millisecond;
        }

		public override string ToString () {
			return ToDateTime ().ToString ("M/dd/yy h:mm:ss.fff tt");
		}

        public string ToShortDateString () {
            var val = ToDateTime ();
            return val.ToString ("M/dd h:mm tt");
        }

        public override bool Equals (object obj) {
            if (this == obj) {
                return true;
            }

            if (!(obj is DateSpan)) {
                return false;
            }

            var dateSpan = (DateSpan)obj;
            var equality = year == dateSpan.year;
            equality &= month == dateSpan.month;
            equality &= day == dateSpan.day;
            equality &= hour == dateSpan.hour;
            equality &= minute == dateSpan.minute;
            equality &= second == dateSpan.second;
            equality &= millisecond == dateSpan.millisecond;
            return equality;
        }

        public override int GetHashCode () {
            return year ^ month ^ day ^ hour ^ minute ^ second ^ millisecond;
        }
    }
}

