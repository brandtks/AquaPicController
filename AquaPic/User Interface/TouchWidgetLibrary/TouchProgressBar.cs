using System;
using Gtk;
using Cairo;
using AquaPic.Utilites;

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
                int bottom = Allocation.Bottom;
                int difference, radius;

                if (_orient == TouchOrientation.Vertical) {
                    radius = width / 2;

                    //cr.Rectangle (left, top, width, height);
                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, radius);
                    colorBackground.SetSource (cr);
                    cr.Fill ();

                    difference = (height * _currentProgress).ToInt ();
                    top += (height - difference);
                    //cr.Rectangle (left, top, width, difference);
                    if (difference > width) {
                        cr.MoveTo (left, bottom - radius);
                        cr.ArcNegative (left + radius, bottom - radius, radius, (180.0).ToRadians (), (0.0).ToRadians ());
                        cr.LineTo (left + width, top + radius);
                        cr.ArcNegative (left + radius, top + radius, radius, (0.0).ToRadians (), (180.0).ToRadians ());
                        cr.ClosePath ();
                    } else {
                        Console.WriteLine ("\nleft + radius is {0}, bottom is {1}, width is {2}", left + radius, bottom, width);
                        Console.WriteLine ("difference is {0}", difference);

                        double p =  (1.0 - (double)difference / (double)width);
                        Console.WriteLine ("p is {0}", p);

                        double angle1 = p * 90.0;
                        Console.WriteLine ("angle1 is {0}", angle1);
                        double r1 = angle1.ToRadians ();
                        Console.WriteLine ("r1 is {0}", r1);

                        double angle2 = 180 - angle1;
                        double r2 = angle2.ToRadians ();

                        double x2 = TouchGlobal.CalcX (left + radius, radius, r2);
                        double y2 = TouchGlobal.CalcY (bottom - radius, radius, r2);
                        Console.WriteLine ("x2 is {0}, y2 is {1}", x2, y2);

                        cr.MoveTo (x2, y2);
                        cr.ArcNegative (left + radius, bottom - radius, radius, r2, r1);
                        cr.ClosePath ();
                    }
                    colorProgress.SetSource (cr);
                    cr.Fill ();
                } else {
                    //cr.Rectangle (left, top, width, height);
                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, height / 2);
                    colorBackground.SetSource (cr);
                    cr.Fill ();

                    difference = (int)(width * _currentProgress);
                    //cr.Rectangle (left, top, difference, height);
                    TouchGlobal.DrawRoundedRectangle (cr, left, top, difference, height, height / 2);
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

