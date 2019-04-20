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
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Service.Test
{
    [TestFixture]
    public class StaticTimerTests
    {
        [Test]
        public void TestParse () {
            var str = "2hr31min15sec400ms";
            var t1 = Timer.Parse (str);
            var t2 = new Time (2, 31, 15, 400);
            Assert.AreEqual (t2, t1);

            str = "2min30sec";
            t1 = Timer.Parse (str);
            t2 = new Time (0, 2, 30);
            Assert.AreEqual (t2, t1);

            str = "300sec";
            t1 = Timer.Parse (str);
            t2 = new Time (0, 0, 300);
            Assert.AreEqual (t2, t1);

            str = "1hr10sec";
            t1 = Timer.Parse (str);
            t2 = new Time (1, 0, 10);
            Assert.AreEqual (t2, t1);

            str = "300";
            try {
                t1 = Timer.Parse (str);
                Assert.Fail ();
            } catch {
                //
            }

            str = "hr";
            try {
                t1 = Timer.Parse (str);
                Assert.Fail ();
            } catch {
                //
            }
        }
    }
}
