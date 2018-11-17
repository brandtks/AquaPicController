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
using GoodtimeDevelopment.Utilites;

namespace GoodtimeDevelopment.TouchWidget
{
    public delegate void ProgressChangeEventHandler (object sender, ProgressChangeEventArgs args);

    public class ProgressChangeEventArgs : EventArgs
    {
        public float currentProgress;

        public ProgressChangeEventArgs (float currentProgress) {
            this.currentProgress = currentProgress;
        }
    }

    public class TouchProgressBar : EventBox
    {
        float _currentProgress;
        public float currentProgress {
            get {
                return _currentProgress;
            }
            set {
                _currentProgress = value;
                _currentProgress.Constrain (0, 1);
            }
        }

        public TouchColor colorBackground;
        public TouchColor colorProgress;
        public bool enableTouch;

        public event ProgressChangeEventHandler ProgressChangedEvent;
        public event ProgressChangeEventHandler ProgressChangingEvent;

        uint timerId;
        bool clicked;
        protected TouchOrientation orientation;
        protected int radius;

        public TouchProgressBar (
            TouchColor colorBackground,
            TouchColor colorProgress,
            float currentProgress,
            bool enableTouch,
            TouchOrientation orientation
        ) {
            Visible = true;
            VisibleWindow = false;

            this.colorBackground = colorBackground;
            this.colorProgress = colorProgress;
            _currentProgress = currentProgress;
            this.enableTouch = enableTouch;
            this.orientation = orientation;
            timerId = 0;
            clicked = false;

            if (this.orientation == TouchOrientation.Vertical) {
                SetSizeRequest (30, 200);
            } else {
                SetSizeRequest (200, 30);
            }

            ExposeEvent += OnExpose;
            ButtonPressEvent += OnProgressBarPress;
            ButtonReleaseEvent += OnProgressBarRelease;
        }

        public override void Dispose () {
            if (timerId != 0) {
                GLib.Source.Remove (timerId);
            }
            base.Dispose ();
        }

        public TouchProgressBar (TouchOrientation orientation)
            : this (new TouchColor ("grey4"), new TouchColor ("pri"), 0f, false, orientation) {
        }

        public TouchProgressBar ()
            : this (new TouchColor ("grey4"), new TouchColor ("pri"), 0f, false, TouchOrientation.Vertical) {
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;

                radius = orientation == TouchOrientation.Vertical ? width / 2 : height / 2;

                TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, radius);
                colorBackground.SetSource (cr);
                cr.Fill ();

                DrawProgressBar (cr, _currentProgress, colorProgress);
            }
        }

        protected void OnProgressBarPress (object o, ButtonPressEventArgs args) {
            if (enableTouch) {
                timerId = GLib.Timeout.Add (20, OnTimerEvent);
                clicked = true;
            }
        }

        protected void OnProgressBarRelease (object o, ButtonReleaseEventArgs args) {
            clicked = false;
            ProgressChangedEvent?.Invoke (this, new ProgressChangeEventArgs (_currentProgress));
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                GetPointer (out int x, out int y);
                float height = Allocation.Height;
                float width = Allocation.Width;
                currentProgress = orientation == TouchOrientation.Vertical ? (height - y) / height : x / width;
                ProgressChangingEvent?.Invoke (this, new ProgressChangeEventArgs (_currentProgress));
                QueueDraw ();
            }

            return clicked;
        }

        protected void DrawProgressBar (Context cr, float progress, TouchColor color) {
            var left = Allocation.Left;
            var top = Allocation.Top;
            var width = Allocation.Width;
            var height = Allocation.Height;
            int bottom = top + height;
            double difference;

            if (orientation == TouchOrientation.Vertical) {
                difference = (height - width) * progress;
                top += height - width - difference.ToInt ();

                cr.MoveTo (left, bottom - radius);
                cr.ArcNegative (left + radius, bottom - radius, radius, Math.PI, 0);
                cr.LineTo (left + width, top + radius);
                cr.ArcNegative (left + radius, top + radius, radius, 0, Math.PI);
                cr.ClosePath ();
            } else {
                difference = ((width - height) * progress);

                cr.MoveTo (left + radius, top);
                cr.ArcNegative (left + radius, top + radius, radius, 3 * Math.PI / 2, Math.PI / 2);
                cr.LineTo (left + difference + height - radius, bottom);
                cr.ArcNegative (left + difference + height - radius, top + radius, radius, Math.PI / 2, 3 * Math.PI / 2);
                cr.ClosePath ();
            }

            var outlineColor = new TouchColor (color);
            outlineColor.ModifyColor (0.5);
            var highlightColor = new TouchColor (color);
            highlightColor.ModifyColor (1.4);
            var lowlightColor = new TouchColor (color);
            lowlightColor.ModifyColor (0.75);

            using (var grad = new LinearGradient (left, top, left, bottom)) {
                grad.AddColorStop (0, highlightColor.ToCairoColor ());
                grad.AddColorStop (0.2, color.ToCairoColor ());
                grad.AddColorStop (0.85, lowlightColor.ToCairoColor ());
                cr.SetSource (grad);
                cr.FillPreserve ();
            }

            outlineColor.SetSource (cr);
            cr.LineWidth = 1;
            cr.Stroke ();
        }
    }
}

