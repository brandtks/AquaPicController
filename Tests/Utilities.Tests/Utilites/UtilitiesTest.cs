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
using System.Collections.Generic;
using NUnit.Framework;

namespace GoodtimeDevelopment.Utilites.Test
{
    [TestFixture]
    public class UtilitiesTest
    {
        [Test] 
        public void TypeIsTest () {
            Assert.True (typeof (int[]).TypeIs (typeof (IList<>)));
            Assert.False (typeof (int[]).TypeIs (typeof (int)));
        }

        [Test]
        public void WithinRangeTest () {
            Assert.True (90.WithinRange (90, 100));
            Assert.True (95.WithinRange (90, 100));
            Assert.True (100.WithinRange (90, 100));
            Assert.False (89.WithinRange (90, 100));
            Assert.False (101.WithinRange (90, 100));

            Assert.True (90d.WithinRange (90, 100));
            Assert.True (95d.WithinRange (90, 100));
            Assert.True (100d.WithinRange (90, 100));
            Assert.False (89.999.WithinRange (90, 100));
            Assert.False (100.001.WithinRange (90, 100));
        }
    }
}
