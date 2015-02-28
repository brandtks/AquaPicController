using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public partial class AquaPicGUI : Gtk.Window
    {
        private PowerWindow powerScreen;
        private MainWindow mainScreen;
        private string currentScreen;

        public AquaPicGUI () : base (Gtk.WindowType.Toplevel) {
            this.Name = "AquaPic.GUI";
            this.Title = global::Mono.Unix.Catalog.GetString ("GUI");
            this.WindowPosition = ((global::Gtk.WindowPosition)(4));
            this.DefaultWidth = 1280;
            this.DefaultHeight = 800;
            this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
            this.Resizable = false;
            this.AllowGrow = false;

            this.mainScreen = new MainWindow (OnButtonTouch);
            this.currentScreen = "Main";

            this.Add (mainScreen);

            this.Show ();
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
            Application.Quit ();
            a.RetVal = true;
        }

        protected void OnButtonTouch (object sender, ButtonReleaseEventArgs args) {
            var b = sender as TouchButton;

            if (string.Compare (b.Text, currentScreen, StringComparison.InvariantCultureIgnoreCase) != 0) {
                switch (b.Text) {
                case "Main":
                    ClearChildren ();
                    mainScreen = new MainWindow (OnButtonTouch);
                    currentScreen = "Main";
                    Add (mainScreen);
                    break;
                case "Power":
                    ClearChildren ();
                    powerScreen = new PowerWindow (OnButtonTouch);
                    currentScreen = "Power";
                    Add (powerScreen);
                    break;
                default:
                    break;
                }

                this.QueueDraw ();
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

