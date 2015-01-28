using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    [System.ComponentModel.ToolboxItem (true)]
    public partial class MyPlugWidget : Gtk.Bin
    {
        public bool onOff { get; set; }
        public byte id { get; set; }

        public event ButtonPressEventHandler PlugClicked;

        public MyPlugWidget (byte id) {
            this.onOff = false;
            this.id = id;
            this.Build ();
        }

        protected void OnAreaExposeEvent (object o, ExposeEventArgs args) {
            DrawingArea area = (DrawingArea) o;
            Context cr =  Gdk.CairoHelper.Create(area.GdkWindow);

            int width, height;

            width = Allocation.Width;
            height = Allocation.Height;

            if (onOff)
                cr.SetSourceRGB (0.0, 2.55, 2.55);
            else
                cr.SetSourceRGB (0.0, 0.45, 0.45);
            cr.Rectangle (0, 0, width, height);
            cr.Fill ();

            cr.SetSourceRGB (0.3, 0.3, 0.3);
            cr.Rectangle (10, 10, width - 20, height - 20);
            cr.Fill (); 

            cr.Dispose ();
        }

        protected void OnAreaButtonClickedEvent (object o, ButtonPressEventArgs args) { 
            if (PlugClicked != null)
                PlugClicked (this, args);
        }
    }
}

