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
using AquaPic.Globals;

namespace AquaPic.Drivers
{
    public partial class PowerBase
    {
        protected class PowerOutlet : GenericOutputChannel
        {
            public static float Voltage = 115;

            public float wattPower;
            public float amperage;
            public float powerFactor;
            public MyState fallback;

            public PowerOutlet (string name) : base (name, typeof(bool)) {
                this.name = name;
                fallback = MyState.Off;
                mode = Mode.Manual;
                amperage = 0.0f;
                wattPower = 0.0f;
                powerFactor = 1.0f;
            }

            public void SetAmperage (float c) {
                amperage = c;
                wattPower = amperage * Voltage * powerFactor;
            }
        }
    }
}

