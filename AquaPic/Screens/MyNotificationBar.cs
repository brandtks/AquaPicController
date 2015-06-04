using System;
using Cairo;
using Gtk;
using MyWidgetLibrary;

namespace AquaPic
{
    public class MyNotificationBar : EventBox
    {
        public MyNotificationBar () {
            this.Visible = true;
            this.VisibleWindow = false;
            this.SetSizeRequest (800, 19);

            GLib.Timeout.Add (1000, onTimer);

            this.ExposeEvent += onExpose;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            var area = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (area.GdkWindow)) {
                cr.MoveTo (0, 0);
                cr.LineTo (800, 0);
                cr.LineTo (800, 17);
                cr.LineTo (0, 17);
                cr.MoveTo (0, 0);
                cr.ClosePath ();
                cr.SetSourceRGB (0.15, 0.15, 0.15);
                cr.Fill ();

                cr.MoveTo (0, 17);
                cr.LineTo (800, 17);
                cr.LineTo (800, 19);
                cr.LineTo (0, 19);
                cr.LineTo (0, 17);
                cr.ClosePath ();

                Gradient pat = new LinearGradient (0, 19, 800, 19);
                pat.AddColorStop (0.0, MyColor.NewGdkColor ("grey2", 0.35));
                pat.AddColorStop (0.5, GuiGlobal.screenData [GuiGlobal.currentScreen].color.ToGdkColor ());
                pat.AddColorStop (1.0, MyColor.NewGdkColor ("grey2", 0.35));
                cr.SetSource (pat);
                cr.Fill ();
                pat.Dispose ();

                Pango.Layout l = new Pango.Layout (this.PangoContext);
                l.Width = Pango.Units.FromPixels (120);
                l.Wrap = Pango.WrapMode.Word;
                l.Alignment = Pango.Alignment.Right;
                l.SetMarkup ("<span color=" + (char)34 + "white" + (char)34 + ">" + DateTime.Now.ToLongTimeString () + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), 680, 0, l);
                l.Dispose ();
            }
        }

        protected bool onTimer () {
            QueueDraw ();
            return true;
        }
    }
}

