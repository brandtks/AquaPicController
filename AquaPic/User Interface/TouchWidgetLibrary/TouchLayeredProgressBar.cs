using System;
using Gtk;
using Cairo;
using AquaPic.Utilites;

namespace TouchWidgetLibrary
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
                int difference, radius, bottom;

                radius = width / 2;
                bottom = top + height;

                double aspectRatio = (double)width / (double)height;
                if (currentProgress > aspectRatio) {
                    difference = (height * currentProgress).ToInt ();
                    top += (height - difference);

                    cr.MoveTo (left, bottom - radius);
                    cr.ArcNegative (left + radius, bottom - radius, radius, (180.0).ToRadians (), (0.0).ToRadians ());
                    cr.LineTo (left + width, top + radius);
                    cr.ArcNegative (left + radius, top + radius, radius, (0.0).ToRadians (), (180.0).ToRadians ());
                    cr.ClosePath ();
                } else {
                    double angle1 = (1 - (currentProgress / aspectRatio)) * 180.0 - 90;
                    double r1 = angle1.ToRadians ();
                    double r2 = (180 - angle1).ToRadians ();

                    double x2 = TouchGlobal.CalcX (left + radius, radius, r2);
                    double y2 = TouchGlobal.CalcY (bottom - radius, radius, r2);

                    cr.MoveTo (x2, y2);
                    cr.ArcNegative (left + radius, bottom - radius, radius, r2, r1);
                    cr.ClosePath ();
                }
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
                int difference, radius, bottom;

                radius = width / 2;
                bottom = top + height;

                double aspectRatio = (double)width / (double)height;
                if (currentProgressSecondary > aspectRatio) {
                    difference = (height * currentProgressSecondary).ToInt ();
                    top += (height - difference);

                    cr.MoveTo (left, bottom - radius);
                    cr.ArcNegative (left + radius, bottom - radius, radius, (180.0).ToRadians (), (0.0).ToRadians ());
                    cr.LineTo (left + width, top + radius);
                    cr.ArcNegative (left + radius, top + radius, radius, (0.0).ToRadians (), (180.0).ToRadians ());
                    cr.ClosePath ();
                } else {
                    double angle1 = (1 - (currentProgressSecondary / aspectRatio)) * 180.0 - 90;
                    double r1 = angle1.ToRadians ();
                    double r2 = (180 - angle1).ToRadians ();

                    double x2 = TouchGlobal.CalcX (left + radius, radius, r2);
                    double y2 = TouchGlobal.CalcY (bottom - radius, radius, r2);

                    cr.MoveTo (x2, y2);
                    cr.ArcNegative (left + radius, bottom - radius, radius, r2, r1);
                    cr.ClosePath ();
                }
                colorProgressSecondary.SetSource (cr);
                cr.Fill ();
            }
        }
    }
}

