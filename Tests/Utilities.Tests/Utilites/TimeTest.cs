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
using NUnit.Framework;

namespace GoodtimeDevelopment.Utilites.Test
{
    [TestFixture]
    public class TimeTest
    {
        [Test]
        public void BeforeAfterTest () {
            var t1 = new Time (8, 0, 30, 300);
            var t2 = new Time (13, 30, 15, 0);

            Assert.True (t1.Before (t2));
            Assert.True (t2.After (t1));
            Assert.False (t1.After (t2));
        }

        [Test]
        public void EqualsShortTimeTest () {
            var now = DateTime.Now;
            var t1 = new Time (now.Hour, now.Minute, now.Second, now.Millisecond);
            Assert.True (t1.EqualsShortTime (now));
        }

        [Test]
        public void AddHoursTest () {
            var t1 = new Time (0, 0, 0, 0);
            t1.AddHours (1);
            var t2 = new Time (1, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (23, 0, 0, 0);
            t1.AddHours (1);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 0, 0, 0);
            t1.AddHours (24);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (1, 0, 0, 0);
            t1.AddHours (-1);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 0, 0, 0);
            t1.AddHours (-1);
            t2 = new Time (23, 0, 0, 0);
            Assert.AreEqual (t2, t1);
        }

        [Test]
        public void AddMinutesTest () {
            var t1 = new Time (0, 0, 0, 0);
            t1.AddMinutes (30);
            var t2 = new Time (0, 30, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (23, 30, 0, 0);
            t1.AddMinutes (30);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 0, 0, 0);
            t1.AddMinutes (60);
            t2 = new Time (1, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 15, 0, 0);
            t1.AddMinutes (-15);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (1, 0, 0, 0);
            t1.AddMinutes (-60);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 15, 0, 0);
            t1.AddMinutes (-30);
            t2 = new Time (23, 45, 0, 0);
            Assert.AreEqual (t2, t1);
        }

        [Test]
        public void AddSecondsTest () {
            var t1 = new Time (0, 0, 0, 0);
            t1.AddSeconds (30);
            var t2 = new Time (0, 0, 30, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (23, 59, 30, 0);
            t1.AddSeconds (30);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 0, 0, 0);
            t1.AddSeconds (60);
            t2 = new Time (0, 1, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 0, 15, 0);
            t1.AddSeconds (-15);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 1, 0, 0);
            t1.AddSeconds (-60);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 0, 15, 0);
            t1.AddSeconds (-30);
            t2 = new Time (23, 59, 45, 0);
            Assert.AreEqual (t2, t1);
        }

        [Test]
        public void AddMilliSecondsTest () {
            var t1 = new Time (0, 0, 0, 0);
            t1.AddMilliSeconds (500);
            var t2 = new Time (0, 0, 0, 500);
            Assert.AreEqual (t2, t1);

            t1 = new Time (23, 59, 59, 500);
            t1.AddMilliSeconds (500);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 0, 0, 0);
            t1.AddMilliSeconds (1000);
            t2 = new Time (0, 0, 1, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 0, 0, 500);
            t1.AddMilliSeconds (500);
            t2 = new Time (0, 0, 1, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 0, 1, 0);
            t1.AddMilliSeconds (-1000);
            t2 = new Time (0, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            t1 = new Time (0, 0, 0, 500);
            t1.AddMilliSeconds (-1000);
            t2 = new Time (23, 59, 59, 500);
            Assert.AreEqual (t2, t1);
        }

        [Test]
        public void AddTimeTest () {
            var t1 = new Time (1, 15, 30, 500);
            var t2 = new Time (0, 45, 30, 500);
            t1.AddTime (t2);
            t2 = new Time (2, 1, 1, 0);
            Assert.AreEqual (t2, t1);
        }

        [Test]
        public void ParseTest () {
            var str = "8:15 AM";
            var t1 = Time.Parse (str);
            var t2 = new Time (8, 15);
            Assert.AreEqual (t2, t1);

            str = "16:15";
            t1 = Time.Parse (str);
            t2 = new Time (16, 15);
            Assert.AreEqual (t2, t1);

            str = "4:15 PM";
            t1 = Time.Parse (str);
            Assert.AreEqual (t2, t1);

            str = "00:00:00:00";
            t1 = Time.Parse (str);
            t2 = Time.TimeZero;
            Assert.AreEqual (t2, t1);

            str = "8:15:45AM";
            t1 = Time.Parse (str);
            t2 = new Time (8, 15, 45);
            Assert.AreEqual (t2, t1);

            str = "8:15:45:540";
            t1 = Time.Parse (str);
            t2 = new Time (8, 15, 45, 540);
            Assert.AreEqual (t2, t1);

            str = "8:15:45:540 AM";
            t1 = Time.Parse (str);
            t2 = new Time (8, 15, 45, 540);
            Assert.AreEqual (t2, t1);

            str = "10:30 PM";
            t1 = Time.Parse (str);
            t2 = new Time (22, 30, 0, 0);
            Assert.AreEqual (t2, t1);

            str = "10 PM";
            t1 = Time.Parse (str);
            t2 = new Time (22, 0, 0, 0);
            Assert.AreEqual (t2, t1);

            str = "2";
            t1 = Time.Parse (str);
            t2 = new Time (2, 0, 0, 0);
            Assert.AreEqual (t2, t1);
        }

        [Test]
        public void ToShortTimeStringTest () {
            var t1 = new Time (8, 0);
            Assert.AreEqual ("8:00 AM", t1.ToShortTimeString ());

            t1 = new Time (15, 0);
            Assert.AreEqual ("3:00 PM", t1.ToShortTimeString ());
        }
        
        [Test]
        public void EqualsTest () {
            var t1 = new Time (8, 0, 0, 0);
            var t2 = new Time (8, 0);
            Assert.True (t1.Equals (t2));

            t2 = new Time (8, 30);
            Assert.False (t1.Equals (t2));

            t2 = new Time (9, 0);
            Assert.False (t1.Equals (t2));

            t2 = new Time (8, 0, 30);
            Assert.False (t1.Equals (t2));

            t2 = new Time (8, 0, 0, 500);
            Assert.False (t1.Equals (t2));
        }

        [Test]
        public void ImplicitStringTest () {
            Time t1 = "8:30:15:300";
            var t2 = new Time (8, 30, 15, 300);
            Assert.AreEqual (t2, t1);
        }
    }
}

