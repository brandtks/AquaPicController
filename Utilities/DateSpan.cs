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

namespace GoodtimeDevelopment.Utilites
{
    public class DateSpan : Time
    {
        protected int _year;
        public int year {
            get {
                return _year;
            }
        }

        protected int _month;
        public int month {
            get {
                return _month;
            }
        }

        protected int _day;
        public int day {
            get {
                return _day;
            }
        }

        public static DateSpan Zero {
            get {
                return new DateSpan (0, 0, 0, 0, 0, 0, 0);
            }
        }

        public static DateSpan Now {
            get {
                return new DateSpan (DateTime.Now);
            }
        }

        public DateSpan (int year, int month, int day, int hour, int minute, int second, int millisecond) {
            _year = year;
            _month = month;
            _day = day;
            _hour = hour;
            _minute = minute;
            _second = second;
            _millisecond = millisecond;
        }

        public DateSpan (int year, int month, int day, int hour, int minute, int second)
            : this (year, month, day, hour, minute, second, 0) {
        }

        public DateSpan (int year, int month, int day, int hour, int minute)
            : this (year, month, day, hour, minute, 0, 0) {
        }

        public DateSpan (int hour, int minute, int second)
            : this (0, 0, 0, hour, minute, second, 0) {
            DateTime now = DateTime.Now;
            _year = now.Year;
            _month = now.Month;
            _day = now.Day;
        }

        public DateSpan (int hour, int minute, int second, int millisecond)
            : this (0, 0, 0, hour, minute, second, millisecond) {
            DateTime now = DateTime.Now;
            _year = now.Year;
            _month = now.Month;
            _day = now.Day;
        }

        public DateSpan (DateTime dateTime)
            : this (dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond) {
        }

        public DateSpan (Time time)
            : this (0, 0, 0, time.hour, time.minute, time.second, time.millisecond) {
            DateTime now = DateTime.Now;
            _year = now.Year;
            _month = now.Month;
            _day = now.Day;
        }

        public DateSpan () {
            DateTime value = DateTime.Now;
            _year = value.Year;
            _month = value.Month;
            _day = value.Day;
            _hour = value.Hour;
            _minute = value.Minute;
            _second = value.Second;
            _millisecond = value.Millisecond;
        }

        public void UpdateDateToToday () {
            DateTime now = DateTime.Now;
            _year = now.Year;
            _month = now.Month;
            _day = now.Day;
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
            return CompareToTime (dateSpan) == -1;
        }

        public bool AfterTime (DateSpan dateSpan) {
            return CompareToTime (dateSpan) == 1;
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
            return new DateTime (_year, _month, _day, _hour, _minute, _second, _millisecond);
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

