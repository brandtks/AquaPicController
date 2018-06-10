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
        public TouchColor bkgndColor;
        public bool enableTouch;
        public bool includeTimeFunctions;
        public event TextSetEventHandler TextChangedEvent;
        public TouchText textRender;

        public TouchTextBox () {
            Visible = true;
            VisibleWindow = false;

            WidthRequest = 100;
            HeightRequest = 30;

            textRender = new TouchText ();
            textRender.textWrap = TouchTextWrap.Shrink;

            text = string.Empty;
            name = string.Empty;
            textColor = new TouchColor ("black");
            textAlignment = TouchAlignment.Left;

            bkgndColor = "grey4";

            enableTouch = false;
            includeTimeFunctions = false;

            ExposeEvent += OnExpose;
            ButtonReleaseEvent += OnTouchButtonRelease;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;

                TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, 3);
                bkgndColor.SetSource (cr);
                cr.FillPreserve ();
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.LineWidth = 0.75;
                cr.Stroke ();

                textRender.Render (this, left + 3, top, width - 6, height);
            }
        }

        protected void OnTouchButtonRelease (object o, ButtonReleaseEventArgs args) {
            if (enableTouch) {
                TouchNumberInput t;

                var parent = Toplevel as Window;
                if (parent != null) {
                    if (parent.IsTopLevel)
                        t = new TouchNumberInput (includeTimeFunctions, parent);
                    else
                        t = new TouchNumberInput (includeTimeFunctions);
                } else
                    t = new TouchNumberInput (includeTimeFunctions);

                if (!string.IsNullOrWhiteSpace (name))
                    t.Title = name;

                t.TextSetEvent += (sender, a) => {
                    TextChangedEvent?.Invoke (this, a);

                    if (a.keepText)
                        text = a.text;
                };

                t.Run ();
                t.Destroy ();
                QueueDraw ();
            }
        }
    }
}

