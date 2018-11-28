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
    public class WaterLevelWidget : BarPlotWidget
    {
        string groupName;
        TouchLabel label;

        public WaterLevelWidget (params object[] options)
            : base () {
            text = "Water Level";
            unitOfMeasurement = UnitsOfMeasurement.Inches;

            label = new TouchLabel ();
            label.textColor = "compl";
            label.text = "Disconnected";
            label.WidthRequest = 199;
            label.textAlignment = TouchAlignment.Center;
            label.textRender.orientation = TouchOrientation.Vertical;
            Put (label, 60, 9);
            label.Show ();

            groupName = string.Empty;
            if (options.Length >= 1) {
                var groupNameOption = options[0] as string;
                if (groupNameOption != null) {
                    if (WaterLevel.CheckWaterLevelGroupKeyNoThrow (groupNameOption)) {
                        groupName = groupNameOption;
                    }
                }
            }

            if (groupName.IsNotEmpty ()) {
                text = groupName;
            }

            var eventbox = new EventBox ();
            eventbox.VisibleWindow = false;
            eventbox.SetSizeRequest (WidthRequest, HeightRequest);
            eventbox.ButtonReleaseEvent += (o, args) => {
                AquaPicGui.AquaPicUserInterface.ChangeScreens ("Water Level", Toplevel, AquaPicGui.AquaPicUserInterface.currentScene);
            };
            Put (eventbox, 0, 0);
            eventbox.Show ();

            fullScale = 15.0f;
            OnUpdate ();
        }

        public override void OnUpdate () {
            if (groupName.IsNotEmpty ()) {
                if (!WaterLevel.GetWaterLevelGroupAnalogSensorConnected (groupName)) {
                    textBox.text = "--";
                    label.Visible = true;
                } else {
                    currentValue = WaterLevel.GetWaterLevelGroupLevel (groupName);
                    label.Visible = false;
                }
            }
        }
    }
}
