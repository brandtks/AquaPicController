using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchLayeredProgressBar : TouchProgressBar
    {
        public MyColor colorProgressSecondary;
        public float currentProgressSecondary;
        public bool drawPrimaryWhenEqual;

        public TouchLayeredProgressBar (
            MyColor colorBackground, 
            MyColor colorProgress,
            float currentProgress, 
            bool enableTouch, 
            MyOrientation orientation,
            MyColor colorProgressSecondary, 
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
            : base (new MyColor ("grey4"), new MyColor ("pri"), 0.0f, false, MyOrientation.Vertical) {
            colorProgressSecondary = new MyColor ("seca");
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

                if (orientation == MyOrientation.Vertical) {
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
            if (orientation == MyOrientation.Vertical) {
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
            if (orientation == MyOrientation.Vertical) {
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

