using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class MySideBar : EventBox
    {
        public MySideBar () {
            this.SetSizeRequest (325, 650);
            this.Visible = true;
            this.VisibleWindow = false;

            this.ExposeEvent += onExpose;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
//                cr.MoveTo (0, 0);
//                cr.CurveTo (106, 15, 212, 28, 318, 37);
//                cr.CurveTo (325, 250, 319, 450, 300, 650);
//                cr.LineTo (0, 650);
//                cr.LineTo (0, 0);
                cr.MoveTo (0, 150);
                cr.CurveTo (106, 165, 212, 178, 318, 187);
                cr.CurveTo (325, 400, 319, 600, 300, 800);
                cr.LineTo (0, 800);
                cr.LineTo (0, 150);
                cr.ClosePath ();
                cr.SetSourceRGBA (0, 0, 0, 0.5);
                cr.Fill ();
            }
        }
    }
}

