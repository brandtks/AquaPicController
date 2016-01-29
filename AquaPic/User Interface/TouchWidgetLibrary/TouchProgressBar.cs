using System;
using Gtk;
using Cairo;

namespace TouchWidgetLibrary
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
        protected TouchOrientation _orient;
        private float _currentProgress;
        public float currentProgress {
            get {
                return _currentProgress;
            }
            set {
                if (value < 0.0f)
                    _currentProgress = 0.0f;
                else if (value > 1.0f)
                    _currentProgress = 1.0f;
                else
                    _currentProgress = value;
            }
        }

        public TouchColor colorBackground;
        public TouchColor colorProgress;
        public bool enableTouch;

        public event ProgressChangeEventHandler ProgressChangedEvent;
        public event ProgressChangeEventHandler ProgressChangingEvent;

        private uint timerId;
        private bool clicked;

        public TouchProgressBar (
            TouchColor colorBackground, 
            TouchColor colorProgress,
            float currentProgress, 
            bool enableTouch, 
            TouchOrientation orientation
        ) {
            this.Visible = true;
            this.VisibleWindow = false;

            this.colorBackground = colorBackground;
            this.colorProgress = colorProgress;
            this._currentProgress = currentProgress;
            this.enableTouch = enableTouch;
            _orient = orientation;
            this.timerId = 0;
            this.clicked = false;

            if (_orient == TouchOrientation.Vertical) {
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
            : this (new TouchColor ("grey4"), new TouchColor ("pri"), 0.0f, false, TouchOrientation.Vertical) { 
        }

        public TouchProgressBar (TouchOrientation orientation)
            : this (new TouchColor ("grey4"), new TouchColor ("pri"), 0.0f, false, orientation) {
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;
                int difference;

                if (_orient == TouchOrientation.Vertical) {
                    cr.Rectangle (left, top, width, height);
                    colorBackground.SetSource (cr);
                    cr.Fill ();

                    difference = (int)(height * _currentProgress);
                    top += (height - difference);
                    cr.Rectangle (left, top, width, difference);
                    colorProgress.SetSource (cr);
                    cr.Fill ();
                } else {
                    cr.Rectangle (left, top, width, height);
                    colorBackground.SetSource (cr);
                    cr.Fill ();

                    difference = (int)(width * _currentProgress);
                    cr.Rectangle (left, top, difference, height);
                    colorProgress.SetSource (cr);
                    cr.Fill ();
                }
            }
        }

        protected void OnProgressBarPress (object o, ButtonPressEventArgs args) {
            if (enableTouch) {
                timerId = GLib.Timeout.Add (20, OnTimerEvent);
                clicked = true;
            }
        }

        protected void OnProgressBarRelease (object o, ButtonReleaseEventArgs args) {
            clicked = false;

            if (ProgressChangedEvent != null)
                ProgressChangedEvent (this, new ProgressChangeEventArgs (_currentProgress));
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);
                if (_orient == TouchOrientation.Vertical)
                    _currentProgress = (float)(Allocation.Height - y) / (float)Allocation.Height;
                else
                    _currentProgress = (float)x / (float)Allocation.Width;

                if (_currentProgress > 1.0f)
                    _currentProgress = 1.0f;
                if (_currentProgress < 0.0f)
                    _currentProgress = 0.0f;

                if (ProgressChangingEvent != null)
                    ProgressChangingEvent (this, new ProgressChangeEventArgs (_currentProgress));

                QueueDraw ();
            }

            return clicked;
        }
    }
}

