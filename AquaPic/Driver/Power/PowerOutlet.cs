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
            public MyState autoState;
            public MyState fallback;
            public Coil OutletControl;
            public string owner;

            public OutletData (string name, StateSetterHandler outletSetter) {
                this.name = name;
                currentState = MyState.Off;
                manualState = MyState.Off;
                fallback = MyState.Off;
                mode = Mode.Manual;
                ampCurrent = 0.0f;
                wattPower = 0.0f;
                powerFactor = 1.0f;
                owner = "Power";
                OutletControl = new Coil (outletSetter);
                OutletControl.StateGetter = () => {
                    return false;
                };
            }

            public void SetAmperage (float c) {
                ampCurrent = c;
                wattPower = ampCurrent * Voltage * powerFactor;
            }
        }
    }
}

