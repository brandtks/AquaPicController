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
    public class WaterLevelLinePlot : LinePlotWidget
    {
        string groupName;
        TouchLabel label;

        public WaterLevelLinePlot (params object[] options)
            : base () {
            text = "Water Level";
            unitOfMeasurement = UnitsOfMeasurement.Inches;

            var eventbox = new EventBox ();
            eventbox.VisibleWindow = false;
            eventbox.SetSizeRequest (WidthRequest, HeightRequest);
            eventbox.ButtonReleaseEvent += (o, args) => {
                var topWidget = this.Toplevel;
                AquaPicGui.AquaPicUserInterface.ChangeScreens ("Water Level", topWidget, AquaPicGui.AquaPicUserInterface.currentScene);
            };
            Put (eventbox, 0, 0);
            eventbox.Show ();

            label = new TouchLabel ();
            label.SetSizeRequest (152, 16);
            label.textColor = "compl";
            label.textAlignment = TouchAlignment.Right;
            label.textHorizontallyCentered = true;
            Put (label, 155, 63);

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
                var dataLogger = WaterLevel.GetWaterLevelGroupDataLogger (groupName);
                linePlot.LinkDataLogger (dataLogger);

                Destroyed += (obj, args) => {
                    linePlot.UnLinkDataLogger (dataLogger);
                };

                text = string.Format ("{0} Water Level", groupName);
            }

            linePlot.rangeMargin = 1;
            linePlot.eventColors.Add ("probe disconnected", new TouchColor ("secb", 0.25));
            linePlot.eventColors.Add ("ato started", new TouchColor ("seca", 0.5));
            linePlot.eventColors.Add ("ato stopped", new TouchColor ("secc", 0.5));
            linePlot.eventColors.Add ("low alarm", new TouchColor ("compl", 0.25));
            linePlot.eventColors.Add ("high alarm", new TouchColor ("compl", 0.25));
            linePlot.eventColors.Add ("disconnected alarm", new TouchColor ("compl", 0.25));

            OnUpdate ();
        }

        public override void OnUpdate () {
            if (groupName.IsNotEmpty ()) {
                if (!WaterLevel.GetWaterLevelGroupAnalogSensorConnected (groupName)) {
                    textBox.text = "--";
                    label.Visible = true;
                    label.text = "Disconnected";
                } else {
                    currentValue = WaterLevel.GetWaterLevelGroupLevel (groupName); ;
                    label.Visible = false;
                }
            }
        }
    }
}

