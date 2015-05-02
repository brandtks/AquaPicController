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
        private PowerOutletSlider[] selectors;
        private int powerID;
        private TouchComboBox combo;

        public PowerWindow (MenuReleaseHandler OnMenuRelease) : base (1, OnMenuRelease) {
            powerID = 0;

            int x, y;
            selectors = new PowerOutletSlider[8];
            for (int i = 0; i < 8; ++i) {
                selectors [i] = new PowerOutletSlider (i);

                selectors [i].SelectorChanged += OnSelectorChanged;

                if (i < 4) {
                    x = (i * 180) + 40;
                    y = 140;
                } else {
                    x = ((i - 4) * 180) + 40;
                    y = 200;
                }
                Put (selectors [i], x, y);

                selectors [i].Show ();
            }

            string[] pwrNames = Power.GetAllPowerStripNames ();
            combo = new TouchComboBox (pwrNames);
            combo.Active = powerID;
            combo.Changed += OnComboChanged;
            Put (combo, 620, 25);
            combo.Show ();

            GetPowerData ();

            Show ();
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
                    selector.StatusColor.ChangeColor("secb");
                } else {
                    selector.Status = "Off";
                    selector.StatusColor.ChangeColor("grey4");
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
                if (args.mode == Mode.Auto) {
                    if (args.state == MyState.On) {
                        selectors [args.outletID].SliderColorOptions [1].ChangeColor ("pri");
                    } else {
                        selectors [args.outletID].SliderColorOptions [1].ChangeColor ("grey4");
                    }
                }
                selectors [args.outletID].QueueDraw ();
            }
        }
    }
}

