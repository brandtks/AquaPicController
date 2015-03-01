using System;
using System.IO;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.PowerDriver;
using AquaPic.Globals;

namespace AquaPic
{
    public partial class PowerWindow : MyBackgroundWidget
    {
        private MyPlugWidget[] plugs;
        private SelectorSwitch[] selectors;
        private int powerID;
        private ComboBox combo;

        public PowerWindow (ButtonReleaseEventHandler OnTouchButtonRelease) : base ("Power", OnTouchButtonRelease) {
            powerID = 0;

            EventBox box = new EventBox ();
            box.Visible = true;
            box.VisibleWindow = false;
            box.ExposeEvent += onBoxExpose;
            box.SetSizeRequest (600, 270);
            Put (box, 340, 265);
            box.Show ();

            int x, y;
            plugs = new MyPlugWidget[8];
            for (int i = 0; i < plugs.Length; ++i) { 
                plugs [i] = new MyPlugWidget ((byte)i);
                plugs [i].PlugClicked += this.PlugClick;

                if (i < 4) {
                    x = (i * 100) + 540;
                    y = 275;
                } else {
                    x = ((i - 4) * 100) + 540;
                    y = 435;
                }

                Put (plugs [i], x, y);
                plugs [i].Show ();
            }

            selectors = new SelectorSwitch[8];
            for (int i = 0; i < selectors.Length; ++i) {
                selectors [i] = new SelectorSwitch (i);
                selectors [i].AddSelectedColorOption (0, 0.50, 0.50, 0.50);
                selectors [i].AddSelectedColorOption (1, 0.35, 0.45, 0.95);
                selectors [i].AddSelectedNameOption (0, "Manual");
                selectors [i].AddSelectedNameOption (1, "Auto");
                selectors [i].SelectorChanged += OnSelectorChanged;

                if (i < 4) {
                    x = (i * 100) + 545;
                    y = 375;
                } else {
                    x = ((i - 4) * 100) + 545;
                    y = 405;
                }

                Put (selectors [i], x, y);
                selectors [i].Show ();
            }

            string[] pwrNames = Power.GetPowerStripNames ();
            combo = new ComboBox (pwrNames);
            combo.Active = powerID;
            combo.Changed += OnComboChanged;
            Put (combo, 50, 50);
            combo.Show ();

            GetPowerData (powerID);

            Show ();
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
            Application.Quit ();
            a.RetVal = true;
        }

        protected void onBoxExpose (object sender, ExposeEventArgs args) {
            var area = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (area.GdkWindow)) {;
                cr.SetSourceRGB(0.15, 0.15, 0.15);
                cr.Rectangle (340, 265, 600, 270);
                cr.FillPreserve ();
                cr.LineWidth = 1;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();
            }
        }

        protected void PlugClick (object o, ButtonPressEventArgs args) {
            MyPlugWidget plug = (MyPlugWidget)o;

            IndividualControl ic;
            MyState s;
            ic.Group = (byte)powerID;
            ic.Individual = plug.id;
            if (!plug.onOff)
                s = MyState.On;
            else
                s = MyState.Off;

            Power.GuiSetPlugState (ic, s);
        }

        protected void PlugStateChange (object sender, StateChangeEventArgs args) {
            if (args.state == MyState.On)
                plugs [args.plugID].onOff = true;
            else
                plugs [args.plugID].onOff = false;

            plugs [args.plugID].QueueDraw ();
        }

        protected void OnSelectorChanged (object sender, SelectorChangedEventArgs args) {
            var sel = sender as SelectorSwitch;

            IndividualControl ic;
            Mode m;
            ic.Group = (byte)powerID;
            ic.Individual = sel.id;
            if (sel.currentSelected == 1)
                m = Mode.Auto;
            else
                m = Mode.Manual;

            Power.SetPlugMode (ic, m);
        }

        protected void GetPowerData (int powerID) {
            MyState[] states = Power.GetAllStates (powerID);
            Mode[] modes = Power.GetAllModes (powerID);
            string[] names = Power.GetAllNames (powerID);
            IndividualControl ic;
            ic.Group = (byte)powerID;

            for (int i = 0; i < plugs.Length; ++i) {
                if (states [i] == MyState.On)
                    plugs [i].onOff = true;
                else
                    plugs [i].onOff = false;

                if (modes [i] == Mode.Auto)
                    selectors [i].currentSelected = 1;
                else
                    selectors [i].currentSelected = 0;

                if (string.IsNullOrWhiteSpace (names [i]))
                    plugs [i].PlugName = (i + 1).ToString ();
                else
                    plugs [i].PlugName = names [i];

                ic.Individual = (byte)i;
                Power.AddHandlerOnStateChange (ic, PlugStateChange);
            }
        }

        protected void OnComboChanged (object sender, EventArgs e) {
            TreeIter iter;
            string name = "";
            int id;

            if (combo.GetActiveIter (out iter)) {
                name = (string)combo.Model.GetValue (iter, 0);
                id = Power.GetPowerStripIndex (name);
                if (id != -1) {
                    powerID = id;
                    GetPowerData (powerID);
                    QueueDraw ();
                    combo.QueueDraw ();
                    for (int i = 0; i < 8; ++i) {
                        plugs [i].QueueDraw ();
                        selectors [i].QueueDraw ();
                    }
                }
            }
        }
    }
}

