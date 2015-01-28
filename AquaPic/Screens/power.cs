using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using System.IO;

namespace AquaPic
{
    public partial class powerWindow : Window
    {
        private MyLedWidget[] leds;
        private MyPlugWidget[] plugs;
        private MyBackgroundWidget fix;

        public powerWindow () : base (Gtk.WindowType.Toplevel) { 
            this.Name = "AquaPic.PowerWindow";
            this.Title = global::Mono.Unix.Catalog.GetString ("power");
            this.WindowPosition = ((global::Gtk.WindowPosition)(4));
            this.DefaultWidth = 1280;
            this.DefaultHeight = 800;
            this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
            this.Resizable = false;
            this.AllowGrow = false;

            fix = new MyBackgroundWidget (this);

            DrawingArea box = new DrawingArea ();
            box.ExposeEvent += onBoxExpose;
            box.SetSizeRequest (600, 270);
            fix.Put (box, 555, 390);
            box.Show ();

            int x, y;
            plugs = new MyPlugWidget[8];
            for (int i = 0; i < 8; ++i) {
                plugs [i] = new MyPlugWidget ((byte)i);
                plugs [i].PlugClicked += plugClick;

                if (i < 4) {
                    x = (i * 100) + 755;
                    y = 400;
                } else {
                    x = ((i - 4) * 100) + 755;
                    y = 560;
                }

                fix.Put (plugs [i], x, y);
                plugs [i].Show ();
            }

            leds = new MyLedWidget[8];
            for (int i = 0; i < 8; ++i) {
                leds [i] = new MyLedWidget ();
                leds [i].color = ledColor.green;
                leds [i].onOff = false;

                x = 730;
                y = (i * 30) + 415;

                fix.Put (leds [i], x, y);
                leds [i].Show ();
            }

            this.Child = fix;
            this.Child.Show ();

            this.Show ();
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
            Application.Quit ();
            a.RetVal = true;
        }

        protected void onBoxExpose (object sender, ExposeEventArgs args) {
            DrawingArea area = (DrawingArea)sender;

            using (Context cr = Gdk.CairoHelper.Create (area.GdkWindow)) {
                cr.SetSourceRGB(0.15, 0.15, 0.15);
                cr.Rectangle (0, 0, 600, 270);
                cr.Fill ();
            }
        }

        protected void plugClick (object o, ButtonPressEventArgs args) {
            MyPlugWidget plug = (MyPlugWidget)o;
            plug.onOff = !plug.onOff;
            int idx = plug.id;

            if (!leds [idx].onOff) {
                leds [idx].onOff = true;
                leds [idx].QueueDraw ();
            }

            plug.QueueDraw ();
        }
    }
}

