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
    public class HomeWidget : Fixed
    {
        public event ButtonReleaseEventHandler WidgetReleaseEvent;

        public EventBox touchArea;
        public string type;
        HomeWidgetPlacement placement;

        public Tuple<int, int>[] pairs {
            get {
                return placement.ToRowColumnPairs ();
            }
        }

        public int rowOrigin {
            get {
                return placement.rowOrigin;
            }
            set {
                placement.rowOrigin = value;
            }
        }

        public int columnOrigin {
            get {
                return placement.columnOrigin;
            }
            set {
                placement.columnOrigin = value;
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

            this.type = type;
            placement = new HomeWidgetPlacement (row, column);

            switch (type) {
            case "Timer": 
                placement.width = 3;
                placement.height = 2;
                break;
            case "LinePlot":
                placement.width = 3;
                placement.height = 1;
                break;
            case "BarPlot": 
                placement.width = 1;
                placement.height = 2;
                break;
            case "CurvedBarPlot": 
                placement.width = 2;
                placement.height = 2;
                break;
            case "Button":
                placement.width = 1;
                placement.height = 1;
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
            }
            selected = false;
            QueueDraw ();
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                if (!selected) {
                    ++holdCounter;
                    if (holdCounter > 50) {
                        selected = true;
                    }
                }
            }
            QueueDraw ();
            return clicked;
        }

        private class HomeWidgetPlacement
        {
            public int rowOrigin;
            public int columnOrigin;
            public int width;
            public int height;

            public HomeWidgetPlacement (int rowOrigin, int columnOrigin) : this (rowOrigin, columnOrigin, 1, 1) { }

            public HomeWidgetPlacement (int rowOrigin, int columnOrigin, int width, int height) {
                this.rowOrigin = rowOrigin;
                this.columnOrigin = columnOrigin;
                this.width = width;
                this.height = height;
            }

            public Tuple<int, int>[] ToRowColumnPairs () {
                var pairs = new Tuple<int, int>[width * height];
                for (int i = 0; i < pairs.Length; ++i) {
                    int row;
                    if (height != 1) {
                        row = rowOrigin + (i / (pairs.Length / height));
                    } else {
                        row = rowOrigin;
                    }
                    var column = columnOrigin + (i % width);
                    pairs[i] = new Tuple<int, int> (row, column);
                }
                return pairs;
            }
        }
    }
}
