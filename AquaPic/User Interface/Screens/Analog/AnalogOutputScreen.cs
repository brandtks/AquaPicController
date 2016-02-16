using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Drivers;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class AnalogOutputWindow : WindowBase
    {
        TouchComboBox combo;
        int cardId;
        AnalogChannelDisplay[] displays;
        uint timerId;

        public AnalogOutputWindow (params object[] options) : base () {
            //TouchGraphicalBox box1 = new TouchGraphicalBox (730, 440);
            //Put (box1, 60, 30);
            //box1.Show ();

            //var l = new TouchLabel ();
            //l.text = "Analog Output Cards";
            //l.textColor = "pri";
            //l.textSize = 14;
            //l.textAlignment = TouchAlignment.Center;
            //l.WidthRequest = 780;
            //Put (l, 10, 36);
            //l.Show ();

            screenTitle = "Analog Output Cards";

            if (AquaPicDrivers.AnalogOutput.cardCount == 0) {
                cardId = -1;
                screenTitle = "No Analog Output Cards Added";
                Show ();
                return;
            }

            cardId = 0;

            displays = new AnalogChannelDisplay[4];
            for (int i = 0; i < 4; ++i) {
                displays [i] = new AnalogChannelDisplay ();
                displays [i].typeLabel.Visible = true;
                displays [i].divisionSteps = 1024;
                displays [i].TypeSelectorChangedEvent += OnSelectorSwitchChanged;
                displays [i].ForceButtonReleaseEvent += OnForceRelease;
                displays [i].ValueChangedEvent += OnValueChanged;
                Put (displays [i], 70, 90 + (i * 75));
                displays [i].Show ();
            }

            string[] names = AquaPicDrivers.AnalogOutput.GetAllCardNames ();
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
            if (cardId != -1) {
                GLib.Source.Remove (timerId);
            }
            base.Dispose ();
        }

        protected bool OnUpdateTimer () {
            int[] values = AquaPicDrivers.AnalogOutput.GetAllChannelValues (cardId);

            int i = 0;
            foreach (var d in displays) {
                d.currentValue = (float)values [i];
                d.QueueDraw ();

                ++i;
            }

            return true;
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int id = AquaPicDrivers.AnalogOutput.GetCardIndex (e.ActiveText);
            if (id != -1) {
                cardId = id;
                GetCardData ();
            }
        }

        protected void OnForceRelease (object sender, ButtonReleaseEventArgs args) {
            AnalogChannelDisplay d = sender as AnalogChannelDisplay;

            IndividualControl ic;
            ic.Group = cardId;
            ic.Individual = AquaPicDrivers.AnalogOutput.GetChannelIndex (cardId, d.label.text);

            Mode m = AquaPicDrivers.AnalogOutput.GetChannelMode (ic);

            if (m == Mode.Auto) {
                AquaPicDrivers.AnalogOutput.SetChannelMode (ic, Mode.Manual);
                d.progressBar.enableTouch = true;
                d.textBox.enableTouch = true;
                d.button.buttonColor = "pri";
                d.ss.Visible = true;
                d.typeLabel.Visible = false;
            } else {
                AquaPicDrivers.AnalogOutput.SetChannelMode (ic, Mode.Auto);
                d.progressBar.enableTouch = false;
                d.textBox.enableTouch = false;
                d.button.buttonColor = "grey4";
                d.ss.Visible = false;
                d.typeLabel.Visible = true;
            }

            d.QueueDraw ();
        }

        protected void OnValueChanged (object sender, float value) {
            AnalogChannelDisplay d = sender as AnalogChannelDisplay;

            IndividualControl ic;
            ic.Group = (byte)cardId;
            ic.Individual = AquaPicDrivers.AnalogOutput.GetChannelIndex (cardId, d.label.text);

            Mode m = AquaPicDrivers.AnalogOutput.GetChannelMode (ic);

            if (m == Mode.Manual)
                AquaPicDrivers.AnalogOutput.SetChannelValue (ic, value.ToInt ());

            d.QueueDraw ();
        }

        protected void OnSelectorSwitchChanged (object sender, SelectorChangedEventArgs args) {
            AnalogChannelDisplay d = sender as AnalogChannelDisplay;

            IndividualControl ic = IndividualControl.Empty;
            ic.Group = cardId;
            ic.Individual = AquaPicDrivers.AnalogOutput.GetChannelIndex (cardId, d.label.text);

            if (args.currentSelectedIndex == 0) {
                AquaPicDrivers.AnalogOutput.SetChannelType (ic, AnalogType.ZeroTen);
            } else {
                AquaPicDrivers.AnalogOutput.SetChannelType (ic, AnalogType.PWM);
            }
        }

        protected void GetCardData () {
            string[] names = AquaPicDrivers.AnalogOutput.GetAllChannelNames (cardId);
            int[] values = AquaPicDrivers.AnalogOutput.GetAllChannelValues (cardId);
            AnalogType[] types = AquaPicDrivers.AnalogOutput.GetAllChannelTypes (cardId);
            Mode[] modes = AquaPicDrivers.AnalogOutput.GetAllChannelModes (cardId);

            int i = 0;
            foreach (var d in displays) {
                d.label.text = names [i];
                d.currentValue = values [i];
                d.typeLabel.text = Utils.GetDescription (types [i]);

                if (modes [i] == Mode.Auto) {
                    d.progressBar.enableTouch = false;
                    d.textBox.enableTouch = false;
                    d.button.buttonColor = "grey4";
                    d.ss.Visible = false;
                } else {
                    d.progressBar.enableTouch = true;
                    d.textBox.enableTouch = true;
                    d.button.buttonColor = "pri";
                    d.ss.Visible = true;

                    if (types [i] == AnalogType.ZeroTen)
                        d.ss.CurrentSelected = 0;
                    else
                        d.ss.CurrentSelected = 1;
                }

                d.QueueDraw ();

                ++i;
            }
        }
    }
}

