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
        public float currentProgressSecondary;
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
                orientation)
        {
            this.colorProgressSecondary = colorProgressSecondary;
            this.currentProgressSecondary = currentProgressSecondary;
            this.drawPrimaryWhenEqual = drawPrimaryWhenEqual;

            ExposeEvent -= OnExpose;
            ExposeEvent += OnExposeSecondary;
        }

        public TouchLayeredProgressBar ()
            : base (new TouchColor ("grey4"), new TouchColor ("pri"), 0.0f, false, TouchOrientation.Vertical) {
            colorProgressSecondary = new TouchColor ("seca");
            currentProgressSecondary = 0.0f;
            drawPrimaryWhenEqual = true;

            ExposeEvent -= OnExpose;
            ExposeEvent += OnExposeSecondary;
        }

        protected void OnExposeSecondary (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;

                if (_orient == TouchOrientation.Vertical) {
                    //cr.Rectangle (left, top, width, height);
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
            if (_orient == TouchOrientation.Vertical) {
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
                cr.ArcNegative (left + radius, bottom - radius, radius, (180.0).ToRadians (), (0.0).ToRadians ());
                cr.LineTo (left + width, top + radius);
                cr.ArcNegative (left + radius, top + radius, radius, (0.0).ToRadians (), (180.0).ToRadians ());
                cr.ClosePath ();

                colorProgress.SetSource (cr);
                cr.Fill ();
            }
        }

        protected void DrawSecondary (Context cr) {
            if (_orient == TouchOrientation.Vertical) {
                /*
                int height = Allocation.Height;
                int difference = (int)(height * currentProgressSecondary);
                int top = Allocation.Top;
                top += (height - difference);
                cr.Rectangle (Allocation.Left, top, Allocation.Width, difference);
                colorProgressSecondary.SetSource (cr);
                cr.Fill ();
                */
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
                cr.ArcNegative (left + radius, bottom - radius, radius, (180.0).ToRadians (), (0.0).ToRadians ());
                cr.LineTo (left + width, top + radius);
                cr.ArcNegative (left + radius, top + radius, radius, (0.0).ToRadians (), (180.0).ToRadians ());
                cr.ClosePath ();

                colorProgressSecondary.SetSource (cr);
                cr.Fill ();
            }
        }
    }
}

ssSecondary.SetSource (cr);
                cr.Fill ();
            }
        }
    }
}

