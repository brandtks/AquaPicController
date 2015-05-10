using System;
using System.Diagnostics;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public delegate void ValueSetHandler (string value);

    public class TouchValueInput_osk : Gtk.Window
    {
        public ValueSetHandler valueSetter;
        private Process osk;
        private Entry e;

        public TouchValueInput_osk (ValueSetHandler valueSetter) : base (Gtk.WindowType.Toplevel) {
            Name = "Touch Numeric Input";
            Title = "Numerical Input";
            WindowPosition = (Gtk.WindowPosition)3;
            DefaultWidth = 800;
            DefaultHeight = 480;
            Resizable = false;
            AllowGrow = false;

            DeleteEvent += (sender, a) => {
                osk.CloseMainWindow ();
                osk.Close ();
            };

            Destroyed += (sender, a) => {
                osk.CloseMainWindow ();
                osk.Close ();
            };

            this.valueSetter = valueSetter;

            Fixed f = new Fixed ();
            f.WidthRequest = 800;
            f.HeightRequest = 480;

            e = new Entry (0);
            e.WidthRequest = 800;
            e.HeightRequest = 30;
            e.CanFocus = true;
            e.Activated += OnActivate;
            f.Put (e, 0, 0);
            e.Show ();

            osk = Process.Start ("osk.exe");

            Add (f);
            f.ShowAll ();
            e.GrabFocus ();
            Show ();
        }

        protected void OnActivate (object sender, EventArgs a) {
            if (valueSetter != null)
                valueSetter (e.Text);

            Destroy ();
        }
    }
}

