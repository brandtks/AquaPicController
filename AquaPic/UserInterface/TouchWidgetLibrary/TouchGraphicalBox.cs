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
using Cairo;

namespace TouchWidgetLibrary
{
    public class TouchGraphicalBox : EventBox
    {
        public string color;
        public float transparency;

        public TouchGraphicalBox (int width, int height) {
            Visible = true;
            VisibleWindow = false;

            WidthRequest = width;
            HeightRequest = height;
            this.color = "grey2";
            transparency = 0.55f;

            ExposeEvent += OnExpose;
        }

        protected virtual void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                cr.Rectangle (Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);
                TouchColor.SetSource (cr, color, transparency);
                cr.Fill ();
            }
        }
    }
}

