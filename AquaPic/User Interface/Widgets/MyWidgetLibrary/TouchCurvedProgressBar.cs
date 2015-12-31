using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
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

        public MyColor backgroundColor;
        public MyColor progressColor;

        public TouchCurvedProgressBar (
            MyColor colorBackground, 
            MyColor colorProgress,
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
            : this (new MyColor ("grey4"), new MyColor ("pri"), 0.0f) { 
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int barWidth = 25;
                int x = Allocation.Left;
                int y = Allocation.Right;
                int width = Allocation.Width;
                int height = Allocation.Height;
                int right = Allocation.Right;

                int originX = x + (width / 2);
                int originY = Allocation.Bottom;

                cr.MoveTo (x, originY);
                cr.Arc (originX, originY, height, CalcRadians (180.0), CalcRadians (0.0));
                cr.LineTo (right - barWidth, originY);
                cr.ArcNegative (originX, originY, height - barWidth, CalcRadians (0.0), CalcRadians (180.0));
                cr.LineTo (x, originY);
                cr.ClosePath ();
                backgroundColor.SetSource (cr);
                cr.Fill ();

                double r = CalcRadians ((1 - _progress) * -180.0);
                double x2 = CalcX (originX, height - barWidth, r);
                double y2 = CalcY (originY, height - barWidth, r);

                cr.MoveTo (x, originY);
                cr.Arc (originX, originY, height, CalcRadians (180.0), r);
                cr.LineTo (x2, y2);
                cr.ArcNegative (originX, originY, height - barWidth, r, CalcRadians (180.0));
                cr.LineTo (x, originY);
                cr.ClosePath ();
                progressColor.SetSource (cr);
                cr.Fill ();
            }
        }

        protected double CalcRadians (double angle) {
            return (Math.PI / 180) * angle;
        }

        protected double CalcX (double originX, double radius, double radians) {
            return originX + radius * Math.Cos (radians);
        }

        protected double CalcY (double originY, double radius, double radians) {
            return originY + radius * Math.Sin (radians);
        }
    }
}

