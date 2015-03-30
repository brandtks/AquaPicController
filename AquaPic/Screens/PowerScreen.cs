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
        private TouchSelectorSwitch[] selectors;
        private int powerID;
        private TouchComboBox combo;

        public PowerWindow (ButtonReleaseEventHandler OnTouchButtonRelease) : base ("Power", OnTouchButtonRelease) {
            powerID = 0;

            int x, y;
            selectors = new TouchSelectorSwitch[8];
            for (int i = 0; i < 8; ++i) {
                selectors [i] = new TouchSelectorSwitch (i, 3, 0, MyOrientation.Horizontal);
                selectors [i].SelectorChanged += OnSelectorChanged;
                if (i < 4) {
                    x = (i * 100) + 200;
                    y = 100;
                } else {
                    x = ((i - 4) * 100) + 200;
                    y = 150;
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

            Show ();
        }

        protected void GetPowerData (int powerID) {
            MyState[] states = Power.GetAllStates (powerID);
            Mode[] modes = Power.GetAllModes (powerID);
            string[] names = Power.GetAllOutletNames (powerID);
            
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
                GetPowerData (powerID);
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
            else if (ss.CurrentSelected == 0) // manual and state off
                Power.SetManualOutletState (ic, MyState.Off);
            else if (ss.CurrentSelected == 2) // manual and state on
                Power.SetManualOutletState (ic, MyState.On);
        }

        protected void PlugStateChange (object sender, StateChangeEventArgs args) {
            if (args.powerID == powerID) {
                if (args.state == MyState.On) {
                    foreach (var color in selectors [args.outletID].BkgndColorOptions)
                        color.ChangeColor ("blue");
                } else {
                    foreach (var color in selectors [args.outletID].BkgndColorOptions)
                        color.ChangeColor (0.15, 0.15, 0.15);
                }
                selectors [args.outletID].QueueDraw ();
            }
        }
    }
}

