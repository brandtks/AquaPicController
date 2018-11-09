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
using System.Collections.Generic;
using Gtk;
using Cairo;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    public class LightingStateWidget : Fixed
    {
        public LightingStateDisplay lightingStateDisplay;
        TouchSelectorSwitch adjustDimmingTogetherSelector;

        public LightingStateWidget () {
            SetSizeRequest (540, 360);

            lightingStateDisplay = new LightingStateDisplay ();
            lightingStateDisplay.SetSizeRequest (540, 360);
            lightingStateDisplay.LightingStateSelectionChanged += (obj, args) => {
                if (args.stateSelected) {
                    adjustDimmingTogetherSelector.Visible = true;
                } else {
                    adjustDimmingTogetherSelector.Visible = false;
                }
            };
            Put (lightingStateDisplay, 0, 0);
            lightingStateDisplay.Show ();

            adjustDimmingTogetherSelector = new TouchSelectorSwitch ();
            adjustDimmingTogetherSelector.SetSizeRequest (140, 30);
            adjustDimmingTogetherSelector.sliderSize = MySliderSize.Large;
            adjustDimmingTogetherSelector.textOptions[0] = "Separate";
            adjustDimmingTogetherSelector.textOptions[1] = "Together";
            adjustDimmingTogetherSelector.sliderColorOptions[0] = "grey2";
            adjustDimmingTogetherSelector.sliderColorOptions[1] = "pri";
            adjustDimmingTogetherSelector.currentSelected = 1;
            adjustDimmingTogetherSelector.SelectorChangedEvent += (obj, args) => {
                if (args.currentSelectedIndex == 1) {
                    lightingStateDisplay.adjustDimmingTogether = true;
                } else {
                    lightingStateDisplay.adjustDimmingTogether = false;
                }
            };
            Put (adjustDimmingTogetherSelector, 10, 10);

            Show ();
        }

        public void SetStates (LightingState[] lightingStates, bool dimmingFixture) {
            lightingStateDisplay.SetStates (lightingStates, dimmingFixture);
        }
    }
}
