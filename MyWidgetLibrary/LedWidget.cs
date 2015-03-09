using System;
using Gtk;
using Cairo;
using System.Collections.Generic;

namespace MyWidgetLibrary
{

    [System.ComponentModel.ToolboxItem (true)]
    public partial class MyLedWidget : Gtk.Bin
    {

        public MyColor color { get; set; }
        public bool onOff { get; set; }

        public MyLedWidget ()
        {
            color = new MyColor ("red");
            onOff = false;

            this.Build ();
        }

        protected void OnAreaExposeEvent (object o, Gtk.ExposeEventArgs args)
        {
            DrawingArea area = (DrawingArea) o;
            Cairo.Context cr =  Gdk.CairoHelper.Create(area.GdkWindow);

            int width, height;

            width = Allocation.Width;
            height = Allocation.Height;

            if (!onOff)
                color.ModifyColor (0.13);
            else
                color.ModifyColor (6.0);

            cr.SetSourceRGB (color.R, color.G, color.B);
            cr.Rectangle (0, 0, width, height);
            cr.Fill ();

            cr.Dispose ();
        }
    }
}

