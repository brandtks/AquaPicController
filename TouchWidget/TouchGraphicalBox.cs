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

namespace GoodtimeDevelopment.TouchWidget
{
    public class TouchGraphicalBox : EventBox
    {
        public TouchColor color;
        public double transparency {
            get {
                return color.A;
            }
            set {
                color.A = value;
            }
        }

        public TouchGraphicalBox (int width, int height) {
            Visible = true;
            VisibleWindow = false;

            SetSizeRequest (width, height);
            color = "grey2";
            transparency = 0.55f;

            ExposeEvent += OnExpose;
        }

        public TouchGraphicalBox ()
            : this (800, 480) { }

        protected virtual void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                cr.Rectangle (Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);
                color.SetSource (cr);
                cr.Fill ();
            }
        }
    }
}

