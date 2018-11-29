#region License

/*
 AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

 Copyright (c) 2018 Goodtime Development

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
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    public class LightingStateWidget : Fixed
    {
        LightingStateDisplay lightingStateDisplay;
        TouchSelectorSwitch adjustDimmingTogetherSelector;
        TouchLabel adjustDimmingTogetherLabel;
        TouchButton acceptChangesButton;
        TouchButton undoChangesButton;
        TouchButton deleteStateButton;
        string fixtureName;

        public LightingStateWidget () {
            SetSizeRequest (540, 380);

            lightingStateDisplay = new LightingStateDisplay ();
            lightingStateDisplay.SetSizeRequest (540, 380);
            lightingStateDisplay.LightingStateSelectionChanged += (obj, args) => {
                if (args.stateSelected) {
                    adjustDimmingTogetherSelector.Visible = true;
                    adjustDimmingTogetherLabel.Visible = true;
                    deleteStateButton.Visible = true;
                } else {
                    adjustDimmingTogetherSelector.Visible = false;
                    adjustDimmingTogetherLabel.Visible = false;
                    deleteStateButton.Visible = false;
                }
            };
            lightingStateDisplay.LightingStateInfoChanged += (obj, hasChanged) => {
                if (hasChanged) {
                    acceptChangesButton.buttonColor = "pri";
                    undoChangesButton.buttonColor = "pri";
                } else {
                    acceptChangesButton.buttonColor = "grey1";
                    undoChangesButton.buttonColor = "grey1";
                }
                acceptChangesButton.QueueDraw ();
                undoChangesButton.QueueDraw ();
            };
            Put (lightingStateDisplay, 0, 0);
            lightingStateDisplay.Show ();

            adjustDimmingTogetherSelector = new TouchSelectorSwitch ();
            adjustDimmingTogetherSelector.SetSizeRequest (180, 30);
            adjustDimmingTogetherSelector.sliderSize = MySliderSize.Large;
            adjustDimmingTogetherSelector.textOptions[0] = "Separate";
            adjustDimmingTogetherSelector.textOptions[1] = "Together";
            adjustDimmingTogetherSelector.sliderColorOptions[1] = "pri";
            adjustDimmingTogetherSelector.selectedTextColorOptions[1] = "black";
            adjustDimmingTogetherSelector.currentSelected = 1;
            adjustDimmingTogetherSelector.SelectorChangedEvent += (obj, args) => {
                lightingStateDisplay.adjustDimmingTogether = args.currentSelectedIndex == 1;
            };
            Put (adjustDimmingTogetherSelector, 0, 350);

            adjustDimmingTogetherLabel = new TouchLabel ();
            adjustDimmingTogetherLabel.text = "Adjust Dimming";
            adjustDimmingTogetherLabel.WidthRequest = 180;
            adjustDimmingTogetherLabel.textAlignment = TouchAlignment.Center;
            Put (adjustDimmingTogetherLabel, 0, 328);

            acceptChangesButton = new TouchButton ();
            acceptChangesButton.text = "Accept";
            acceptChangesButton.buttonColor = "grey1";
            acceptChangesButton.SetSizeRequest (80, 40);
            acceptChangesButton.ButtonReleaseEvent += (obj, args) => {
                if (lightingStateDisplay.hasStateInfoChanged) {
                    var parent = Toplevel as Window;
                    var ms = new TouchDialog ("Do you want to make the changes permanent", parent, new string[] { "Yes", "No", "Cancel" });
                    ms.Response += (o, a) => {
                        if (a.ResponseId != ResponseType.Cancel) {
                            var makeChangePermanant = a.ResponseId == ResponseType.Yes;

                            Lighting.SetLightingFixtureLightingStates (
                                fixtureName,
                                lightingStateDisplay.lightingStates,
                                !makeChangePermanant);

                            if (makeChangePermanant) {
                                Lighting.UpdateFixtureSettingsToFile (fixtureName);
                            }

                            lightingStateDisplay.selectedState = -1;
                            lightingStateDisplay.hasStateInfoChanged = false;
                            lightingStateDisplay.QueueDraw ();
                        }
                    };

                    ms.Run ();
                    ms.Destroy ();
                }
            };
            Put (acceptChangesButton, 460, 340);
            acceptChangesButton.Show ();

            undoChangesButton = new TouchButton ();
            undoChangesButton.text = "Undo";
            undoChangesButton.buttonColor = "grey1";
            undoChangesButton.SetSizeRequest (80, 40);
            undoChangesButton.ButtonReleaseEvent += (obj, args) => {
                if (lightingStateDisplay.hasStateInfoChanged) {
                    SetStates (fixtureName);
                }
            };
            Put (undoChangesButton, 370, 340);
            undoChangesButton.Show ();

            deleteStateButton = new TouchButton ();
            deleteStateButton.text = "Delete";
            deleteStateButton.SetSizeRequest (80, 40);
            deleteStateButton.buttonColor = "compl";
            deleteStateButton.textColor = "white";
            deleteStateButton.ButtonReleaseEvent += (obj, args) => {
                var parent = Toplevel as Window;
                var ms = new TouchDialog ("Are you sure you want to delete the state", parent);
                ms.Response += (o, a) => {
                    if (a.ResponseId == ResponseType.Yes) {
                        lightingStateDisplay.RemoveSelectedState ();
                    }
                };

                ms.Run ();
                ms.Destroy ();
            };
            Put (deleteStateButton, 280, 340);

            Show ();
        }

        public void SetStates (string fixtureName) {
            this.fixtureName = fixtureName;
            lightingStateDisplay.SetStates (
                Lighting.GetLightingFixtureLightingStates (fixtureName),
                Lighting.IsDimmingFixture (fixtureName));
        }
    }
}
