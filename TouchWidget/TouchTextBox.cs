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
    public class TouchTextBox : EventBox
    {
        public string name;
        public string text {
            get {
                return textRender.text;
            }
            set {
                textRender.text = value;
            }
        }

        public TouchColor textColor {
            get {
                return textRender.font.color;
            }
            set {
                textRender.font.color = value;
            }
        }

        public int textSize {
            get {
                return textRender.font.size;
            }
            set {
                textRender.font.size = value;
            }
        }

        public TouchAlignment textAlignment {
            get {
                return textRender.alignment;
            }
            set {
                textRender.alignment = value;
            }
        }
        public TouchColor backgroundColor;
        public bool enableTouch;
        public bool includeTimeFunctions;
        public event TextSetEventHandler TextChangedEvent;
        public TouchText textRender;

        public TouchTextBox () {
            Visible = true;
            VisibleWindow = false;

            SetSizeRequest (100, 30);

            textRender = new TouchText ();
            textRender.textWrap = TouchTextWrap.Shrink;

            text = string.Empty;
            name = string.Empty;
            textColor = new TouchColor ("black");
            textAlignment = TouchAlignment.Left;

            backgroundColor = "grey4";

            enableTouch = false;
            includeTimeFunctions = false;

            ExposeEvent += OnExpose;
            ButtonReleaseEvent += OnTouchButtonRelease;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;

                TouchGlobal.DrawRoundedRectangle (cr, left + 4, top + 4, width - 2, height - 2, 3);
                var shadowColor = new TouchColor (backgroundColor);
                shadowColor.ModifyColor (0.5);
                shadowColor.A = 0.4;
                shadowColor.SetSource (cr);
                cr.Fill ();

                TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, 3);
                backgroundColor.SetSource (cr);
                cr.FillPreserve ();

                TouchColor.SetSource (cr, "grey0");
                cr.LineWidth = 1;
                cr.Stroke ();

                textRender.Render (this, left + 3, top, width - 6, height);
            }
        }

        protected void OnTouchButtonRelease (object o, ButtonReleaseEventArgs args) {
            if (enableTouch) {
                var parent = Toplevel as Window;
                var t = new TouchNumberInput (includeTimeFunctions, parent);

                if (!string.IsNullOrWhiteSpace (name))
                    t.Title = name;

                t.TextSetEvent += (sender, a) => {
                    TextChangedEvent?.Invoke (this, a);

                    if (a.keepText) {
                        text = a.text;
                    }
                };

                t.Run ();
                t.Destroy ();
                QueueDraw ();
            }
        }
    }
}

