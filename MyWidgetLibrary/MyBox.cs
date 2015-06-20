using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class MyBox : EventBox
    {
        public string color;
        public float transparency;

        public MyBox (int width, int height) {
            Visible = true;
            VisibleWindow = false;

            WidthRequest = width;
            HeightRequest = height;
            this.color = "grey2";
            transparency = 0.55f;

            ExposeEvent += OnExpose;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                cr.Rectangle (Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);
                MyColor.SetSource (cr, color, transparency);
                cr.Fill ();
            }
        }
    }
}

