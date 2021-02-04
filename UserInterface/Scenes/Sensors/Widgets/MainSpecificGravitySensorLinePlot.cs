// #region License
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
using AquaPic.Gadgets.Sensor;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    public class SpecificGravitySensorLinePlot : LinePlotWidget
    {
        TouchLabel label;

        public SpecificGravitySensorLinePlot (string group, int row, int column) : base ("SG Sensor", group, row, column) {
            text = "No SG Sensor";
            unitOfMeasurement = UnitsOfMeasurement.None;
            linePlot.rangeMargin = 0.2;

            label = new TouchLabel ();
            label.SetSizeRequest (152, 16);
            label.textColor = "compl";
            label.textAlignment = TouchAlignment.Right;
            label.textHorizontallyCentered = true;
            Put (label, 155, 63);

            this.group = group;
            if (Sensors.SpecificGravitySensors.CheckGadgetKeyNoThrow (this.group)) {
                var dataLogger = Sensors.SpecificGravitySensors.GetDataLogger (this.group);
                linePlot.LinkDataLogger (dataLogger);

                Destroyed += (obj, args) => {
                    linePlot.UnLinkDataLogger (dataLogger);
                };

                text = string.Format ("{0} SG", this.group);

                WidgetReleaseEvent += (o, args) => {
                    AquaPicGui.AquaPicUserInterface.ChangeScreens ("Sensors", Toplevel, AquaPicGui.AquaPicUserInterface.currentScene, this.group);
                };
            } else {
                this.group = string.Empty;
            }

            linePlot.rangeMargin = 1;
            linePlot.eventColors.Add ("probe disconnected", new TouchColor ("secb", 0.25));
            linePlot.eventColors.Add ("disconnected alarm", new TouchColor ("compl", 0.25));
        }

        public override void Update () {
            if (group.IsNotEmpty ()) {
                if (!Sensors.PhProbes.GetAnalogSensorConnected (group)) {
                    textBox.text = "--";
                    label.Visible = true;
                    label.text = "Disconnected";
                } else {
                    currentValue = (float)Sensors.PhProbes.GetGadgeValue (group);
                    label.Visible = false;
                }
            }
        }
    }
}
