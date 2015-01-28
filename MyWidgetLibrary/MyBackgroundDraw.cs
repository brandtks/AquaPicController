using System;
//using System.Timers;
using Gtk;
//using Cairo;

namespace MyWidgetLibrary
{
    public class MyBackgroundWidget_old : Image 
    {
        public MyBackgroundWidget_old (string path) {
            Gdk.Pixbuf pic = new Gdk.Pixbuf (path);
            this.Pixbuf = pic;
            this.Visible = true;
        }
    }

    /*
    public partial class MyBackgroundWidget : DrawingArea
    {
//        private ImageSurface img;

        public delegate void BackgroundExposeEventHandler (object o);
        public event BackgroundExposeEventHandler BackgroundExposed;

        private bool clicked;
        private Timer clickTimer;
        private int clickAction;
        private int clickX, clickY;

        //private double dial;

//        public MyBackgroundWidget (ImageSurface img)
//        {
//            this.img = img;
//        }

        public MyBackgroundWidget () {
            this.Events = Gdk.EventMask.AllEventsMask;
            this.clicked = false;
            //this.dial = 0.0;
        }

        protected override bool OnExposeEvent(Gdk.EventExpose args) {
            Context cr = Gdk.CairoHelper.Create (this.GdkWindow);
            ImageSurface img = new ImageSurface ("images/background3.png");
            cr.SetSourceSurface (img, 0, 0);
            cr.Paint ();
            img.Dispose ();

//            cr.MoveTo (1280, 0);
//            cr.LineTo (1280, 150);
//            cr.Arc (640, -3921, 4121, 81.1 * (Math.PI / 180), 98.9 * (Math.PI / 180));
//            cr.LineTo (0, 150);
//            cr.LineTo (0, 0);
//            cr.ClosePath ();
//
//            Gradient pat = new RadialGradient (640, -3921, 4121, 640, -450, 450);
//            pat.AddColorStop (0, new Color (0.15, 0.15, 0.15, 0.75));
//            pat.AddColorStop (0.1, new Color (0.1, 0.1, 0.1, 0.75));
//            pat.AddColorStop (0.50, new Color (0.1, 0.1, 0.1, 0.75));
//            pat.AddColorStop (0.75, new Color (0.25, 0.25, 0.25, 0.75));
//            pat.AddColorStop (1, new Color (0.95, 0.95, 0.95, 0.75));
//            cr.SetSource (pat);
//            cr.Fill ();

            cr.MoveTo (0, 150);
            cr.CurveTo (106, 165, 212, 178, 318, 187);
            cr.CurveTo (325, 400, 319, 600, 300, 800);
            cr.LineTo (0, 800);
            cr.LineTo (0, 150);
            cr.ClosePath ();
            cr.SetSourceRGBA (0, 0, 0, 0.5);
            cr.Fill ();

            cr.Dispose ();

            if (BackgroundExposed != null) {
                BackgroundExposed (this);
            }

            clickTimer = new Timer (20);
            clickTimer.Elapsed += OnTimerEvent;

            return true;
        }

        private void drawSquare(ref Context cr, int x, int y, double scale) {
            cr.Save ();

            int sX, sY;

            if (scale > 1.0) {
                double scaleMul = ((scale - 1) * 100) / 2;
                sX = x - (int)scaleMul;
                sY = y - (int)scaleMul;
            } else {
                sX = x;
                sY = y;
            }

            cr.Rectangle (sX, sY, 100 * scale, 100 * scale);

            cr.SetSourceRGB (1, 0, 0);
            cr.Fill ();

            cr.Restore ();
        }

        private void drawUpTri(ref Context cr, int x, int y, double scale) {
            cr.Save ();

            int sX, sY;

            if (scale > 1.0) {
                double scaleMul = ((scale - 1) * 100) / 2;
                sX = x - (int)scaleMul;
                sY = y - (int)scaleMul;
            } else {
                sX = x;
                sY = y;
            }

            double sc = 100 * scale;

            cr.MoveTo (sX, sY + sc);
            cr.LineTo (sX + sc, sY + sc);
            cr.LineTo (sX + (sc / 2), sY);
            cr.LineTo (sX, sY + sc);
            cr.ClosePath ();

            cr.SetSourceRGB (1, 0, 0);
            cr.Fill ();

            cr.Restore ();
        }

        private void drawDownTri(ref Context cr, int x, int y, double scale) {
            cr.Save ();

            int sX, sY;

            if (scale > 1.0) {
                double scaleMul = ((scale - 1) * 100) / 2;
                sX = x - (int)scaleMul;
                sY = y - (int)scaleMul;
            } else {
                sX = x;
                sY = y;
            }

            double sc = 100 * scale;

            cr.MoveTo (sX, sY);
            cr.LineTo (sX + sc, sY);
            cr.LineTo (sX + (sc / 2), sY + sc);
            cr.LineTo (sX, sY);
            cr.ClosePath ();

            cr.SetSourceRGB (1, 0, 0);
            cr.Fill ();

            cr.Restore ();
        }

        private void drawRightTri(ref Context cr, int x, int y, double scale) {
            cr.Save ();

            int sX, sY;

            if (scale > 1.0) {
                double scaleMul = ((scale - 1) * 100) / 2;
                sX = x - (int)scaleMul;
                sY = y - (int)scaleMul;
            } else {
                sX = x;
                sY = y;
            }

            double sc = 100 * scale;

            cr.MoveTo (sX, sY);
            cr.LineTo (sX + sc, sY+ (sc / 2));
            cr.LineTo (sX, sY + sc);
            cr.LineTo (sX, sY);
            cr.ClosePath ();

            cr.SetSourceRGB (1, 0, 0);
            cr.Fill ();

            cr.Restore ();
        }

        private void drawLeftTri(ref Context cr, int x, int y, double scale) {
            cr.Save ();

            int sX, sY;

            if (scale > 1.0) {
                double scaleMul = ((scale - 1) * 100) / 2;
                sX = x - (int)scaleMul;
                sY = y - (int)scaleMul;
            } else {
                sX = x;
                sY = y;
            }

            double sc = 100 * scale;

            cr.MoveTo (sX + sc, sY);
            cr.LineTo (sX + sc, sY + sc);
            cr.LineTo (sX, sY + (sc / 2));
            cr.LineTo (sX + sc, sY);
            cr.ClosePath ();

            cr.SetSourceRGB (1, 0, 0);
            cr.Fill ();

            cr.Restore ();
        }

        private void drawUpTrap(ref Context cr, int x, int y, double scale) {
            cr.Save ();

            int sX, sY;

            if (scale > 1.0) {
                double scaleMul = ((scale - 1) * 100) / 2;
                sX = x - (int)scaleMul;
                sY = y - (int)scaleMul;
            } else {
                sX = x;
                sY = y;
            }

            double sc = 100 * scale;

            cr.MoveTo (sX, sY + sc);
            cr.LineTo (sX + sc, sY + sc);
            cr.LineTo (sX + (.75 * sc), sY);
            cr.LineTo (sX + (.25 * sc), sY);
            cr.MoveTo (sX, sY + sc);
            cr.ClosePath ();

            cr.SetSourceRGB (1, 0, 0);
            cr.Fill ();

            cr.Restore ();
        }

        private void drawDownTrap(ref Context cr, int x, int y, double scale) {
            cr.Save ();

            int sX, sY;

            if (scale > 1.0) {
                double scaleMul = ((scale - 1) * 100) / 2;
                sX = x - (int)scaleMul;
                sY = y - (int)scaleMul;
            } else {
                sX = x;
                sY = y;
            }

            double sc = 100 * scale;

            cr.MoveTo (sX, sY);
            cr.LineTo (sX + sc, sY);
            cr.LineTo (sX + (.75 * sc), sY + sc);
            cr.LineTo (sX + (.25 * sc), sY + sc);
            cr.MoveTo (sX, sY);
            cr.ClosePath ();

            cr.SetSourceRGB (1, 0, 0);
            cr.Fill ();

            cr.Restore ();
        }

        protected override bool OnButtonPressEvent (Gdk.EventButton evnt) {
            clickTimer.Enabled = true;
            clicked = true;

            clickX = (int)evnt.X;
            clickY = (int)evnt.Y;
            clickAction = 0;

            //Console.WriteLine ("click-" + evnt.X.ToString () + ":" + evnt.Y.ToString ());
            return true;
        }

        protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt) {
            clickTimer.Enabled = false;
            clicked = false;

            if (clickAction == 0) {
                if ((evnt.X <= (clickX + 25)) && (evnt.X >= (clickX - 25)) && (evnt.Y <= (clickY + 25)) && (evnt.Y >= (clickY - 25)))
                    clickAction = 100;
            }

            switch (clickAction) {
            case 1:
                Console.WriteLine ("drag right");
                break;
            case -1:
                Console.WriteLine ("drag left");
                break;
            case 2:
                Console.WriteLine ("drag down");
                break;
            case -2:
                Console.WriteLine ("drag up");
                break;
            case 100:
                Console.WriteLine ("click release");
                break;
            default:
                break;
            }

            //Console.WriteLine ("release-" + evnt.X.ToString () + ":" + evnt.Y.ToString ());
            return true;
        }

        protected void OnTimerEvent (object source, ElapsedEventArgs e) {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);

                if (clickAction == 0) {
                    if (x > (clickX + 25))
                        clickAction = 1;
                    else if (x < (clickX - 25))
                        clickAction = -1;
                    else if (y > (clickY + 25))
                        clickAction = 2;
                    else if (y < (clickY - 25))
                        clickAction = -2;
                }

                //Console.WriteLine ("drag-" + x.ToString () + ":" + y.ToString ());
            }
        }
    }
    */
}

