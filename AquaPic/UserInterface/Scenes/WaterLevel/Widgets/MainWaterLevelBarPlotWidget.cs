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
        int flashUpdate;

        public WaterLevelWidget (params object[] options)
            : base () 
        {
            text = "Water Level";
            unitOfMeasurement = UnitsOfMeasurement.Inches;

            label = new TouchLabel ();
            label.textColor = "compl";
            label.text = "Probe Disconnected";
            label.WidthRequest = 199;
            label.textAlignment = TouchAlignment.Center;
            Put (label, 3, 55);
            label.Show ();

            groupName = string.Empty;
            if (options.Length >= 1) {
                groupName = options[0] as string;
                if (groupName != null) {
                    if (!WaterLevel.CheckWaterLevelGroupKeyNoThrow (groupName)) {
                        groupName = Temperature.defaultTemperatureGroup;
                    }
                } else {
                    groupName = Temperature.defaultTemperatureGroup;
                }
            } else {
                groupName = Temperature.defaultTemperatureGroup;
            }

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
            flashUpdate = 0;
            OnUpdate ();
        }

        public override void OnUpdate () {
            bool usingLevel = false;
            if (groupName.IsNotEmpty ()) {
                var analogSensorName = WaterLevel.GetWaterLevelGroupAnalogSensorName (groupName);
                if (analogSensorName.IsNotEmpty ()) {
                    if (WaterLevel.GetAnalogLevelSensorEnable (analogSensorName)) {
                        usingLevel = true;
                        var level = WaterLevel.GetAnalogLevelSensorLevel (analogSensorName);
                        if (level < 0.0f) {
                            currentValue = 0.0f;

                            flashUpdate = ++flashUpdate % 4;
                            if (flashUpdate <= 1)
                                label.Visible = true;
                            else
                                label.Visible = false;
                        } else {
                            currentValue = level;
                            label.Visible = false;
                            flashUpdate = 0;
                        }
                    }
                }
            }

            if (!usingLevel) {
                label.text = "Probe Disabled";
                label.Visible = true;
            }
        }
    }
}    }

            if (!usingLevel) {
                label.text = "Probe Disabled";
                label.Visible = true;
            }
        }
    }
}