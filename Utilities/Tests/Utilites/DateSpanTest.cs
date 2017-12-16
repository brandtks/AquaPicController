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
    public class DateSpanTest
    {
        [Test]
        public void BeforeAfterTest () {
            var ds1 = new DateSpan (2017, 1, 1, 12, 30, 0, 0);
            var ds2 = new DateSpan (2017, 1, 2, 12, 30, 0, 0);
            Assert.True (ds1.Before (ds2));
            Assert.True (ds2.After (ds1));
            Assert.False (ds1.After (ds2));
        }

        [Test]
        public void BeforeAfterTimeTest () {
            var ds1 = new DateSpan (2017, 1, 1, 11, 0, 0, 0);
            var ds2 = new DateSpan (1940, 12, 25, 12, 0, 0, 0);
            Assert.True (ds1.BeforeTime (ds2));
            Assert.True (ds2.AfterTime (ds1));
            Assert.False (ds1.AfterTime (ds2));
        }

        [Test]
        public void DifferenceInMintuesTest () {
            var ds1 = new DateSpan (2017, 1, 1, 11, 0, 0, 0);
            var ds2 = new DateSpan (2017, 1, 1, 12, 0, 0, 0);

            var difference = ds1.DifferenceInMinutes (ds2);
            Assert.AreEqual (-60d, difference);

            difference = ds2.DifferenceInMinutes (ds1);
            Assert.AreEqual (60d, difference);

            difference = ds1.DifferenceInMinutes (ds1);
            Assert.AreEqual (0d, difference);
        }

        [Test]
        public void DifferenceInSecondsTest () {
            var ds1 = new DateSpan (2017, 1, 1, 12, 0, 0, 0);
            var ds2 = new DateSpan (2017, 1, 1, 12, 1, 0, 0);

            var difference = ds1.DifferenceInSeconds (ds2);
            Assert.AreEqual (-60d, difference);

            difference = ds2.DifferenceInSeconds (ds1);
            Assert.AreEqual (60d, difference);

            difference = ds1.DifferenceInSeconds (ds1);
            Assert.AreEqual (0d, difference);
        }

        [Test]
        public void AddDaysTest () {
            var ds1 = new DateSpan (2017, 1, 1, 12, 0, 0, 0);
            var ds2 = new DateSpan (2017, 1, 2, 12, 0, 0, 0);
            ds1.AddDays (1);
            Assert.AreEqual (ds2, ds1);
        }

        [Test]
        public void UpdateTimeTest () {
            var ds1 = new DateSpan (2017, 1, 1, 12, 0, 0, 0);
            var ds2 = new DateSpan (2017, 1, 1, 13, 30, 30, 500);
            var ds3 = new DateSpan (2017, 1, 1, 2, 45, 15, 250);
            var t = new Time (13, 30, 30, 500);
            var ts = new TimeSpan (0, 2, 45, 15, 250);

            ds1.UpdateTime (t);
            Assert.AreEqual (ds2, ds1);

            ds1.UpdateTime (ts);
            Assert.AreEqual (ds3, ds1);
        }

        [Test]
        public void EqualsTest () {
            var ds1 = new DateSpan (2017, 1, 1, 12, 0, 0, 0);
            var ds2 = new DateSpan (2017, 1, 1, 12, 0, 0, 0);
            Assert.AreEqual (ds2, ds1);

            ds1 = new DateSpan (2016, 1, 1, 12, 0, 0, 0);
            Assert.AreNotEqual (ds2, ds1);

            ds1 = new DateSpan (2017, 2, 1, 12, 0, 0, 0);
            Assert.AreNotEqual (ds2, ds1);

            ds1 = new DateSpan (2017, 1, 2, 12, 0, 0, 0);
            Assert.AreNotEqual (ds2, ds1);

            ds1 = new DateSpan (2017, 1, 1, 11, 0, 0, 0);
            Assert.AreNotEqual (ds2, ds1);

            ds1 = new DateSpan (2017, 1, 1, 12, 30, 0, 0);
            Assert.AreNotEqual (ds2, ds1);

            ds1 = new DateSpan (2017, 1, 1, 12, 0, 15, 0);
            Assert.AreNotEqual (ds2, ds1);

            ds1 = new DateSpan (2017, 1, 1, 12, 0, 0, 500);
            Assert.AreNotEqual (ds2, ds1);
        }
    }
}

ert.AreNotEqual (ds2, ds1);

            ds1 = new DateSpan (2017, 1, 1, 12, 0, 0, 500);
            Assert.AreNotEqual (ds2, ds1);
        }
    }
}

