using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class MyNotificationBar : EventBox
    {
        private uint updateTimer;

        public MyNotificationBar () {
            this.Visible = true;
            this.VisibleWindow = false;
            this.SetSizeRequest (1280, 20);

            this.updateTimer = GLib.Timeout.Add (1000, onTimer);

            this.ExposeEvent += onExpose;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            var area = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (area.GdkWindow)) {
                cr.MoveTo (0, 0);
                cr.LineTo (1280, 0);
                cr.LineTo (1280, 18);
                cr.LineTo (0, 18);
                cr.MoveTo (0, 0);
                cr.ClosePath ();
                cr.SetSourceRGB (0.15, 0.15, 0.15);
                cr.Fill ();

                cr.MoveTo (0, 18);
                cr.LineTo (1280, 18);
                cr.LineTo (1280, 20);
                cr.LineTo (0, 20);
                cr.LineTo (0, 18);
                cr.ClosePath ();

                Gradient pat = new LinearGradient (0, 19, 1280, 19);
                pat.AddColorStop (0.0, new Color (0.15, 0.15, 0.15, 0.15));
                pat.AddColorStop (0.5, new Color (0.95, 0.95, 0.95, 0.95));
                pat.AddColorStop (1.0, new Color (0.15, 0.15, 0.15, 0.15));
                cr.SetSource (pat);
                cr.Fill ();
                pat.Dispose ();

                Pango.Layout l = new Pango.Layout (this.PangoContext);
                l.Width = Pango.Units.FromPixels (120);
                l.Wrap = Pango.WrapMode.Word;
                l.Alignment = Pango.Alignment.Right;
                l.SetMarkup ("<span color=" + (char)34 + "white" + (char)34 + ">" + DateTime.Now.ToLongTimeString () + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 12");
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), 1160, 0, l);
                l.Dispose ();
            }
        }

        protected bool onTimer () {
            QueueDraw ();
            return true;
        }
    }
}

