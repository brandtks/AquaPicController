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
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    public class TemperatureLinePlot : LinePlotWidget
    {
        TouchLabel label;

        public TemperatureLinePlot (string group, int row, int column) : base ("Temperature", group, row, column) {
            text = "No Temperature";
            unitOfMeasurement = UnitsOfMeasurement.Degrees;

            label = new TouchLabel ();
            label.SetSizeRequest (152, 16);
            label.textColor = "compl";
            label.textAlignment = TouchAlignment.Right;
            label.textHorizontallyCentered = true;
            Put (label, 155, 63);

            this.group = group;
            if (!Temperature.CheckTemperatureGroupKeyNoThrow (this.group)) {
                this.group = Temperature.defaultTemperatureGroup;
            }

            if (this.group.IsNotEmpty ()) {
                var dataLogger = Temperature.GetTemperatureGroupDataLogger (this.group);
                linePlot.LinkDataLogger (dataLogger);

                Destroyed += (obj, args) => {
                    linePlot.UnLinkDataLogger (dataLogger);
                };

                text = string.Format ("{0} Temperature", this.group);

                WidgetReleaseEvent += (o, args) => {
                    AquaPicGui.AquaPicUserInterface.ChangeScreens ("Temperature", Toplevel, AquaPicGui.AquaPicUserInterface.currentScene, this.group);
                };
            }

            linePlot.eventColors.Add ("no probes", new TouchColor ("secb", 0.25));
            linePlot.eventColors.Add ("heater on", new TouchColor ("seca", 0.5));
            linePlot.eventColors.Add ("heater off", new TouchColor ("secc", 0.5));
            linePlot.eventColors.Add ("disconnected alarm", new TouchColor ("compl", 0.25));
            linePlot.eventColors.Add ("low alarm", new TouchColor ("compl", 0.5));
            linePlot.eventColors.Add ("high alarm", new TouchColor ("compl", 0.5));
        }

        public override void Update () {
            if (group.IsNotEmpty ()) {
                if (Temperature.AreTemperatureProbesConnected (group)) {
                    currentValue = Temperature.GetTemperatureGroupTemperature (group);
                } else {
                    textBox.text = "--";
                    label.Visible = true;
                    label.text = "Disconnected";
                }
            }
        }
    }
}

