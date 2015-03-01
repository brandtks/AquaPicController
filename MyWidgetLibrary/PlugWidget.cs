using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public partial class MyPlugWidget : EventBox //Gtk.Bin
    {
        public bool onOff { get; set; }
        public byte id { get; set; }
        public string PlugName { get; set; }

        public event ButtonPressEventHandler PlugClicked;

        public MyPlugWidget (int id) {
            this.Visible = true;
            this.VisibleWindow = false;

            this.onOff = false;
            this.id = (byte)id;
            this.PlugName = null;

            this.WidthRequest = 90;
            this.HeightRequest = 90;

            this.ExposeEvent += OnAreaExposeEvent;
            this.ButtonPressEvent += OnAreaButtonClickedEvent;

            //this.Build ();
        }

        protected void OnAreaExposeEvent (object o, ExposeEventArgs args) {
            //var area = o as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                int width = Allocation.Width;
                int height = Allocation.Height;
                int left = Allocation.Left;
                int top = Allocation.Top;

                if (onOff)
                    cr.SetSourceRGB (0.0, 2.55, 2.55);
                else
                    cr.SetSourceRGB (0.0, 0.45, 0.45);
                cr.Rectangle (left, top, width, height);
                cr.FillPreserve ();
                cr.LineWidth = 0.85;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();

                cr.SetSourceRGB (0.3, 0.3, 0.3);
                cr.Rectangle (left + 10, top + 10, width - 20, height - 20);
                cr.FillPreserve ();
                cr.LineWidth = 0.5;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();

                Pango.Layout l = new Pango.Layout (PangoContext);
                l.Width = Pango.Units.FromPixels (width - 10);
                l.Wrap = Pango.WrapMode.WordChar;
                l.Alignment = Pango.Alignment.Center;
                l.SetMarkup ("<span color=" + (char)34 + "white" + (char)34 + ">" + PlugName + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), left + 5, top + 10, l);
                l.Dispose ();
            }
        }

        protected void OnAreaButtonClickedEvent (object o, ButtonPressEventArgs args) { 
            if (PlugClicked != null)
                PlugClicked (this, args);
        }
    }
}

