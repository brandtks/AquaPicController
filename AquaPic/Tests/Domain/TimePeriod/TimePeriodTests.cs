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
            var ds1 = new DateSpan (new Time (8, 15, 30));
            var ds2 = new DateSpan (new Time (12, 30));
            var start = "8:15:30";
            var end = "12:30";
            var tp = new TimePeriod (start, end);
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
