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
using Cairo;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    public class DigitalDisplay : Fixed
    {
        public event ButtonReleaseEventHandler ForceButtonReleaseEvent;
        public event SelectorChangedEventHandler StateSelectedChangedEvent;

        public TouchLabel label;
        public TouchLabel textBox;
        public TouchButton button;
        public TouchSelectorSwitch selector;

        public DigitalDisplay () {
            SetSizeRequest (120, 140);

            label = new TouchLabel ();
            label.WidthRequest = 120;
            label.textAlignment = TouchAlignment.Center;
            label.textRender.textWrap = TouchTextWrap.Shrink;
            label.textColor = "grey3";
            Put (label, 0, 35);

            textBox = new TouchLabel ();
            textBox.text = "Open";
            textBox.textColor = "seca";
            textBox.textAlignment = TouchAlignment.Center;
            textBox.textSize = 20;
            textBox.SetSizeRequest (120, 30);
            Put (textBox, 0, 0);

            button = new TouchButton ();
            button.text = "Force";
            button.buttonColor = "grey3";
            button.ButtonReleaseEvent += OnForceReleased;
            button.SetSizeRequest (120, 30);
            Put (button, 0, 55);

            selector = new TouchSelectorSwitch (2);
            selector.SetSizeRequest (120, 30);
            selector.sliderSize = MySliderSize.Large;
            selector.textOptions[0] = "Open";
            selector.textOptions[1] = "Closed";
            selector.sliderColorOptions[0] = "grey2";
            selector.sliderColorOptions[1] = "pri";
            selector.SelectorChangedEvent += OnSelectorChange;
            selector.Visible = false;
            Put (selector, 0, 90);

            Show ();
        }

        protected void OnForceReleased (object sender, ButtonReleaseEventArgs args) {
            if (ForceButtonReleaseEvent != null)
                ForceButtonReleaseEvent (this, args);
            else
                throw new NotImplementedException ("Force button release not implemented");
        }

        protected void OnSelectorChange (object sender, SelectorChangedEventArgs args) {
            if (StateSelectedChangedEvent != null)
                StateSelectedChangedEvent (this, args);
            else
                throw new NotImplementedException ("State selector changed not implemented");
        }
    }
}

