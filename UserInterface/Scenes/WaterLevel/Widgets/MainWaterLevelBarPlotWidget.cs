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
using Gtk;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    public class WaterLevelBarPlotWidget : BarPlotWidget
    {
        TouchLabel label;

        public WaterLevelBarPlotWidget (string group, int row, int column) : base ("Water Level", group, row, column) {
            text = "No Water Level";
            unitOfMeasurement = UnitsOfMeasurement.Inches;
            fullScale = 15f;

            label = new TouchLabel ();
            label.textColor = "compl";
            label.text = "Disconnected";
            label.textAlignment = TouchAlignment.Center;
            label.textRender.orientation = TouchOrientation.Vertical;
            Put (label, 60, 9);
            label.Show ();

            this.group = group;
            if (WaterLevel.CheckWaterLevelGroupKeyNoThrow (this.group)) {
                text = this.group;
                WidgetReleaseEvent += (o, args) => {
                    AquaPicGui.AquaPicUserInterface.ChangeScreens ("Water Level", Toplevel, AquaPicGui.AquaPicUserInterface.currentScene, this.group);
                };
                fullScale = WaterLevel.GetWaterLevelGroupMaxLevel (this.group);
            } else {
                this.group = string.Empty;
            }
        }

        public override void Update () {
            if (group.IsNotEmpty ()) {
                if (!WaterLevel.GetWaterLevelGroupAnalogSensorConnected (group)) {
                    textBox.text = "--";
                    label.Visible = true;
                } else {
                    currentValue = WaterLevel.GetWaterLevelGroupLevel (group);
                    label.Visible = false;
                }
            }
        }
    }
}

