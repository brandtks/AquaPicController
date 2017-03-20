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
    public delegate void TextChangedHandler (object sender, TextChangedEventArgs args);

    public class TextChangedEventArgs : EventArgs {
        public string text;
        public bool keepText;

        public TextChangedEventArgs (string text) {
            this.text = text;
            keepText = true;
        }
    }

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
        public event TextChangedHandler TextChangedEvent;
        public TouchText textRender;

        public TouchTextBox () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.WidthRequest = 100;
            this.HeightRequest = 30;

            textRender = new TouchText ();
            textRender.textWrap = TouchTextWrap.Shrink;

            this.text = string.Empty;
            name = string.Empty;
            this.textColor = new TouchColor ("black");
            this.textAlignment = TouchAlignment.Left;

            bkgndColor = "grey4";

            enableTouch = false;
            includeTimeFunctions = false;

            this.ExposeEvent += OnExpose;
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

                var parent = this.Toplevel as Gtk.Window;
                if (parent != null) {
                    if (parent.IsTopLevel)
                        t = new TouchNumberInput (includeTimeFunctions, parent);
                    else
                        t = new TouchNumberInput (includeTimeFunctions);
                } else
                    t = new TouchNumberInput (includeTimeFunctions);

                if (!string.IsNullOrWhiteSpace (name))
                    t.Title = name;
                
                t.NumberSetEvent += (value) => {
                    if (!string.IsNullOrWhiteSpace (value)) {
                        TextChangedEventArgs a = new TextChangedEventArgs (value);

                        if (TextChangedEvent != null)
                            TextChangedEvent (this, a);

                        if (a.keepText) 
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

