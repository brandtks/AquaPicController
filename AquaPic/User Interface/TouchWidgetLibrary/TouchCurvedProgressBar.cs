using System;
using Gtk;
using Cairo;
using AquaPic.Utilites;

namespace TouchWidgetLibrary
{
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

            SetSizeRequest (160, 80);

            this.ExposeEvent += OnExpose;
        }

        public TouchCurvedProgressBar () 
            : this (new TouchColor ("grey4"), new TouchColor ("pri"), 0.0f) { 
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int barWidth = 25;
                int x = Allocation.Left;
                int width = Allocation.Width;
                int height = Allocation.Height;
                int right = Allocation.Right;

                int originX = x + (width / 2);
                int originY = Allocation.Bottom;

                cr.MoveTo (x, originY);
                cr.Arc (originX, originY, height, (180.0).ToRadians (), (0.0).ToRadians ());
                cr.LineTo (right - barWidth, originY);
                cr.ArcNegative (originX, originY, height - barWidth, (0.0).ToRadians (), (180.0).ToRadians ());
                cr.LineTo (x, originY);
                cr.ClosePath ();
                backgroundColor.SetSource (cr);
                cr.Fill ();

                double r = ((1 - _progress) * -180.0).ToRadians ();
                double x2 = TouchGlobal.CalcX (originX, height - barWidth, r);
                double y2 = TouchGlobal.CalcY (originY, height - barWidth, r);

                cr.MoveTo (x, originY);
                cr.Arc (originX, originY, height, (180.0).ToRadians (), r);
                cr.LineTo (x2, y2);
                cr.ArcNegative (originX, originY, height - barWidth, r, (180.0).ToRadians ());
                cr.LineTo (x, originY);
                cr.ClosePath ();
                progressColor.SetSource (cr);
                cr.Fill ();
            }
        }
    }
}

