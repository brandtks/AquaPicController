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
        public string PlugName { get; set; }

        public event ButtonPressEventHandler PlugClicked;

        public MyPlugWidget (byte id) {
            this.onOff = false;
            this.id = id;
            this.PlugName = null;
            this.Build ();
        }

        protected void OnAreaExposeEvent (object o, ExposeEventArgs args) {
            DrawingArea area = (DrawingArea) o;
            Context cr =  Gdk.CairoHelper.Create(area.GdkWindow);

            int width, height, left, top;

            width = Allocation.Width;
            height = Allocation.Height;
            left = Allocation.Left;
            top = Allocation.Top;

            if (onOff)
                cr.SetSourceRGB (0.0, 2.55, 2.55);
            else
                cr.SetSourceRGB (0.0, 0.45, 0.45);
            cr.Rectangle (0, 0, width, height);
            cr.Fill ();

            cr.SetSourceRGB (0.3, 0.3, 0.3);
            cr.Rectangle (10, 10, width - 20, height - 20);
            cr.Fill ();

            /*
            Pango.Layout l = new Pango.Layout (area.PangoContext);
            l.Width = Pango.Units.FromPixels (width);
            l.Wrap = Pango.WrapMode.Word;
            l.Alignment = Pango.Alignment.Left;
            //l.SetText (ButtonLabel);
            l.SetMarkup ("<span color=" + (char)34 + "white" + (char)34 + ">" + PlugName + "</span>"); 
            l.FontDescription = Pango.FontDescription.FromString ("Courier New 12");
            GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), 1, 1, l);
            l.Dispose ();
            */

            cr.SetSourceRGB (1.0, 1.0, 1.0);
            cr.SelectFontFace ("Courier New 12", FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize (12);
            cr.MoveTo (5, 12);
            cr.ShowText (PlugName);

            cr.Dispose ();
        }

        protected void OnAreaButtonClickedEvent (object o, ButtonPressEventArgs args) { 
            if (PlugClicked != null)
                PlugClicked (this, args);
        }
    }
}

