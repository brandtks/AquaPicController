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
using Cairo;
using Gtk;
using GoodtimeDevelopment.TouchWidget;
using AquaPic.Modules;
using AquaPic.Runtime;


namespace AquaPic.UserInterface
{
    public class LightingStateDisplay : EventBox
    {
        bool clicked, expanded;
        uint clickTimer;
        int clickX, clickY;

        public LightingStateDisplay () {
            Visible = true;
            VisibleWindow = false;
            SetSizeRequest (800, 19);

            ExposeEvent += onExpose;
            //ButtonPressEvent += OnButtonPress;
            //ButtonReleaseEvent += OnButtonRelease;
        }

        public override void Dispose () {
            if (clickTimer != 0) {
                GLib.Source.Remove (clickTimer);
            }
            base.Dispose ();
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                
            }
        }

        protected void OnButtonPress (object o, ButtonPressEventArgs args) {
            if (expanded) {
                clicked = true;
                GetPointer (out clickX, out clickY);
                clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
            }
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            clicked = false;

        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);

                double yDelta = clickY - y;
                double xDelta = clickX - x;

                QueueDraw ();
            }

            return clicked;
        }
    }
}
