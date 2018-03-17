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
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class ButtonWidget : TouchButton
    {
        public ButtonWidget (string name) : base () {
            SetSizeRequest (100, 82);

            text = name;
            bool s1 = Bit.Check (text);
            if (s1)
                buttonColor = "pri";
            else
                buttonColor = "seca";
            
            ButtonReleaseEvent += OnButtonRelease;
        }

        public virtual void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            TouchButton b = sender as TouchButton;
            string stateText = b.text;
            bool s = Bit.Check (stateText);
            if (s) {
                Bit.Reset (stateText);
                b.buttonColor = "seca";
            } else {
                Bit.Set (stateText);
                b.buttonColor = "pri";
            }
        }
    }
}

