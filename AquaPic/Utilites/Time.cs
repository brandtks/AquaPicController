using System;

namespace AquaPic.Utilites
{
    public class Time
    {
        public byte hour { get; set; }
        public byte min { get; set; }
        public byte sec { get; set; }
        public byte millisec { get; set; }

		public static Time TimeZero {
            get {
				return new Time (TimeSpan.Zero);
            }
        }

        public Time (byte hours, byte mins, byte secs, byte millisecs) {
            this.hour = hours;
            this.min = mins;
            this.sec = secs;
            this.millisec = millisecs;
        }

        public Time (byte hours, byte mins, byte secs) 
            : this (hours, mins, secs, 0) {
        }

        public Time (byte hours, byte mins)
            : this (hours, mins, 0, 0) {
        }

        public Time (TimeSpan value) 
            : this ((byte)value.Hours, (byte)value.Minutes, (byte)value.Seconds, (byte)value.Milliseconds) {
        }

        public Time (Time value)
            : this (value.hour, value.min, value.sec, value.millisec) {
        }

        public Time () {
            DateTime value = DateTime.Now;
			this.hour = (byte)value.Hour;
			this.min = (byte)value.Minute;
			this.sec = (byte)value.Second;
			this.millisec = (byte)value.Millisecond;
        }

        public void SetTime (TimeSpan value) {
			this.hour = (byte)value.Hours;
			this.min = (byte)value.Minutes;
			this.sec = (byte)value.Seconds;
			this.millisec = (byte)value.Milliseconds;
        }

        public void SetTime (Time value) {
            this.hour = value.hour;
            this.min = value.min;
            this.sec = value.sec;
            this.millisec = value.millisec;
        }

        public TimeSpan toTimeSpan () {
            TimeSpan val = new TimeSpan (0, hour, min, sec, millisec);
            return val;
        }

        public int CompareToTime (Time value) {
            return TimeSpan.Compare (toTimeSpan (), value.toTimeSpan ());
        }

        public bool EqualsShortTime (DateTime value) {
            if (value.Hour != hour)
                return false;

            if (value.Minute != min)
                return false;

            return true;
        }

        public void AddMinutes (int value) {
            TimeSpan val2 = new TimeSpan (hour, min + value, sec);
            SetTime (val2);
        }

        public void AddTimeToTime (Time value) {
            TimeSpan val = toTimeSpan ();
            val.Add (value.toTimeSpan ());
            SetTime (val);
        }

		public string TimeToString () {
            int h = hour;
            string t = "AM";

            if (h > 12) {
                h %= 12;
                t = "PM";
            }
            return string.Format ("{0}:{1:00} {2}", h, min, t);
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
    }
}

