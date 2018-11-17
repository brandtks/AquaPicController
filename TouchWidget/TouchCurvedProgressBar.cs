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
    public enum CurveStyle
    {
        HalfCurve,
        ThreeQuarterCurve
    }

    public class TouchCurvedProgressBar : EventBox
    {
        float _progress;
        public float progress {
            get {
                return _progress;
            }
            set {
                _progress = value;
                _progress.Constrain (0, 1);
            }
        }

        public int barWidth;
        public CurveStyle curveStyle;

        public TouchColor backgroundColor;
        public TouchColor progressColor;

        public TouchCurvedProgressBar (
            TouchColor colorBackground,
            TouchColor colorProgress,
            float currentProgress
        ) {
            Visible = true;
            VisibleWindow = false;

            backgroundColor = colorBackground;
            progressColor = colorProgress;
            _progress = currentProgress;
            barWidth = 20;
            curveStyle = CurveStyle.HalfCurve;

            SetSizeRequest (160, 80);

            ExposeEvent += OnExpose;
        }

        public TouchCurvedProgressBar ()
            : this (new TouchColor ("grey4"), new TouchColor ("pri"), 0) {
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                var left = Allocation.Left;
                var top = Allocation.Top;
                var width = Allocation.Width;
                var height = Allocation.Height;
                var radius = barWidth / 2;
                int bigRadius;

                double fudgeFactor = -0.0125 * radius;

                if (curveStyle == CurveStyle.HalfCurve) {
                    bigRadius = height - radius - 2;
                    int originX = left + (width / 2);
                    int originY = top + height - radius;

                    cr.MoveTo (originX - bigRadius + barWidth, originY);
                    cr.Arc (originX - bigRadius + radius, originY, radius, 0, Math.PI);
                    cr.Arc (originX, originY, bigRadius, Math.PI, 0);
                    cr.Arc (originX + bigRadius - radius, originY, radius, 0, Math.PI);
                    cr.ArcNegative (originX, originY, bigRadius - barWidth + fudgeFactor, 0, Math.PI);
                    cr.ClosePath ();
                    backgroundColor.SetSource (cr);
                    cr.Fill ();

                    double r = ((1 - _progress) * -180.0).ToRadians ();
                    double x2 = TouchGlobal.CalcX (originX, bigRadius - radius, r);
                    double y2 = TouchGlobal.CalcY (originY, bigRadius - radius, r);

                    cr.MoveTo (originX - bigRadius + barWidth, originY);
                    cr.Arc (originX - bigRadius + radius, originY, radius, 0, Math.PI);
                    cr.Arc (originX, originY, bigRadius, Math.PI, r);
                    cr.Arc (x2, y2, radius, r, r + Math.PI);
                    cr.ArcNegative (originX, originY, bigRadius - barWidth + fudgeFactor, r, Math.PI);
                    cr.LineTo (left, originY);
                    cr.ClosePath ();
                    progressColor.SetSource (cr);
                    cr.Fill ();
                } else if (curveStyle == CurveStyle.ThreeQuarterCurve) {
                    int below = (0.45 * height).ToInt ();
                    bigRadius = height - below - 2;
                    int originX = left + (width / 2);
                    int originY = top + height - below;

                    double x2 = TouchGlobal.CalcX (originX, bigRadius - radius, 3 * Math.PI / 4);
                    double y2 = TouchGlobal.CalcY (originY, bigRadius - radius, 3 * Math.PI / 4);

                    cr.MoveTo (x2, y2);
                    cr.Arc (x2, y2, radius, 7 * Math.PI / 4, 3 * Math.PI / 4);
                    cr.Arc (originX, originY, bigRadius, 3 * Math.PI / 4, Math.PI / 4);
                    x2 = TouchGlobal.CalcX (originX, bigRadius - radius, Math.PI / 4);
                    y2 = TouchGlobal.CalcY (originY, bigRadius - radius, Math.PI / 4);
                    cr.Arc (x2, y2, radius, Math.PI / 4, (-135.0).ToRadians ());
                    cr.ArcNegative (originX, originY, bigRadius - barWidth + fudgeFactor, Math.PI / 4, 3 * Math.PI / 4);
                    cr.ClosePath ();
                    backgroundColor.SetSource (cr);
                    cr.Fill ();

                    double r = (_progress * 270.0 + 135.0).ToRadians ();

                    x2 = TouchGlobal.CalcX (originX, bigRadius - radius, 3 * Math.PI / 4);
                    y2 = TouchGlobal.CalcY (originY, bigRadius - radius, 3 * Math.PI / 4);
                    cr.Arc (x2, y2, radius, 7 * Math.PI / 4, 3 * Math.PI / 4);
                    cr.Arc (originX, originY, bigRadius, 3 * Math.PI / 4, r);
                    x2 = TouchGlobal.CalcX (originX, bigRadius - radius, r);
                    y2 = TouchGlobal.CalcY (originY, bigRadius - radius, r);
                    cr.Arc (x2, y2, radius, r, r + Math.PI);
                    cr.ArcNegative (originX, originY, bigRadius - barWidth + fudgeFactor, r, 3 * Math.PI / 4);

                    var outlineColor = new TouchColor (progressColor);
                    outlineColor.ModifyColor (0.5);
                    var highlightColor = new TouchColor (progressColor);
                    highlightColor.ModifyColor (1.4);
                    var lowlightColor = new TouchColor (progressColor);
                    lowlightColor.ModifyColor (0.75);

                    var colorOriginX = TouchGlobal.CalcX (originX, bigRadius - 5, 11 * Math.PI / 8);
                    var colorOriginY = TouchGlobal.CalcY (originY, bigRadius - 5, 11 * Math.PI / 8);
                    using (var grad = new RadialGradient (colorOriginX, colorOriginY, 0, colorOriginX, colorOriginY, (bigRadius - 5) * 2)) {
                        grad.AddColorStop (0, highlightColor.ToCairoColor ());
                        grad.AddColorStop (0.35, progressColor.ToCairoColor ());
                        grad.AddColorStop (0.75, lowlightColor.ToCairoColor ());
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
}

