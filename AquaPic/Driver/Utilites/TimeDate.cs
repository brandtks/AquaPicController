using System;

namespace AquaPic.Utilites
{
    public class TimeDate : Time
    {
		public int year { get; set; }
        public byte month { get; set; }
        public byte day { get; set; }

		public static TimeDate Zero { 
            get {
				return new TimeDate (0, 0, 0, 0, 0, 0, 0);
            }
        }

		public static TimeDate Now {
            get {
				return new TimeDate (DateTime.Now);
            }
        }

        public TimeDate (byte hours, byte mins, byte secs) {
            DateTime now = DateTime.Now;
			this.year = now.Year;
			this.month = (byte)now.Month;
			this.day = (byte)now.Day;
            this.hour = hours;
            this.min = mins;
            this.sec = secs;
            this.millisec = 0;
        }

        public TimeDate (byte hours, byte mins, byte secs, byte millisecs) {
            DateTime now = DateTime.Now;
			this.year = now.Year;
			this.month = (byte)now.Month;
			this.day = (byte)now.Day;
            this.hour = hours;
            this.min = mins;
            this.sec = secs;
            this.millisec = millisecs;
        }

		public TimeDate (int years, byte months, byte days, byte hours, byte mins, byte secs, byte millisecs) {
            this.year = years;
            this.month = months;
            this.day = days;
            this.hour = hours;
            this.min = mins;
            this.sec = secs;
            this.millisec = millisecs;
        }

        public TimeDate (DateTime value) {
			this.year = value.Year;
			this.month = (byte)value.Month;
			this.day = (byte)value.Day;
			this.hour = (byte)value.Hour;
			this.min = (byte)value.Minute;
			this.sec = (byte)value.Second;
			this.millisec = (byte)value.Millisecond;
        }

        public TimeDate (Time value) {
            DateTime now = DateTime.Now;
            this.year = now.Year;
            this.month = (byte)now.Month;
            this.day = (byte)now.Day;
            this.hour = value.hour;
            this.min = value.min;
            this.sec = value.sec;
            this.millisec = value.millisec;
        }

        public TimeDate () {
			DateTime value = DateTime.Now;
			this.year = value.Year;
			this.month = (byte)value.Month;
			this.day = (byte)value.Day;
			this.hour = (byte)value.Hour;
			this.min = (byte)value.Minute;
			this.sec = (byte)value.Second;
			this.millisec = (byte)value.Millisecond;
        }

        public void setTimeDate (TimeDate value) {
            year = value.year;
            month = value.month;
            day = value.day;
            hour = value.hour;
            min = value.min;
            sec = value.sec;
            millisec = value.millisec;
        }

        public void setTimeDate (DateTime value) {
			year = value.Year;
			month = (byte)value.Month;
			day = (byte)value.Day;
			hour = (byte)value.Hour;
			min = (byte)value.Minute;
			sec = (byte)value.Second;
			millisec = (byte)value.Millisecond;
        }

        public void setTimeDate (Time value) {
            DateTime now = DateTime.Now;
            this.year = now.Year;
            this.month = (byte)now.Month;
            this.day = (byte)now.Day;
            this.hour = value.hour;
            this.min = value.min;
            this.sec = value.sec;
            this.millisec = value.millisec;
        }

        public void updateDateToToday () {
            DateTime now = DateTime.Now;
            year = now.Year;
            month = (byte)now.Month;
            day = (byte)now.Day;
        }

        // returns 1 if after value, -1 if before value
        public int compareTo (TimeDate value) {
            DateTime val = toDateTime ();
            return val.CompareTo (value.toDateTime ());
        }

        // returns 1 if after value, -1 if before value
        public int compareTo (DateTime value) {
            DateTime val = toDateTime ();
            return val.CompareTo (value);
        }

        // returns 1 if after value, -1 if before value
        public int compareToTime (TimeDate value) {
            return TimeSpan.Compare (toTimeSpan (), value.toTimeSpan ());
        }

        public double DifferenceInTime(TimeDate value) {
            DateTime val = value.toDateTime();
            TimeSpan span = val.Subtract(this.toDateTime());
            return span.TotalMinutes;
        }

        public DateTime toDateTime () {
            DateTime val = new DateTime (year, month, day, hour, min, sec, millisec);
            return val;
        }

        public void addMinToDate (int value) {
            DateTime val = toDateTime ();
            val.AddMinutes (value);
            setTimeDate (val);
        }

        public void addTimeToDate (Time value) {
            DateTime val = toDateTime ();
            val.Add (value.toTimeSpan ());
            setTimeDate (val);
        }

		public override string ToString () {
			DateTime val = toDateTime ();
			return val.ToString ();
		}

        public override string ToString (string format) {
			DateTime val = toDateTime ();
			return val.ToString (format);
		}

        public override string ToString (IFormatProvider provider) {
			DateTime val = toDateTime ();
			return val.ToString (provider);
		}

        public override string ToString (string format, IFormatProvider provider) {
			DateTime val = toDateTime ();
			return val.ToString (format, provider);
		}
    }
}

