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
using GoodtimeDevelopment.Utilites;
using AquaPic.Drivers;
using AquaPic.Service;

namespace AquaPic.Gadgets.Device.Heater
{
    public class Heater : GenericDevice
    {
        public string temperatureGroup;
        public string bitName;

        public Heater (HeaterSettings settings) : base (settings) {
            temperatureGroup = settings.temperatureGroup;
            bitName = temperatureGroup.RemoveWhitespace () + "HeaterRequest";
            Driver.Power.AddOutlet (channel, name, true, key);
        }

        protected override ValueType OnRun () {
            return Bit.Instance.Check (bitName);
        }

        public override void Dispose () {
            Driver.Power.RemoveChannel (channel);
        }
    }
}

