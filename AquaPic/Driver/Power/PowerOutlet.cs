#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using AquaPic.Globals;
using AquaPic.Operands;

namespace AquaPic.Drivers
{
    public partial class Power 
    {
        private class OutletData
        {
            public static float Voltage = 115;

            public float wattPower;
            public float ampCurrent;
            public Mode mode;
            public float powerFactor;
            public string name;
            public MyState currentState;
            public MyState manualState;
            public MyState fallback;
            public Coil OutletControl;
            public string owner;

            public OutletData (string name, ConditionSetterHandler outletSetter) {
                this.name = name;
                currentState = MyState.Off;
                manualState = MyState.Off;
                fallback = MyState.Off;
                mode = Mode.Manual;
                ampCurrent = 0.0f;
                wattPower = 0.0f;
                powerFactor = 1.0f;
                OutletControl = new Coil ();
                OutletControl.ConditionGetter = () => {
                    return false;
                };
                OutletControl.ConditionSetter = outletSetter;
                owner = "Power";
            }

            public void SetAmpCurrent (float c) {
                ampCurrent = c;
                wattPower = ampCurrent * Voltage * powerFactor;
            }
        }
    }
}

