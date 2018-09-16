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
    public enum ButtonClickAction : byte
    {
        None = 0,
        NoTransparency,
        Brighten,
        Darken
    }

    public class TouchButton : EventBox
    {
        public TouchText render;
        public TouchColor buttonColor;
        private TouchColor unmodifiedColor;
        public ButtonClickAction clickAction;

        public string text {
            get {
                return render.text;
            }
            set {
                render.text = value;
            }
        }

        public TouchColor textColor {
            get {
                return render.font.color;
            }
            set {
                render.font.color = value;
            }
        }

        public int textSize {
            get {
                return render.font.size;
            }
            set {
                render.font.size = value;
            }
        }

        public TouchAlignment textAlignment {
            get {
                return render.alignment;
            }
            set {
                render.alignment = value;
            }
        }

        public TouchButton () {
            Visible = true;
            VisibleWindow = false;

            render = new TouchText ();
            buttonColor = "pri";
            text = "";
            textColor = "black";
            HeightRequest = 45;
            WidthRequest = 45;
            textAlignment = TouchAlignment.Center;
            clickAction = ButtonClickAction.Darken;

            ExposeEvent += OnExpose;
            ButtonPressEvent += OnTouchButtonPress;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                var left = Allocation.Left;
                var width = Allocation.Width;
                var top = Allocation.Top;
                var bottom = Allocation.Bottom;
                var height = Allocation.Height;

                var outlineColor = new TouchColor (buttonColor);
                outlineColor.ModifyColor (0.5);
                var highlightColor = new TouchColor (buttonColor);
                highlightColor.ModifyColor (1.4);

                TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, 4.0);
                outlineColor.SetSource (cr);
                cr.StrokePreserve ();

                using (var grad = new LinearGradient (left, top, left, bottom)) {
                    grad.AddColorStop (0, highlightColor.ToCairoColor ());
                    grad.AddColorStop (0.2, buttonColor.ToCairoColor ());
                    cr.SetSource (grad);
                    cr.Fill ();
                }

                render.Render (this, left + 3, top, width - 6, height);
            }
        }

        protected void OnTouchButtonPress (object o, ButtonPressEventArgs args) {
            if (args.Event.Type == Gdk.EventType.ButtonPress) {
                if (clickAction == ButtonClickAction.NoTransparency) {
                    unmodifiedColor = new TouchColor (buttonColor);
                    buttonColor.ModifyAlpha (1.0);
                } else if (clickAction == ButtonClickAction.Brighten) {
                    unmodifiedColor = new TouchColor (buttonColor);
                    buttonColor.ModifyColor (1.25);
                } else if (clickAction == ButtonClickAction.Darken) {
                    unmodifiedColor = new TouchColor (buttonColor);
                    buttonColor.ModifyColor (0.75);
                }

                QueueDraw ();
            }
        }

        protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt) {
            buttonColor = unmodifiedColor;
            QueueDraw ();

            if ((evnt.X < 0) || (evnt.X > Allocation.Width)) {
                return true;
            }

            if ((evnt.Y < 0) || (evnt.Y > Allocation.Height)) {
                return true;
            }

            return base.OnButtonReleaseEvent (evnt);
        }
    }
}

