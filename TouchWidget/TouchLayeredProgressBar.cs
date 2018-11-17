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
    public class TouchLayeredProgressBar : TouchProgressBar
    {
        public TouchColor colorProgressSecondary;
        float _currentProgressSecondary;
        public float currentProgressSecondary {
            get {
                return _currentProgressSecondary;
            }
            set {
                _currentProgressSecondary = value;
                _currentProgressSecondary.Constrain (0, 1);
            }
        }
        public bool drawPrimaryWhenEqual;

        public TouchLayeredProgressBar (
            TouchColor colorBackground,
            TouchColor colorProgress,
            float currentProgress,
            bool enableTouch,
            TouchOrientation orientation,
            TouchColor colorProgressSecondary,
            float currentProgressSecondary,
            bool drawPrimaryWhenEqual)
            : base (
                colorBackground,
                colorProgress,
                currentProgress,
                enableTouch,
                orientation) {
            this.colorProgressSecondary = colorProgressSecondary;
            this.currentProgressSecondary = currentProgressSecondary;
            this.drawPrimaryWhenEqual = drawPrimaryWhenEqual;

            ExposeEvent -= OnExpose;
            ExposeEvent += OnExposeSecondary;
        }

        public TouchLayeredProgressBar ()
            : base (new TouchColor ("grey4"), new TouchColor ("pri"), 0, false, TouchOrientation.Vertical) {
            colorProgressSecondary = new TouchColor ("seca");
            currentProgressSecondary = 0;
            drawPrimaryWhenEqual = true;

            ExposeEvent -= OnExpose;
            ExposeEvent += OnExposeSecondary;
        }

        protected void OnExposeSecondary (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;

                if (orientation == TouchOrientation.Vertical) {
                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, width / 2);
                    colorBackground.SetSource (cr);
                    cr.Fill ();
                }

                if (currentProgress > currentProgressSecondary) {
                    DrawPrimary (cr);
                    DrawSecondary (cr);
                } else if (currentProgress < currentProgressSecondary) {
                    DrawSecondary (cr);
                    DrawPrimary (cr);
                } else {
                    if (drawPrimaryWhenEqual)
                        DrawPrimary (cr);
                    else
                        DrawSecondary (cr);
                }
            }
        }

        protected void DrawPrimary (Context cr) {
            if (orientation == TouchOrientation.Vertical) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;
                int radius, bottom;
                double difference;

                radius = width / 2;
                bottom = top + height;

                difference = (height - width) * currentProgress;
                top += (height - width - difference.ToInt ());

                cr.MoveTo (left, bottom - radius);
                cr.ArcNegative (left + radius, bottom - radius, radius, Math.PI, 0);
                cr.LineTo (left + width, top + radius);
                cr.ArcNegative (left + radius, top + radius, radius, 0, Math.PI);
                cr.ClosePath ();

                var outlineColor = new TouchColor (colorProgress);
                outlineColor.ModifyColor (0.5);
                var highlightColor = new TouchColor (colorProgress);
                highlightColor.ModifyColor (1.4);
                var lowlightColor = new TouchColor (colorProgress);
                lowlightColor.ModifyColor (0.75);

                using (var grad = new LinearGradient (left, top, left, bottom)) {
                    grad.AddColorStop (0, highlightColor.ToCairoColor ());
                    grad.AddColorStop (0.2, colorProgress.ToCairoColor ());
                    grad.AddColorStop (0.85, lowlightColor.ToCairoColor ());
                    cr.SetSource (grad);
                    cr.FillPreserve ();
                }

                outlineColor.SetSource (cr);
                cr.LineWidth = 1;
                cr.Stroke ();
            }
        }

        protected void DrawSecondary (Context cr) {
            if (orientation == TouchOrientation.Vertical) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;
                int radius, bottom;
                double difference;

                radius = width / 2;
                bottom = top + height;

                difference = ((height - width) * currentProgressSecondary);
                top += ((height - width) - difference.ToInt ());

                cr.MoveTo (left, bottom - radius);
                cr.ArcNegative (left + radius, bottom - radius, radius, Math.PI, 0);
                cr.LineTo (left + width, top + radius);
                cr.ArcNegative (left + radius, top + radius, radius, 0, Math.PI);
                cr.ClosePath ();

                var outlineColor = new TouchColor (colorProgressSecondary);
                outlineColor.ModifyColor (0.5);
                var highlightColor = new TouchColor (colorProgressSecondary);
                highlightColor.ModifyColor (1.4);
                var lowlightColor = new TouchColor (colorProgressSecondary);
                lowlightColor.ModifyColor (0.75);

                using (var grad = new LinearGradient (left, top, left, bottom)) {
                    grad.AddColorStop (0, highlightColor.ToCairoColor ());
                    grad.AddColorStop (0.2, colorProgressSecondary.ToCairoColor ());
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
}

