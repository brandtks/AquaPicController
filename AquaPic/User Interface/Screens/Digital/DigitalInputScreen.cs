using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Drivers;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class DigitalInputWindow : WindowBase
    {
        TouchComboBox combo;
        int cardId;
        uint timerId;

        DigitalDisplay[] displays;

        public DigitalInputWindow (params object[] options) : base () {
            //TouchGraphicalBox box1 = new TouchGraphicalBox (780, 395);
            //Put (box1, 10, 30);
            //box1.Show ();

            screenTitle = "Digital Input Cards";

            if (DigitalInput.cardCount == 0) {
                cardId = -1;
                screenTitle = "No Digital Input Cards Added";
                Show ();
                return;
            }

            cardId = 0;

            displays = new DigitalDisplay[6];

            for (int i = 0; i < 6; ++i) {
                displays [i] = new DigitalDisplay ();
                displays [i].ForceButtonReleaseEvent += OnForceRelease;
                displays [i].StateSelectedChangedEvent += OnSelectorChanged;
                int x, y;
                if (i < 3) {
                    x = (i * 150) + 175;
                    y = 125;
                } else {
                    x = ((i - 3) * 150) + 175;
                    y = 275;
                }
                Put (displays [i], x, y);
                displays [i].Show ();
            }

            string[] names = DigitalInput.GetAllCardNames ();
            combo = new TouchComboBox (names);
            combo.Active = cardId;
            combo.WidthRequest = 235;
            combo.ChangedEvent += OnComboChanged;
            Put (combo, 550, 35);
            combo.Show ();

            GetCardData ();

            timerId = GLib.Timeout.Add (1000, OnUpdateTimer);

            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        protected bool OnUpdateTimer () {
            bool[] states = DigitalInput.GetAllStates (cardId);

            for (int i = 0; i < states.Length; ++i) {
                if (states [i]) {
                    displays [i].textBox.bkgndColor = "pri";
                    displays [i].textBox.text = "Closed";
                } else {
                    displays [i].textBox.bkgndColor = "seca";
                    displays [i].textBox.text = "Open";
                }

                displays [i].textBox.QueueDraw ();
            }

            return true;
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int id = DigitalInput.GetCardIndex (e.ActiveText);
            if (id != -1) {
                cardId = id;
                GetCardData ();
            }
        }

        protected void OnForceRelease (object sender, ButtonReleaseEventArgs args) {
            DigitalDisplay d = sender as DigitalDisplay;

            IndividualControl ic;
            ic.Group = (byte)cardId;
            ic.Individual = DigitalInput.GetInputIndex (cardId, d.label.text);

            Mode m = DigitalInput.GetMode (ic);

            if (m == Mode.Auto) {
                DigitalInput.SetMode (ic, Mode.Manual);
                d.selector.Visible = true;
                d.button.buttonColor = "pri";
            } else {
                DigitalInput.SetMode (ic, Mode.Auto);
                d.selector.Visible = false;
                d.button.buttonColor = "grey4";
            }

            d.QueueDraw ();
        }

        protected void OnSelectorChanged (object sender, SelectorChangedEventArgs args) {
            DigitalDisplay d = sender as DigitalDisplay;

            IndividualControl ic;
            ic.Group = (byte)cardId;
            ic.Individual = DigitalInput.GetInputIndex (cardId, d.label.text);

            bool s = DigitalInput.GetState (ic);

            if (s) {
                DigitalInput.SetState (ic, false);
                d.textBox.bkgndColor = "pri";
                d.textBox.text = "Closed";
            } else {
                DigitalInput.SetState (ic, true);
                d.textBox.bkgndColor = "seca";
                d.textBox.text = "Open";
            }

            d.QueueDraw ();
        }

        protected void GetCardData () {
            bool[] states = DigitalInput.GetAllStates (cardId);
            Mode[] modes = DigitalInput.GetAllModes (cardId);
            string[] names = DigitalInput.GetAllInputNames (cardId);

            int i = 0;
            foreach (var d in displays) {
                d.label.text = names [i];

                if (states [i]) {
                    d.textBox.bkgndColor = "pri";
                    d.textBox.text = "Closed";
                    d.selector.CurrentSelected = 1;
                } else {
                    d.textBox.bkgndColor = "seca";
                    d.textBox.text = "Open";
                    d.selector.CurrentSelected = 0;
                }

                if (modes [i] == Mode.Auto) {
                    d.selector.Visible = false;
                    d.button.buttonColor = "grey4";
                } else {
                    d.selector.Visible = true;
                    d.button.buttonColor = "pri";
                }

                d.QueueDraw ();

                ++i;
            }
        }
    }
}

