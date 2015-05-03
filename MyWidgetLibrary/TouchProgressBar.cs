using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchProgressBar : EventBox
    {
        public MyColor ColorBackground;
        public MyColor ColorProgress;
        public float CurrentProgress;
        public bool EnableTouch;
        public MyOrientation Orientation;

        private uint timer;
        private bool clicked;

        public TouchProgressBar (
            MyColor colorBackground, 
            MyColor colorProgress, 
            float currentProgress, 
            bool enableTouch, 
            MyOrientation orientation
        ) {
            this.Visible = true;
            this.VisibleWindow = false;

            this.ColorBackground = colorBackground;
            this.ColorProgress = colorProgress;
            this.CurrentProgress = currentProgress;
            this.EnableTouch = enableTouch;
            this.Orientation = orientation;
            this.timer = 0;
            this.clicked = false;

            if (this.Orientation == MyOrientation.Vertical) {
                this.WidthRequest = 30;
                this.HeightRequest = 200;
            } else {
                this.WidthRequest = 200;
                this.HeightRequest = 30;
            }

            this.ExposeEvent += OnExpose;
            this.ButtonPressEvent += OnProgressBarPress;
            this.ButtonReleaseEvent += OnProgressBarRelease;
        }

        public TouchProgressBar () 
            : this (new MyColor ("grey4"), new MyColor ("pri"), 0.0f, false, MyOrientation.Vertical) { 
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;
                int difference;

                if (Orientation == MyOrientation.Vertical) {
                    cr.Rectangle (left, top, width, height);
                    ColorBackground.SetSource (cr);
                    cr.Fill ();

                    difference = (int)(height * CurrentProgress);
                    top += (height - difference);
                    cr.Rectangle (left, top, width, difference);
                    ColorProgress.SetSource (cr);
                    cr.Fill ();
                }
            }
        }

        protected void OnProgressBarPress (object o, ButtonPressEventArgs args) {
            if (EnableTouch) {
                timer = GLib.Timeout.Add (20, OnTimerEvent);
                clicked = true;
            }
        }

        protected void OnProgressBarRelease (object o, ButtonReleaseEventArgs args) {
            clicked = false;
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);
                if (Orientation == MyOrientation.Vertical)
                    CurrentProgress = (float)(Allocation.Height - y) / (float)Allocation.Height;
                else
                    CurrentProgress = (float)(Allocation.Width - x) / (float)Allocation.Width;

                QueueDraw ();
            }

            return clicked;
        }
    }
}

