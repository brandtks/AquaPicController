using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class MyBox : EventBox
    {
        public string color;

        public MyBox (int width, int height) {
            Visible = true;
            VisibleWindow = false;

            WidthRequest = width;
            HeightRequest = height;
            this.color = "grey1";

            ExposeEvent += OnExpose;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                cr.Rectangle (Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);
                MyColor.SetSource (cr, color, 0.55);
                cr.Fill ();
            }
        }
    }
}

