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
using System.Linq;

namespace GoodtimeDevelopment.Utilites
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

        public static Time TimeNow {
            get {
                var now = DateTime.Now;
                return new Time (now.Hour, now.Minute, now.Second, now.Millisecond);
            }
        }

        public static implicit operator Time (string timeString) {
            return Parse (timeString);
        }

        public Time (int hour, int minute, int second, int millisecond) {
            _hour = hour;
            _minute = minute;
            _second = second;
            _millisecond = millisecond;
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

        public bool Before (Time time) {
            return CompareToTime (time) == -1;
        }

        public bool After (Time time) {
            return CompareToTime (time) == 1;
        }

        // returns 1 if after value, -1 if before value
        protected int CompareToTime (Time value) {
            return TimeSpan.Compare (ToTimeSpan (), value.ToTimeSpan ());
        }

        public bool EqualsShortTime (DateTime value) {
            if (value.Hour != _hour)
                return false;

            if (value.Minute != _minute)
                return false;

            return true;
        }

        public void AddHours (int numberOfHours) {
            TimeSpan timeSpan = new TimeSpan (0, _hour + numberOfHours, _minute, _second, _millisecond);
            _hour = timeSpan.Hours;
            _minute = timeSpan.Minutes;
            _second = timeSpan.Seconds;
            _millisecond = timeSpan.Milliseconds;
        }

        public void AddMinutes (int numberOfMinutes) {
            TimeSpan timeSpan = new TimeSpan (0, _hour, _minute + numberOfMinutes, _second, _millisecond);
            _hour = timeSpan.Hours;
            _minute = timeSpan.Minutes;
            _second = timeSpan.Seconds;
            _millisecond = timeSpan.Milliseconds;
        }

        public void AddSeconds (int numberOfSeconds) {
            TimeSpan timeSpan = new TimeSpan (0, _hour, _minute, _second + numberOfSeconds, _millisecond);
            _hour = timeSpan.Hours;
            _minute = timeSpan.Minutes;
            _second = timeSpan.Seconds;
            _millisecond = timeSpan.Milliseconds;
        }

        public void AddMilliSeconds (int numberOfMilliSeconds) {
            TimeSpan timeSpan = new TimeSpan (0, _hour, _minute, _second, _millisecond + numberOfMilliSeconds);
            _hour = timeSpan.Hours;
            _minute = timeSpan.Minutes;
            _second = timeSpan.Seconds;
            _millisecond = timeSpan.Milliseconds;
        }

        public void AddTime (Time time) {
            var timeSpan = ToTimeSpan ().Add (time.ToTimeSpan ());
            _hour = timeSpan.Hours;
            _minute = timeSpan.Minutes;
            _second = timeSpan.Seconds;
            _millisecond = timeSpan.Milliseconds;
        }

        public Time Negate () {
            return new Time (ToTimeSpan ().Negate ());
        }

        public static Time Parse (string value) {
            var seperator = new char[2] { ':' , '.' };
            var t = value.Split (seperator, 4);

            int hours = 0, minutes = 0, seconds = 0, milliseconds = 0;
            string hrFormatString = string.Empty;

            var lastElement = t.Length - 1;
            if (t[lastElement].Length > 2) { // there might be am or pm after the last element
                int splitPoint = t[lastElement].Length - 2; // the last two character might be am or pm
                hrFormatString = t[lastElement].Substring (splitPoint); // the the last two characters
                if (hrFormatString.All (x => char.IsLetter (x))) { // make sure the last two characters are letter characters
                    t[lastElement] = t[lastElement].Substring (0, splitPoint); // remove the last two characters
                } else { // If the last two character are not letter than there isn't a hour format string
                    hrFormatString = string.Empty;
                }
            }

            if (t.Length < 2) {
                throw new Exception ("The string can not be parsed into a Time value");
            }

            // the value to parse has at least the hours and seconds
            hours = Convert.ToInt32 (t[0]);
            if (string.Equals (hrFormatString, "pm", StringComparison.InvariantCultureIgnoreCase)) {
                if (hours < 13) { // the hours spot indicates AM time period
                    hours = (hours + 12) % 24;
                }
            }
            if (hours == 24) { // convert the 24th hour to 0
                hours = 0;
            }

            minutes = Convert.ToInt32 (t[1]);

            if ((hours < 0) || (hours > 23))
                throw new Exception ("Invalid hours");

            if ((minutes < 0) || (minutes > 59))
                throw new Exception ("Invalid minutes");

            if (t.Length >= 3) { // the value to parse contains seconds
                seconds = Convert.ToInt32 (t[2]);
            }

            if ((seconds < 0) || (seconds > 59))
                throw new Exception ("Invalid seconds");

            if (t.Length >= 4) { // the value to parse contains milliseconds
                milliseconds = Convert.ToInt32 (t[3]);
            }

            return new Time (hours, minutes, seconds, milliseconds);
        }

        public string ToShortTimeString () {
            int h = _hour;
            string t = "AM";

            if (h > 12) {
                h %= 12;
                t = "PM";
            }

            return string.Format ("{0}:{1:00} {2}", h, _minute, t);
        }

        public override string ToString () {
            return string.Format ("{0}:{1}:{2}.{3}", hour, minute, second, millisecond);
        }

        public override bool Equals (object obj) {
            if (this == obj) {
                return true;
            }

            if (!(obj is Time)) {
                return false;
            }

            var time = (Time)obj;
            var equality = hour == time.hour;
            equality &= minute == time.minute;
            equality &= second == time.second;
            equality &= millisecond == time.millisecond;
            return equality;
        }

        public override int GetHashCode () {
            return hour ^ minute ^ second ^ millisecond;
        }
    }
}

