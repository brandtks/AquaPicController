using System;
using System.IO;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.PowerDriver;
using AquaPic.Utilites;

namespace AquaPic
{
    public class PowerWindow : MyBackgroundWidget
    {
        private PowerOutletSlider[] selectors;
        private int powerID;
        private TouchComboBox combo;

        public PowerWindow (MenuReleaseHandler OnMenuRelease) : base (1, OnMenuRelease) {
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

                selectors [i].SelectorChangedEvent += OnSelectorChanged;

                if (i < 4) {
                    x = (i * 190) + 30;
                    y = 155;
                } else {
                    x = ((i - 4) * 190) + 30;
                    y = 235;
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

            Show ();
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
            foreach (var selector in selectors) {
                selector.OutletName = names [i];

                if (states [i] == MyState.On) {
                    selector.Status = "On";
                    selector.StatusColor = "secb";
                } else {
                    selector.Status = "Off";
                    selector.StatusColor = "grey4";
                }

                if (modes [i] == Mode.Auto) {
                    selector.CurrentSelected = 1;
                } else { // mode is manual
                    if (states [i] == MyState.On) {
                        selector.CurrentSelected = 2;
                    } else {
                        selector.CurrentSelected = 0;
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
                    selectors [args.outletID].Status = "On";
                    selectors [args.outletID].StatusColor = "secb";
                } else {
                    selectors [args.outletID].Status = "Off";
                    selectors [args.outletID].StatusColor = "grey4";
                }

                // have to call QueueDrawArea because there is text that needs to be draw
                // outside the widgets allocated area
                selectors [args.outletID].QueueDrawArea (
                    Allocation.Left, 
                    Allocation.Top - 10, 
                    Allocation.Width, 
                    Allocation.Height + 10);
            }
        }
    }
}

