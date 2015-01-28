﻿using System;

namespace AquaPic.Utilites
{
    public class Time
    {
        public byte hour { get; set; }
        public byte min { get; set; }
        public byte sec { get; set; }
        public byte millisec { get; set; }

		public static Time TZero {
            get {
				return new Time (TimeSpan.Zero);
            }
        }

        public Time (byte hours, byte mins, byte secs) {
            this.hour = hours;
            this.min = mins;
            this.sec = secs;
            this.millisec = 0;
        }

        public Time (byte hours, byte mins, byte secs, byte millisecs) {
            this.hour = hours;
            this.min = mins;
            this.sec = secs;
            this.millisec = millisecs;
        }

        public Time (TimeSpan value) {
			this.hour = (byte)value.Hours;
			this.min = (byte)value.Minutes;
			this.sec = (byte)value.Seconds;
			this.millisec = (byte)value.Milliseconds;
        }

        public Time () {
            DateTime value = DateTime.Now;
			this.hour = (byte)value.Hour;
			this.min = (byte)value.Minute;
			this.sec = (byte)value.Second;
			this.millisec = (byte)value.Millisecond;
        }

        public void setTime (TimeSpan value) {
			hour = (byte)value.Hours;
			min = (byte)value.Minutes;
			sec = (byte)value.Seconds;
			millisec = (byte)value.Milliseconds;
        }

        public TimeSpan toTimeSpan () {
            TimeSpan val = new TimeSpan (0, hour, min, sec, millisec);
            return val;
        }

        public int compareToTime (Time value) {
            return TimeSpan.Compare (toTimeSpan (), value.toTimeSpan ());
        }

        public void addMinToTime (int value) {
            TimeSpan val1 = toTimeSpan ();
            TimeSpan val2 = new TimeSpan (hour, min + value, sec);
            val1.Add (val2);
            setTime (val1);
        }

        public void addTimeToTime (Time value) {
            TimeSpan val = toTimeSpan ();
            val.Add (value.toTimeSpan ());
            setTime (val);
        }

		public string toString () {
			TimeSpan val = toTimeSpan ();
			return val.ToString ();
		}
    }
}

