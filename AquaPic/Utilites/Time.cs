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

namespace AquaPic.Utilites
{
    public class Time
    {
        protected int _hour;
        public int hour {
            get {
                return _hour;
            }
        }

        protected int _minute;
        public int minute {
            get {
                return _minute;
            }
        }

        protected int _second;
        public int second {
            get {
                return _second;
            }
        }

        protected int _millisecond;
        public int millisecond {
            get {
                return _millisecond;
            }
        }

		public static Time TimeZero {
            get {
				return new Time (TimeSpan.Zero);
            }
        }

        public Time (int hour, int minute, int second, int millisecond) {
            this._hour = hour;
            this._minute = minute;
            this._second = second;
            this._millisecond = millisecond;
        }

        public Time (int hour, int minute, int second) 
            : this (hour, minute, second, 0) {
        }

        public Time (int hours, int minute)
            : this (hours, minute, 0, 0) {
        }

        public Time (TimeSpan timeSpan) 
            : this (timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds) {
        }

        public Time (Time timeSpan)
            : this (timeSpan._hour, timeSpan._minute, timeSpan._second, timeSpan._millisecond) {
        }

        public Time () {
            DateTime value = DateTime.Now;
		    _hour = value.Hour;
			_minute = value.Minute;
			_second = value.Second;
			_millisecond = value.Millisecond;
        }

        public TimeSpan ToTimeSpan () {
            TimeSpan val = new TimeSpan (0, _hour, _minute, _second, _millisecond);
            return val;
        }

        public int CompareToTime (Time value) {
            return TimeSpan.Compare (ToTimeSpan (), value.ToTimeSpan ());
        }

        public bool EqualsShortTime (DateTime value) {
            if (value.Hour != _hour)
                return false;

            if (value.Minute != _minute)
                return false;

            return true;
        }

        public void AddMinutes (int numberOfMinutes) {
            TimeSpan timeSpan = new TimeSpan (_hour, _minute + numberOfMinutes, _second);
            _hour = timeSpan.Hours;
            _minute = timeSpan.Minutes;
            _second = timeSpan.Seconds;
            _millisecond = timeSpan.Milliseconds;
        }

        public void AddTimeToTime (Time value) {
            TimeSpan timeSpan = ToTimeSpan ();
            timeSpan.Add (value.ToTimeSpan ());
            _hour = timeSpan.Hours;
            _minute = timeSpan.Minutes;
            _second = timeSpan.Seconds;
            _millisecond = timeSpan.Milliseconds;
        }
          
        public static Time Parse (string value) {
            int pos = value.IndexOf (":");

            if ((pos != 1) && (pos != 2))
                throw new Exception ();

            string hourString = value.Substring (0, pos);
            int hour = Convert.ToInt32 (hourString);

            if ((hour < 0) || (hour > 23))
                throw new Exception ();

            string minString = value.Substring (pos + 1, 2);
            int min = Convert.ToInt32 (minString);

            if ((min < 0) || (min > 59))
                throw new Exception ();

            pos = value.Length;
            if (pos > 3) {
                string last = value.Substring (pos - 2);
                if (string.Equals (last, "pm", StringComparison.InvariantCultureIgnoreCase)) {
                    if ((hour >= 1) && (hour <= 12))
                        hour = (hour + 12) % 24;
                }
            }

            return new Time ((byte)hour, (byte)min);
        }

        public string TimeToString () {
            int h = _hour;
            string t = "AM";

            if (h > 12) {
                h %= 12;
                t = "PM";
            }
            return string.Format ("{0}:{1:00} {2}", h, _minute, t);
        }
    }
}

