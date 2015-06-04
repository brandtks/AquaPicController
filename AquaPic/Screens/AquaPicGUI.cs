using System;
using System.Diagnostics;
using Cairo;
using Gtk;
using MyWidgetLibrary;

namespace AquaPic
{
    public partial class AquaPicGUI : Gtk.Window
    {
        #if SIMULATION
        private Process simulator;
        #endif

        #if SIMULATION
        public AquaPicGUI (Process simulator) : base (Gtk.WindowType.Toplevel) {
        #else
        public AquaPicGUI () : base (Gtk.WindowType.Toplevel) {
        #endif
            this.Name = "AquaPic.GUI";
            this.Title = global::Mono.Unix.Catalog.GetString ("AquaPic Controller Version 1");
            this.WindowPosition = ((global::Gtk.WindowPosition)(4));
            this.DefaultWidth = 800;
            this.DefaultHeight = 480;
            this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
            this.Resizable = false;
            this.AllowGrow = false;

            #if SIMULATION
            this.simulator = simulator;
            #endif

            GuiGlobal.ChangeScreenEvent += ScreenChange;

            GuiGlobal.currentScreen = "Main";
            Add (GuiGlobal.screenData ["Main"].CreateInstance ());

            this.Show ();
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
            #if SIMULATION
            simulator.CloseMainWindow ();
            simulator.Close ();
            #endif

            Application.Quit ();
            a.RetVal = true;
        }

        public void ScreenChange (ScreenData screen) {
            if (GuiGlobal.currentScreen != screen.name) {
                GuiGlobal.currentScreen = screen.name;
                ClearChildren ();

                Add (screen.CreateInstance ());

                QueueDraw ();
            }
        }

        protected void ClearChildren () {
            foreach (Widget w in this.Children) {
                this.Remove (w);
                w.Dispose ();
            }
        }
    }
}

