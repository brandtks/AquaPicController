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
        private TouchSelectorSwitch[] selectors;
        private int powerID;
        private TouchComboBox combo;
        private TouchTextBox statusText;
        private TouchTextBox timeText;
        private TouchTextBox addressText;

        public PowerWindow (ButtonReleaseEventHandler OnTouchButtonRelease) : base ("Power", OnTouchButtonRelease) {
            powerID = 0;

            EventBox box = new EventBox ();
            box.Visible = true;
            box.VisibleWindow = false;
            box.ExposeEvent += OnBoxExpose;
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

            selectors = new TouchSelectorSwitch[8];
            for (int i = 0; i < selectors.Length; ++i) {
                selectors [i] = new TouchSelectorSwitch (i);
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

            string[] pwrNames = Power.GetAllPowerStripNames ();
            combo = new TouchComboBox (pwrNames);
            combo.Active = powerID;
            combo.Changed += OnComboChanged;
            Put (combo, 1040, 35);
            combo.Show ();

            statusText = new TouchTextBox ();
            statusText.WidthRequest = 250;
            Put (statusText, 1015, 580);
            statusText.Show ();

            var label1 = new TouchLabel ();
            label1.Text = "Status";
            label1.Justification = Justify.Right;
            label1.FontColor = new MyColor (255, 255, 255);
            Put (label1, 1013, 585);
            label1.Show ();

            timeText = new TouchTextBox ();
            Put (timeText, 1015, 620);
            timeText.Show ();

            var label2 = new TouchLabel ();
            label2.Text = "Response Time";
            label2.Justification = Justify.Right;
            label2.FontColor = new MyColor (255, 255, 255);
            Put (label2, 1013, 625);
            label2.Show ();

            addressText = new TouchTextBox ();
            addressText.Justification = Justify.Right;
            addressText.WidthRequest = 50;
            Put (addressText, 1215, 620);
            addressText.Show ();

            var label3 = new TouchLabel ();
            label3.Text = "Address";
            label3.Justification = Justify.Right;
            label3.FontColor = new MyColor (255, 255, 255);
            Put (label3, 1213, 625);
            label3.Show ();

            GetPowerData (powerID);

            Show ();
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
            Application.Quit ();
            a.RetVal = true;
        }

        protected void OnBoxExpose (object sender, ExposeEventArgs args) {
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

            Power.ManualSetPlugState (ic, s);
        }

        protected void PlugStateChange (object sender, StateChangeEventArgs args) {
            if (args.state == MyState.On)
                plugs [args.plugID].onOff = true;
            else
                plugs [args.plugID].onOff = false;

            plugs [args.plugID].QueueDraw ();
        }

        protected void OnSelectorChanged (object sender, SelectorChangedEventArgs args) {
            var sel = sender as TouchSelectorSwitch;

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
            string[] names = Power.GetAllPlugNames (powerID);
            string status = Power.GetApbStatus (powerID);
            int time = Power.GetApbResponseTime (powerID);
            int address = Power.GetApbAddress (powerID);
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

            statusText.Text = status;
            timeText.Text = time.ToString ();
            addressText.Text = address.ToString ();
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int id = Power.GetPowerStripIndex (e.ActiveText);
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

        protected void OnApbPowerStripStatusUpdate (object sender) {
            string status = Power.GetApbStatus (powerID);
            int time = Power.GetApbResponseTime (powerID);
            statusText.Text = status;
            timeText.Text = time.ToString ();
        }
    }
}

