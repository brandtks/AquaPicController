using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Drivers;
using AquaPic.Modules;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class PowerWindow : WindowBase
    {
        private PowerOutletSlider[] selectors;
        private int powerID;
        private TouchComboBox combo;

        public PowerWindow (params object[] options) : base () {
            //TouchGraphicalBox box1 = new TouchGraphicalBox (780, 395);
            //Put (box1, 10, 30);
            //box1.Show ();

            screenTitle = "Power Strip";

            if (Power.powerStripCount == 0) {
                powerID = -1;
                screenTitle = "No Power Strips Added";
                Show ();
                return;
            }

            powerID = 0;

            int x, y;
            IndividualControl ic;
            ic.Group = (byte)powerID;
            selectors = new PowerOutletSlider[8];
            for (int i = 0; i < 8; ++i) {
                selectors [i] = new PowerOutletSlider (i);
                selectors [i].ss.SelectorChangedEvent += OnSelectorChanged;

                int idx = i;
                selectors [i].UpdateScreen = () => {
                    IndividualControl indCont;
                    indCont.Group = powerID;
                    indCont.Individual = idx;
                    selectors [idx].OutletName.text = Power.GetOutletName (indCont);
                    Mode mode = Power.GetOutletMode (indCont);
                    MyState state = Power.GetOutletState (indCont);
                    if (mode  == Mode.Auto) {
                        selectors [idx].ss.CurrentSelected = 1;
                    } else { // mode is manual
                        if (state == MyState.On) {
                            selectors [idx].ss.CurrentSelected = 2;
                        } else {
                            selectors [idx].ss.CurrentSelected = 0;
                        }
                    }

                    selectors [idx].QueueDraw ();
                };

                if ((i % 2) == 0) { // even number top row
                    x = ((i - (i / 2)) * 185) + 50;
                    y = 105;
                } else {
                    x = (((i - (i / 2)) - 1) * 185) + 50;
                    y = 255;
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

            ExposeEvent += (o, args) => {
                using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                    TouchColor.SetSource (cr, "grey3", 0.75);

                    for (int i = 0; i < 3; ++i) {
                        cr.MoveTo (60 + (i * 185), 252.5);
                        cr.LineTo (220 + (i * 185), 252.5);
                        cr.ClosePath ();
                        cr.Stroke ();

                        cr.MoveTo (232.5+ (i * 185), 115);
                        cr.LineTo (232.5 + (i * 185), 385);
                        cr.ClosePath ();
                        cr.Stroke ();
                    }

                    cr.MoveTo (615, 252.5);
                    cr.LineTo (775, 252.5);
                    cr.ClosePath ();
                    cr.Stroke ();
                }
            };

            GetPowerData ();

            ShowAll ();
        }

        public override void Dispose () {
            if (powerID != -1) {
                IndividualControl ic;
                ic.Group = (byte)powerID;
                for (int i = 0; i < selectors.Length; ++i) {
                    ic.Individual = (byte)i;
                    Power.RemoveHandlerOnStateChange (ic, PlugStateChange);
                }
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
                Power.SetOutletManualState (ic, MyState.Off);
            } else if (ss.CurrentSelected == 2) {// manual and state on
                Power.SetOutletMode (ic, Mode.Manual);
                Power.SetOutletManualState (ic, MyState.On);
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

