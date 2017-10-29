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
using Gtk;
using GoodtimeDevelopment.TouchWidget;
using AquaPic.Operands;

namespace AquaPic.UserInterface
{
//    public delegate ButtonWidget CreateButtonHandler ();
//
//    public class ButtonData {
//        public CreateButtonHandler CreateInstanceEvent;
//
//        public ButtonData (CreateButtonHandler CreateInstanceEvent) {
//            this.CreateInstanceEvent = CreateInstanceEvent;
//        }
//
//        public ButtonWidget CreateInstance () {
//            if (CreateInstanceEvent != null)
//                return CreateInstanceEvent ();
//            else
//                throw new Exception ("No bar plot constructor implemented");
//        }
//    }

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

