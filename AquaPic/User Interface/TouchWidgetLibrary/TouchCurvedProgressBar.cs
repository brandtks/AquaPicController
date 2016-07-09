using System;
using Gtk;
using Cairo;
using AquaPic.Utilites;

namespace TouchWidgetLibrary
{
    public enum CurveStyle {
        HalfCurve,
        ThreeQuarterCurve
    }

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

        public int barWidth;
        public CurveStyle curveStyle;

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
            barWidth = 20;
            curveStyle = CurveStyle.HalfCurve;

            SetSizeRequest (160, 80);

            this.ExposeEvent += OnExpose;
        }

        public TouchCurvedProgressBar () 
            : this (new TouchColor ("grey4"), new TouchColor ("pri"), 0.0f) { 
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int x = Allocation.Left;
                int width = Allocation.Width;
                int height = Allocation.Height;
                //int right = x + width;
                int radius = barWidth / 2;
                int bigRadius;

                double fudgeFactor = -0.0125 * radius;

                if (curveStyle == CurveStyle.HalfCurve) {
                    bigRadius = height - radius - 2;
                    int originX = x + (width / 2);
                    int originY = Allocation.Top + height - radius;

                    cr.MoveTo (originX - bigRadius + barWidth, originY);
                    cr.Arc (originX - bigRadius + radius, originY, radius, (0.0).ToRadians (), (180.0).ToRadians ());
                    cr.Arc (originX, originY, bigRadius, (180.0).ToRadians (), (0.0).ToRadians ());
                    cr.Arc (originX + bigRadius - radius, originY, radius, (0.0).ToRadians (), (180.0).ToRadians ());
                    cr.ArcNegative (originX, originY, bigRadius - barWidth + fudgeFactor, (0.0).ToRadians (), (180.0).ToRadians ());
                    cr.ClosePath ();
                    backgroundColor.SetSource (cr);
                    cr.Fill ();

                    double r = ((1 - _progress) * -180.0).ToRadians ();
                    double x2 = TouchGlobal.CalcX (originX, bigRadius - radius, r);
                    double y2 = TouchGlobal.CalcY (originY, bigRadius - radius, r);

                    cr.MoveTo (originX - bigRadius + barWidth, originY);
                    cr.Arc (originX - bigRadius + radius, originY, radius, (0.0).ToRadians (), (180.0).ToRadians ());
                    cr.Arc (originX, originY, bigRadius, (180.0).ToRadians (), r);
                    cr.Arc (x2, y2, radius, r, r + Math.PI);
                    cr.ArcNegative (originX, originY, bigRadius - barWidth + fudgeFactor, r, (180.0).ToRadians ());
                    cr.LineTo (x, originY);
                    cr.ClosePath ();
                    progressColor.SetSource (cr);
                    cr.Fill ();
                } else if (curveStyle == CurveStyle.ThreeQuarterCurve) {
                    int below = (0.45 * (double)height).ToInt ();
                    bigRadius = height - below - 2;
                    int originX = x + (width / 2);
                    int originY = Allocation.Top + height - below;

                    double x2 = TouchGlobal.CalcX (originX, bigRadius - radius, (135.0).ToRadians ());
                    double y2 = TouchGlobal.CalcY (originY, bigRadius - radius, (135.0).ToRadians ());

                    cr.MoveTo (x2, y2);
                    cr.Arc (x2, y2, radius, (-45.0).ToRadians (), (135.0).ToRadians ());
                    cr.Arc (originX, originY, bigRadius, (135.0).ToRadians (), (45.0).ToRadians ());
                    x2 = TouchGlobal.CalcX (originX, bigRadius - radius, (45.0).ToRadians ());
                    y2 = TouchGlobal.CalcY (originY, bigRadius - radius, (45.0).ToRadians ());
                    cr.Arc (x2, y2, radius, (45.0).ToRadians (), (-135.0).ToRadians ());
                    cr.ArcNegative (originX, originY, bigRadius - barWidth + fudgeFactor, (45.0).ToRadians (), (135.0).ToRadians ());
                    cr.ClosePath ();
                    backgroundColor.SetSource (cr);
                    cr.Fill ();

                    double r = (_progress * 270.0 + 135.0).ToRadians ();

                    x2 = TouchGlobal.CalcX (originX, bigRadius - radius, (135.0).ToRadians ());
                    y2 = TouchGlobal.CalcY (originY, bigRadius - radius, (135.0).ToRadians ());
                    cr.Arc (x2, y2, radius, (-45.0).ToRadians (), (135.0).ToRadians ());
                    cr.Arc (originX, originY, bigRadius, (135.0).ToRadians (), r);
                    x2 = TouchGlobal.CalcX (originX, bigRadius - radius, r);
                    y2 = TouchGlobal.CalcY (originY, bigRadius - radius, r);
                    cr.Arc (x2, y2, radius, r, r + Math.PI);
                    cr.ArcNegative (originX, originY, bigRadius - barWidth + fudgeFactor, r, (135.0).ToRadians ());
                    cr.ClosePath ();
                    progressColor.SetSource (cr);
                    cr.Fill ();
                }
            }
        }
    }
}

