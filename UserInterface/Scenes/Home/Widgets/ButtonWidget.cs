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
using AquaPic.Service;

namespace AquaPic.UserInterface
{
    public class ButtonWidget : HomeWidget, IHomeWidgetUpdatable
    {
        public TouchButton button;

        public ButtonWidget (string name, int row, int column) : base ("Button", name, row, column) {
            button = new TouchButton ();
            button.SetSizeRequest (100, 82);
            button.text = name;
            button.buttonColor = Bit.Check (button.text) ? "pri" : "seca";
            button.ButtonReleaseEvent += OnButtonRelease;
            Put (button, 0, 0);
            button.Show ();
        }

        public virtual void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            var b = sender as TouchButton;
            var stateText = b.text;
            if (Bit.Check (stateText)) {
                Bit.Reset (stateText);
                b.buttonColor = "seca";
            } else {
                Bit.Set (stateText);
                b.buttonColor = "pri";
            }
            b.QueueDraw ();
        }

        public void Update () {
            button.buttonColor = Bit.Check (button.text) ? "pri" : "seca";
        }
    }
}

