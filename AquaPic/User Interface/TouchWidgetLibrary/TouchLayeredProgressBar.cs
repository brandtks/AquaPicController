using System;
using Gtk;
using Cairo;

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
                    cr.Rectangle (left, top, width, height);
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
                int height = Allocation.Height;
                int difference = (int)(height * currentProgress);
                int top = Allocation.Top;
                top += (height - difference);
                cr.Rectangle (Allocation.Left, top, Allocation.Width, difference);
                colorProgress.SetSource (cr);
                cr.Fill ();
            }
        }

        protected void DrawSecondary (Context cr) {
            if (_orient == TouchOrientation.Vertical) {
                int height = Allocation.Height;
                int difference = (int)(height * currentProgressSecondary);
                int top = Allocation.Top;
                top += (height - difference);
                cr.Rectangle (Allocation.Left, top, Allocation.Width, difference);
                colorProgressSecondary.SetSource (cr);
                cr.Fill ();
            }
        }
    }
}

