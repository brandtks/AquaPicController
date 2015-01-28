using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public partial class mainWindow : Gtk.Window
    {
        private MyBackgroundWidget fix;

        public mainWindow () : base (Gtk.WindowType.Toplevel) {
            this.Name = "AquaPic.MainWindow";
            this.Title = global::Mono.Unix.Catalog.GetString ("Main");
            this.WindowPosition = ((global::Gtk.WindowPosition)(4));
            this.DefaultWidth = 1280;
            this.DefaultHeight = 800;
            this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
            this.Resizable = false;
            this.AllowGrow = false;

            fix = new MyBackgroundWidget (this);

            this.Child = fix;
            this.Child.Show ();

            this.Show ();
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
            Application.Quit ();
            a.RetVal = true;
        }
    }
}

