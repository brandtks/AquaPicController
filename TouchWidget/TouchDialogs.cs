#region License

/*
    Touch Widgets

    Copyright (c) 2017 Goodtime Development

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
using Cairo;
using Gtk;

namespace GoodtimeDevelopment.TouchWidget
{
    public class MessageBox
    {
        public static void Show (string msg) {
            var ms = new Dialog (string.Empty, null, DialogFlags.DestroyWithParent);
            ms.KeepAbove = true;

            #if RPI_BUILD
            ms.Decorated = false;

            ms.ExposeEvent += (o, args) => {
                using (Context cr = Gdk.CairoHelper.Create (ms.GdkWindow)) {
                    cr.MoveTo (ms.Allocation.Left, ms.Allocation.Top);
                    cr.LineTo (ms.Allocation.Right, ms.Allocation.Top);
                    cr.LineTo (ms.Allocation.Right, ms.Allocation.Bottom);
                    cr.LineTo (ms.Allocation.Left, ms.Allocation.Bottom);
                    cr.ClosePath ();
                    cr.LineWidth = 1.8;
                    TouchColor.SetSource (cr, "grey4");
                    cr.Stroke ();
                }
            };
            #endif

            ms.ModifyBg (StateType.Normal, TouchColor.NewGtkColor ("grey0"));

            var btn = new TouchButton ();
            btn.text = "Ok";
            btn.HeightRequest = 30;
            btn.ButtonReleaseEvent += (o, args) =>
                ms.Respond (ResponseType.Ok);
            ms.ActionArea.Add (btn);

            var label = new Label ();
            label.LineWrap = true;
            label.Text = msg;
            label.ModifyFg (StateType.Normal, TouchColor.NewGtkColor ("white"));
            label.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            ms.VBox.Add (label);
            label.Show ();

            ms.Run ();
            ms.Destroy ();
            ms.Dispose ();
        }
    }

    public class TouchDialog : Gtk.Dialog
    {
        public TouchDialog (string msg, Gtk.Window parent) 
            : base (string.Empty, parent, DialogFlags.DestroyWithParent)
        {
            this.ModifyBg (StateType.Normal, TouchColor.NewGtkColor ("grey0"));

            this.KeepAbove = true;

            #if RPI_BUILD
            Decorated = false;

            ExposeEvent += (o, args) => {
                using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                    cr.MoveTo (Allocation.Left, Allocation.Top);
                    cr.LineTo (Allocation.Right, Allocation.Top);
                    cr.LineTo (Allocation.Right, Allocation.Bottom);
                    cr.LineTo (Allocation.Left, Allocation.Bottom);
                    cr.ClosePath ();
                    cr.LineWidth = 1.8;
                    TouchColor.SetSource (cr, "grey4");
                    cr.Stroke ();
                }
            };
            #endif

            var btn = new TouchButton ();
            btn.text = "Yes";
            btn.HeightRequest = 30;
            btn.ButtonReleaseEvent += (o, args) => 
                this.Respond (ResponseType.Yes);
            this.ActionArea.Add (btn);

            btn = new TouchButton ();
            btn.text = "No";
            btn.HeightRequest = 30;
            btn.ButtonReleaseEvent += (o, args) => 
                this.Respond (ResponseType.No);
            this.ActionArea.Add (btn);

            var label = new Label ();
            label.LineWrap = true;
            label.Text = msg;
            label.ModifyFg (StateType.Normal, TouchColor.NewGtkColor ("white"));
            label.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            this.VBox.Add (label);
            label.Show ();
        }

        public override void Destroy () {
            base.Destroy ();
            Dispose ();
        }
    }
}

