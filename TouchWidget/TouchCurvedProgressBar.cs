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
        private float _progress;
        public float progress {
            get {
                return _progress;
            }
            set {
                if (value < 0.0f)
                    _progress = 0.0f;
                else if (value > 1.0f)
                    _progress = 1.0f;
                else
                    _progress = value;
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
            this.Visible = true;
            this.VisibleWindow = false;

            this.backgroundColor = colorBackground;
            this.progressColor = colorProgress;
            this._progress = currentProgress;
            barWidth = 20;
            curveStyle = CurveStyle.HalfCurve;

            SetSizeRequest (160, 80);

            this.ExposeEvent += OnExpose;
        }

        public TouchCurvedProgressBar ()
            : this (new TouchColor ("grey4"), new TouchColor ("pri"), 0.0f) {
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                int x = Allocation.Left;
                int width = Allocation.Width;
                int height = Allocation.Height;
                int radius = barWidth / 2;
                int bigRadius;

                double fudgeFactor = -0.0125 * radius;

                if (curveStyle == CurveStyle.HalfCurve) {
                    bigRadius = height - radius - 2;
                    int originX = x + (width / 2);
                    int originY = Allocation.Top + height - radius;

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
                    cr.LineTo (x, originY);
                    cr.ClosePath ();
                    progressColor.SetSource (cr);
                    cr.Fill ();
                } else if (curveStyle == CurveStyle.ThreeQuarterCurve) {
                    int below = (0.45 * height).ToInt ();
                    bigRadius = height - below - 2;
                    int originX = x + (width / 2);
                    int originY = Allocation.Top + height - below;

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
                    cr.ClosePath ();
                    progressColor.SetSource (cr);
                    cr.Fill ();
                }
            }
        }
    }
}

