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
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class HomeWidget : Fixed
    {
        public event ButtonReleaseEventHandler WidgetReleaseEvent;
        public EventBox touchArea;

        uint clickTimer;
        bool clicked, longHold, selected;

        public HomeWidget () {
            SetSizeRequest (100, 82);
        }

        protected override void OnShown () {
            if (touchArea == null) {
                touchArea = new EventBox ();
                touchArea.VisibleWindow = false;
                touchArea.SetSizeRequest (WidthRequest, HeightRequest);
                touchArea.ButtonPressEvent += OnTouchAreaButtonPress;
                touchArea.ButtonReleaseEvent += OnTouchAreaButtonRelease;
                touchArea.ExposeEvent += OnTouchAreaExpose;
                touchArea.DragBegin += (o, args) => {
                    Console.WriteLine ("Dragging object");
                };
                Put (touchArea, 0, 0);
                touchArea.Show ();
            }
            base.OnShown ();
        }

        public override void Dispose () {
            GLib.Source.Remove (clickTimer);
            base.Dispose ();
        }

        protected void OnTouchAreaExpose (object obj, ExposeEventArgs args) {
            if (selected) {
                using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                    cr.Rectangle (Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);
                    TouchColor.SetSource (cr, "grey0", 0.5);
                    cr.Fill ();
                }
            }
        }

        protected void OnTouchAreaButtonPress (object sender, ButtonPressEventArgs args) {
            longHold = false;
            selected = false;
            clicked = true;
            clickTimer = GLib.Timeout.Add (1000, OnTimerEvent);
        }

        protected void OnTouchAreaButtonRelease (object sender, ButtonReleaseEventArgs args) {
            clicked = false;
            if (!longHold) {
                if (WidgetReleaseEvent != null) {
                    WidgetReleaseEvent (sender, args);
                } else {
                    var x = args.Event.X;
                    var y = args.Event.Y;
                    for (int i = Children.Length - 1; i >= 0; --i) {
                        var child = Children[i];
                        if (child != touchArea) {
                            if (child.Visible) {
                                var left = child.Allocation.X - Allocation.X;
                                var right = left + child.Allocation.Width;
                                var top = child.Allocation.Y - Allocation.Y;
                                var bottom = top + child.Allocation.Height;
                                if (x > left && x < right && y > top && y < bottom) {
                                    child.ProcessEvent (args.Event);
                                    break;
                                }
                            }
                        }
                    }
                }
            } else {
                selected = false;
            }
            QueueDraw ();
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                longHold = true;
                selected = true;
                QueueDraw ();
            }
            return false;
        }
    }
}
