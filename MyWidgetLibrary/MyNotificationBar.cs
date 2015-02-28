using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class MyNotificationBar : DrawingArea
    {
        private uint updateTimer;

        public MyNotificationBar () {
            this.Visible = true;
            this.SetSizeRequest (1280, 18);

            this.updateTimer = GLib.Timeout.Add (1000, onTimer);

            this.ExposeEvent += onExpose;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            DrawingArea area = (DrawingArea)sender;
            using (Context cr = Gdk.CairoHelper.Create (area.GdkWindow)) {
                cr.MoveTo (0, 0);
                cr.LineTo (1280, 0);
                cr.LineTo (1280, 18);
                cr.LineTo (0, 18);
                cr.ClosePath ();

                cr.SetSourceRGB (0.15, 0.15, 0.15);
                cr.Fill ();

                Pango.Layout l = new Pango.Layout (this.PangoContext);
                l.Width = Pango.Units.FromPixels (100);
                l.Wrap = Pango.WrapMode.Word;
                l.Alignment = Pango.Alignment.Right;
                l.SetMarkup ("<span color=" + (char)34 + "white" + (char)34 + ">" + DateTime.Now.ToLongTimeString () + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 12");
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), 1180, 0, l);
                l.Dispose ();
            }
        }

        protected bool onTimer () {
            QueueDraw ();
            return true;
        }
    }
}

