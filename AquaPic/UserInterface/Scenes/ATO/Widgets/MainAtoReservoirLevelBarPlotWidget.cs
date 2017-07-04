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
using System.Linq;
using Gtk;
using AquaPic.Modules;
using AquaPic.Runtime;
using AquaPic.Utilites;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class AtoReservoirLevelWidget : BarPlotWidget
    {
        private TouchLabel label;

        public AtoReservoirLevelWidget ()
            : base () {
            text = "ATO Reservoir Level";
            unitOfMeasurement = TouchWidgetLibrary.UnitsOfMeasurement.Inches;

            label = new TouchLabel ();
            label.textColor = "compl";
            label.text = "Disconnected";
            label.textRender.orientation = TouchOrientation.Vertical;
            label.WidthRequest = 100;
            Put (label, 60, 9);
            label.Show ();

            var eventbox = new EventBox ();
            eventbox.VisibleWindow = false;
            eventbox.SetSizeRequest (WidthRequest, HeightRequest);
            eventbox.ButtonReleaseEvent += (o, args) => {
                var topWidget = this.Toplevel;
                AquaPicGui.AquaPicUserInterface.ChangeScreens ("Water Level", topWidget, AquaPicGui.AquaPicUserInterface.currentScene);
            };
            Put (eventbox, 0, 0);
            eventbox.Show ();

            fullScale = 15.0f;

            OnUpdate ();
        }

        public override void OnUpdate () {
            if (WaterLevel.atoReservoirLevelEnabled) {
                if (WaterLevel.atoReservoirLevel < 0.0f) {
                    textBox.text = "--";
                    label.Visible = true;
                    label.text = "Disconnected";
                } else {
                    currentValue = WaterLevel.atoReservoirLevel;
                    label.Visible = false;
                }
            } else {
                textBox.text = "--";
                label.Visible = true;
                label.text = "Disabled";
            }
        }
    }
}