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
    public class DimmingLightCurvedBarPlot : CurvedBarPlotWidget
    {
        string fixtureName;

        public DimmingLightCurvedBarPlot (params object[] options) {
            text = "Lighting";
            unitOfMeasurement = UnitsOfMeasurement.Percentage;

            fixtureName = string.Empty;
            if (options.Length >= 1) {
                var fixtureNameOption = options[0] as string;
                if (fixtureNameOption != null) {
                    if (Lighting.CheckFixtureKeyNoThrow (fixtureNameOption) &&
                        Lighting.IsDimmingFixture (fixtureNameOption)) {
                        fixtureName = fixtureNameOption;
                        text = fixtureName;
                        WidgetReleaseEvent += (o, args) => {
                            AquaPicGui.AquaPicUserInterface.ChangeScreens ("Lighting", Toplevel, AquaPicGui.AquaPicUserInterface.currentScene, fixtureName);
                        };
                    }
                }
            }
        }

        public override void OnUpdate () {
            if (fixtureName.IsNotEmpty ()) {
                currentValue = Lighting.GetCurrentDimmingLevel (fixtureName);
            }
        }
    }
}

