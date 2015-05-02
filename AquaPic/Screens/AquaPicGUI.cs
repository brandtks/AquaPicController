using System;
using System.Diagnostics;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public partial class AquaPicGUI : Gtk.Window
    {
        private PowerWindow powerScreen;
        private MainWindow mainScreen;
        private LightingWindow lighingScreen;
        private WaveWindow waveScreen;
        private ConditionWindow conditionScreen;
        private SettingsWindow settingsScreen;
        private int currentScreen;

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

            this.mainScreen = new MainWindow (OnMenuRelease);
            this.currentScreen = 0;

            this.Add (mainScreen);

//            powerScreen = new PowerWindow (OnButtonTouch);
//            currentScreen = "Power";
//            Add (powerScreen);

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

        protected void OnMenuRelease (int screenKey) {
            if (screenKey != currentScreen) {
                ClearChildren ();
                currentScreen = screenKey;

                switch (currentScreen) {
                case 0:
                    mainScreen = new MainWindow (OnMenuRelease);
                    Add (mainScreen);
                    break;
                case 1:
                    powerScreen = new PowerWindow (OnMenuRelease);
                    Add (powerScreen);
                    break;
                case 2:
                    lighingScreen = new LightingWindow (OnMenuRelease);
                    Add (lighingScreen);
                    break;
                case 3:
                    waveScreen = new WaveWindow (OnMenuRelease);
                    Add (waveScreen);
                    break;
                case 4:
                    conditionScreen = new ConditionWindow (OnMenuRelease);
                    Add (conditionScreen);
                    break;
                case 5:
                    settingsScreen = new SettingsWindow (OnMenuRelease);
                    Add (settingsScreen);
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

