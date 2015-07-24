using System;
using System.IO;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Drivers;
using AquaPic.Modules;
using AquaPic.Runtime;
using AquaPic.SerialBus;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class PowerWindow : WindowBase
    {
        private PowerOutletSlider[] selectors;
        private int powerID;
        private TouchComboBox combo;

        public PowerWindow (params object[] options) : base () {
            MyBox box1 = new MyBox (780, 395);
            Put (box1, 10, 30);
            box1.Show ();

            powerID = 0;

            int x, y;
            IndividualControl ic;
            ic.Group = (byte)powerID;
            selectors = new PowerOutletSlider[8];
            for (int i = 0; i < 8; ++i) {
                selectors [i] = new PowerOutletSlider (i);

                selectors [i].ss.SelectorChangedEvent += OnSelectorChanged;

                if (i < 4) {
                    x = (i * 190) + 25;
                    y = 80;
                } else {
                    x = ((i - 4) * 190) + 25;
                    y = 250;
                }
                Put (selectors [i], x, y);

                selectors [i].Show ();

                ic.Individual = (byte)i;
                Power.AddHandlerOnStateChange (ic, PlugStateChange);
            }

            string[] pwrNames = Power.GetAllPowerStripNames ();
            combo = new TouchComboBox (pwrNames);
            combo.Active = powerID;
            combo.ChangedEvent += OnComboChanged;
            Put (combo, 610, 35);
            combo.Show ();

            GetPowerData ();

            ShowAll ();
        }

        public override void Dispose () {
            IndividualControl ic;
            ic.Group = (byte)powerID;
            for (int i = 0; i < selectors.Length; ++i) {
                ic.Individual = (byte)i;
                Power.RemoveHandlerOnStateChange (ic, PlugStateChange);
            }

            base.Dispose ();
        }

        protected void GetPowerData () {
            MyState[] states = Power.GetAllStates (powerID);
            Mode[] modes = Power.GetAllModes (powerID);
            string[] names = Power.GetAllOutletNames (powerID);

            int i = 0;
            foreach (var s in selectors) {
                s.OutletName.text = names [i];

                if (states [i] == MyState.On) {
                    s.Status.text = "On";
                    s.Status.textColor = "secb";
                } else {
                    s.Status.text = "Off";
                    s.Status.textColor = "grey4";
                }

                if (modes [i] == Mode.Auto) {
                    s.ss.CurrentSelected = 1;
                } else { // mode is manual
                    if (states [i] == MyState.On) {
                        s.ss.CurrentSelected = 2;
                    } else {
                        s.ss.CurrentSelected = 0;
                    }
                }
                ++i;
            }
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int id = Power.GetPowerStripIndex (e.ActiveText);
            if (id != -1) {
                IndividualControl ic;
                ic.Group = (byte)powerID; // old powerID
                for (int i = 0; i < selectors.Length; ++i) {
                    ic.Individual = (byte)i;
                    Power.RemoveHandlerOnStateChange (ic, PlugStateChange);
                }

                powerID = id;
                GetPowerData ();

                ic.Group = (byte)powerID;
                for (int i = 0; i < selectors.Length; ++i) {
                    ic.Individual = (byte)i;
                    Power.AddHandlerOnStateChange (ic, PlugStateChange);
                }
                QueueDraw ();
            }
        }

        protected void OnSelectorChanged (object sender, SelectorChangedEventArgs e) {
            var ss = sender as TouchSelectorSwitch;
            IndividualControl ic;
            ic.Group = (byte)powerID;
            ic.Individual = ss.Id;

            if (ss.CurrentSelected == 1) // auto
                Power.SetOutletMode (ic, Mode.Auto);
            else if (ss.CurrentSelected == 0) { // manual and state off
                Power.SetOutletMode (ic, Mode.Manual);
                Power.SetManualOutletState (ic, MyState.Off);
            } else if (ss.CurrentSelected == 2) {// manual and state on
                Power.SetOutletMode (ic, Mode.Manual);
                Power.SetManualOutletState (ic, MyState.On);
            }
        }

        protected void PlugStateChange (object sender, StateChangeEventArgs args) {
            if (args.powerID == powerID) {

                if (args.state == MyState.On) {
                    selectors [args.outletID].Status.text = "On";
                    selectors [args.outletID].Status.textColor = "secb";
                } else {
                    selectors [args.outletID].Status.text = "Off";
                    selectors [args.outletID].Status.textColor = "grey4";
                }
                   
                selectors [args.outletID].QueueDraw ();
            }
        }
    }
}

