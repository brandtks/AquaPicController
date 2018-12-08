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
    public delegate void RequestNewTileLocationHandler (int x, int y);
    public delegate void WidgetSelectedHandler (HomeWidget widget);
    public delegate void WidgetUnselectedHandler (HomeWidget widget);

    public class HomeWidget : Fixed
    {
        public event ButtonReleaseEventHandler WidgetReleaseEvent;
        public event RequestNewTileLocationHandler RequestNewTileLocationEvent;
        public event WidgetSelectedHandler WidgetSelectedEvent;
        public event WidgetUnselectedHandler WidgetUnselectedEvent;

        public EventBox touchArea;
        HomeWidgetPlacement placement;

        public Tuple<int, int>[] pairs {
            get {
                return placement.ToRowColumnPairs ();
            }
        }

        public int row {
            get {
                return placement.row;
            }
            set {
                placement.row = value;
            }
        }

        public int column {
            get {
                return placement.column;
            }
            set {
                placement.column = value;
            }
        }

        public int columnWidth {
            get {
                return placement.columnWidth;
            }
        }

        public int rowHeight {
            get {
                return placement.rowHeight;
            }
        }

        public int x {
            get {
                return placement.x;
            }
        }

        public int y {
            get {
                return placement.y;
            }
        }

        public int width {
            get {
                return placement.width;
            }
        }

        public int height {
            get {
                return placement.height;
            }
        }

        uint clickTimer;
        bool clicked, selected;
        int holdCounter;

        public HomeWidget (string type, int row, int column) {
            SetSizeRequest (100, 82);

            placement = new HomeWidgetPlacement (row, column);

            switch (type) {
            case "Timer": 
                placement.columnWidth = 3;
                placement.rowHeight = 2;
                break;
            case "LinePlot":
                placement.columnWidth = 3;
                placement.rowHeight = 1;
                break;
            case "BarPlot": 
                placement.columnWidth = 1;
                placement.rowHeight = 2;
                break;
            case "CurvedBarPlot": 
                placement.columnWidth = 2;
                placement.rowHeight = 2;
                break;
            case "Button":
                placement.columnWidth = 1;
                placement.rowHeight = 1;
                break;
            default:
                throw new Exception (string.Format ("Unknown home widget type {0}", type));
            }
        }

        protected override void OnShown () {
            if (touchArea == null) {
                touchArea = new EventBox ();
                touchArea.VisibleWindow = false;
                touchArea.SetSizeRequest (WidthRequest, HeightRequest);
                touchArea.ButtonPressEvent += OnTouchAreaButtonPress;
                touchArea.ButtonReleaseEvent += OnTouchAreaButtonRelease;
                touchArea.ExposeEvent += OnTouchAreaExpose;
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
            holdCounter = 0;
            selected = false;
            clicked = true;
            clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
        }

        protected void OnTouchAreaButtonRelease (object sender, ButtonReleaseEventArgs args) {
            clicked = false;
            if (!selected) {
                if (WidgetReleaseEvent != null) {
                    WidgetReleaseEvent (sender, args);
                } else {
                    var releaseX = args.Event.X;
                    var releaseY = args.Event.Y;
                    for (int i = Children.Length - 1; i >= 0; --i) {
                        var child = Children[i];
                        if (child != touchArea && child.Visible) {
                            var left = child.Allocation.X - Allocation.X;
                            var right = left + child.Allocation.Width;
                            var top = child.Allocation.Y - Allocation.Y;
                            var bottom = top + child.Allocation.Height;
                            if (releaseX > left && releaseX < right && releaseY > top && releaseY < bottom) {
                                child.ProcessEvent (args.Event);
                                break;
                            }
                        }
                    }
                }
            }
            selected = false;
            WidgetUnselectedEvent?.Invoke (this);
            QueueDraw ();
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                if (!selected) {
                    ++holdCounter;
                    if (holdCounter > 50) {
                        selected = true;
                        WidgetSelectedEvent?.Invoke (this);
                    }
                }

                if (selected) {
                    GetPointer (out int currentX, out int currentY);
                    RequestNewTileLocationEvent?.Invoke (currentX + Allocation.Left, currentY + Allocation.Top);
                }
            }

            QueueDraw ();
            return clicked;
        }
    }
}
