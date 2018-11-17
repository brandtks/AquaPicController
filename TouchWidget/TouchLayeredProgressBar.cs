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

                radius = orientation == TouchOrientation.Vertical ? width / 2 : height / 2;

                TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, width / 2);
                colorBackground.SetSource (cr);
                cr.Fill ();

                if (currentProgress > currentProgressSecondary) {
                    DrawProgressBar (cr, currentProgress, colorProgress);
                    DrawProgressBar (cr, currentProgressSecondary, colorProgressSecondary);
                } else if (currentProgress < currentProgressSecondary) {
                    DrawProgressBar (cr, currentProgressSecondary, colorProgressSecondary);
                    DrawProgressBar (cr, currentProgress, colorProgress);
                } else {
                    if (drawPrimaryWhenEqual) {
                        DrawProgressBar (cr, currentProgress, colorProgress);
                    } else {
                        DrawProgressBar (cr, currentProgressSecondary, colorProgressSecondary);
                    }
                }
            }
        }
    }
}

