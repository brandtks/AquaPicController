using System;
using Gtk;
using Cairo;
using System.Collections.Generic;

namespace MyWidgetLibrary
{
    public enum ledColor : byte {
        red = 1,
        green,
        blue,
        yellow
    }

    [System.ComponentModel.ToolboxItem (true)]
    public partial class MyLedWidget : Gtk.Bin
    {
        private Dictionary<ledColor, double[]> colors; 

        public ledColor color { get; set; }
        public bool onOff { get; set; }

        public MyLedWidget ()
        {
            colors = new Dictionary<ledColor, double[]> () {
                { ledColor.red, new double[3] { 2.55, 0.0, 0.0 } },
                { ledColor.green, new double[3] { 0.0, 2.55, 0.0 } },
                { ledColor.blue, new double[3] { 0.0, 0.0, 2.55 } },
                { ledColor.yellow, new double[3] { 2.55, 2.55, 0.0 } }
            };
            color = ledColor.red;
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

            double rCh, gCh, bCh;
            rCh = colors [color] [0];
            gCh = colors [color] [1];
            bCh = colors [color] [2];

            if (!onOff) {
                rCh *= 0.13;
                gCh *= 0.13;
                bCh *= 0.13;
            }

            cr.SetSourceRGB (rCh, gCh, bCh);
            cr.Rectangle (0, 0, width, height);
            cr.Fill ();

            cr.Dispose ();
        }
    }
}

