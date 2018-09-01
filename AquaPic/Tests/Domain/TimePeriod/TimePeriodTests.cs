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
using NUnit.Framework;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Runtime.Test
{
    [TestFixture]
    public class TimePeriodTests
    {
        [Test]
        public void TimePeriodConstructorTest () {
            AbstractTimes.UpdateRiseSetTimes ();

            var start = "sunrise";
            var end = "sunset";
            var tp = new TimePeriod (start, end);
            Assert.AreEqual (AbstractTimes.sunRiseToday, tp.startTime);
            Assert.AreEqual (AbstractTimes.sunSetToday, tp.endTime);

            start = "sunset";
            end = "sunrise";
            tp = new TimePeriod (start, end);
            Assert.AreEqual (AbstractTimes.sunSetToday, tp.startTime);
            Assert.AreEqual (AbstractTimes.sunRiseTomorrow, tp.endTime);

            var ds1 = AbstractTimes.sunRiseToday;
            ds1.AddHours (2);
            start = "sunrise";
            end = "sunrise + 2hr";
            tp = new TimePeriod (start, end);
            Assert.AreEqual (AbstractTimes.sunRiseToday, tp.startTime);
            Assert.AreEqual (ds1, tp.endTime);

            ds1 = AbstractTimes.sunSetToday;
            ds1.AddHours (-2);
            start = "sunset - 2hr";
            end = "sunset";
            tp = new TimePeriod (start, end);
            Assert.AreEqual (ds1, tp.startTime);
            Assert.AreEqual (AbstractTimes.sunSetToday, tp.endTime);

            ds1 = new DateSpan (new Time (8, 15, 30));
            var ds2 = new DateSpan (new Time (12, 30));
            start = "8:15:30";
            end = "12:30";
            tp = new TimePeriod (start, end);
            Assert.AreEqual (ds1, tp.startTime);
            Assert.AreEqual (ds2, tp.endTime);

            ds1 = new DateSpan (new Time (22, 30));
            ds2 = new DateSpan (new Time (2, 15));
            ds2.AddDays (1);
            start = "10:30PM";
            end = "2:15am";
            tp = new TimePeriod (start, end);
            Assert.AreEqual (ds1, tp.startTime);
            Assert.AreEqual (ds2, tp.endTime);
        }
    }
}
