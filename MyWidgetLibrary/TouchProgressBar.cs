using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
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
        public MyColor colorBackground;
        public MyColor colorProgress;
        public float currentProgress;
        public bool enableTouch;
        public MyOrientation orientation;

        public ProgressChangeEventHandler ProgressChangedEvent;
        public ProgressChangeEventHandler ProgressChangingEvent;

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

            this.colorBackground = colorBackground;
            this.colorProgress = colorProgress;
            this.currentProgress = currentProgress;
            this.enableTouch = enableTouch;
            this.orientation = orientation;
            this.timer = 0;
            this.clicked = false;

            if (this.orientation == MyOrientation.Vertical) {
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

                if (orientation == MyOrientation.Vertical) {
                    cr.Rectangle (left, top, width, height);
                    colorBackground.SetSource (cr);
                    cr.Fill ();

                    difference = (int)(height * currentProgress);
                    top += (height - difference);
                    cr.Rectangle (left, top, width, difference);
                    colorProgress.SetSource (cr);
                    cr.Fill ();
                }
            }
        }

        protected void OnProgressBarPress (object o, ButtonPressEventArgs args) {
            if (enableTouch) {
                timer = GLib.Timeout.Add (20, OnTimerEvent);
                clicked = true;
            }
        }

        protected void OnProgressBarRelease (object o, ButtonReleaseEventArgs args) {
            clicked = false;

            if (ProgressChangedEvent != null)
                ProgressChangedEvent (this, new ProgressChangeEventArgs (currentProgress));
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);
                if (orientation == MyOrientation.Vertical)
                    currentProgress = (float)(Allocation.Height - y) / (float)Allocation.Height;
                else
                    currentProgress = (float)(Allocation.Width - x) / (float)Allocation.Width;

                if (currentProgress > 1.0f)
                    currentProgress = 1.0f;
                if (currentProgress < 0.0f)
                    currentProgress = 0.0f;

                if (ProgressChangingEvent != null)
                    ProgressChangingEvent (this, new ProgressChangeEventArgs (currentProgress));

                QueueDraw ();
            }

            return clicked;
        }
    }
}

