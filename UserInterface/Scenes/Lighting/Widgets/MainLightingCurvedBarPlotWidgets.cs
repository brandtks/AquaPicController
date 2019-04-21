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
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Gadgets.Device;

namespace AquaPic.UserInterface
{
    public class DimmingLightCurvedBarPlot : CurvedBarPlotWidget
    {
        public DimmingLightCurvedBarPlot (string group, int row, int column) : base ("Lighting", group, row, column) {
            text = "No Light";
            unitOfMeasurement = UnitsOfMeasurement.Percentage;

            this.group = group;
            if (Devices.Lighting.CheckGadgetKeyNoThrow (this.group)) {
                if (Devices.Lighting.IsDimmingFixture (this.group)) {
                    text = this.group;
                    WidgetReleaseEvent += (o, args) => {
                        AquaPicGui.AquaPicUserInterface.ChangeScreens ("Lighting", Toplevel, AquaPicGui.AquaPicUserInterface.currentScene, this.group);
                    };
                } else {
                    this.group = string.Empty;
                }
            } else {
                this.group = string.Empty;
            }
        }

        public override void Update () {
            if (group.IsNotEmpty ()) {
                currentValue = Devices.Lighting.GetCurrentDimmingLevel (group);
            }
        }
    }
}

