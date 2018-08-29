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
using AquaPic.Modules;
using Gtk;

namespace AquaPic.UserInterface
{
    public delegate float GetDimmingLevelHandler ();

    public class DimmingLightBarPlot : CurvedBarPlotWidget
    {
        public GetDimmingLevelHandler GetDimmingLevel;

        public DimmingLightBarPlot (string name, GetDimmingLevelHandler GetDimmingLevel) {
            text = name;
            unitOfMeasurement = GoodtimeDevelopment.TouchWidget.UnitsOfMeasurement.Percentage;
            this.GetDimmingLevel = GetDimmingLevel;

            var eventbox = new EventBox ();
            eventbox.VisibleWindow = false;
            eventbox.SetSizeRequest (WidthRequest, HeightRequest);
            eventbox.ButtonReleaseEvent += (o, args) => {
                var topWidget = this.Toplevel;
                AquaPicGui.AquaPicUserInterface.ChangeScreens ("Lighting", topWidget, AquaPicGui.AquaPicUserInterface.currentScene, name);
            };
            Put (eventbox, 0, 0);
            eventbox.Show ();
        }

        public override void OnUpdate () {
            currentValue = GetDimmingLevel ();
        }
    }
}

