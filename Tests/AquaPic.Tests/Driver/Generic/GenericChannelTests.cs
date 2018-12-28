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

namespace AquaPic.Drivers.Test
{
    [TestFixture]
    public class GenericChannelTests
    {
        [Test]
        public void TestSetValue () {
            var channel = new GenericChannel ("Test", typeof (float));
            channel.SetValue ((short)1024);
            Assert.AreEqual (1024f, channel.value);

            channel = new GenericChannel ("Test", typeof (bool));
            channel.SetValue (1);
            Assert.AreEqual (true, channel.value);

            channel.SetValue (0);
            Assert.AreEqual (false, channel.value);
        }
    }
}
