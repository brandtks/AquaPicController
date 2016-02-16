using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Drivers;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class AnalogInputWindow : WindowBase
    {
        TouchComboBox combo;
        int cardId;
        AnalogChannelDisplay[] displays;
        uint timerId;

        public AnalogInputWindow (params object[] options) : base () {
            //TouchGraphicalBox box1 = new TouchGraphicalBox (730, 440);
            //Put (box1, 60, 30);
            //box1.Show ();

            //var l = new TouchLabel ();
            //l.text = "Analog Input Cards";
            //l.textColor = "pri";
            //l.textSize = 14;
            //l.textAlignment = TouchAlignment.Center;
            //l.WidthRequest = 780;
            //Put (l, 10, 36);
            //l.Show ();

            screenTitle = "Analog Inputs Cards";

            if (AquaPicDrivers.AnalogInput.cardCount == 0) {
                cardId = -1;
                screenTitle = "No Analog Input Cards Added";
                Show ();
                return;
            }

            cardId = 0;

            displays = new AnalogChannelDisplay[4];
            for (int i = 0; i < 4; ++i) {
                displays [i] = new AnalogChannelDisplay ();
                displays [i].divisionSteps = 4096;
                displays [i].ForceButtonReleaseEvent += OnForceRelease;
                displays [i].ValueChangedEvent += OnValueChanged;
                Put (displays [i], 70, 90 + (i * 75));
            }

            string[] names = AquaPicDrivers.AnalogInput.GetAllCardNames ();
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
            float[] values = AquaPicDrivers.AnalogInput.GetAllChannelValues (cardId);

            int i = 0;
            foreach (var d in displays) {
                d.currentValue = values [i];
                d.QueueDraw ();

                ++i;
            }

            return true;
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int id = AquaPicDrivers.AnalogInput.GetCardIndex (e.ActiveText);
            if (id != -1) {
                cardId = id;
                GetCardData ();
            }
        }

        protected void OnForceRelease (object sender, ButtonReleaseEventArgs args) {
            AnalogChannelDisplay d = sender as AnalogChannelDisplay;

            IndividualControl ic;
            ic.Group = (byte)cardId;
            ic.Individual = AquaPicDrivers.AnalogInput.GetChannelIndex (cardId, d.label.text);

            Mode m = AquaPicDrivers.AnalogInput.GetChannelMode (ic);

            if (m == Mode.Auto) {
                AquaPicDrivers.AnalogInput.SetChannelMode (ic, Mode.Manual);
                d.progressBar.enableTouch = true;
                d.textBox.enableTouch = true;
                d.button.buttonColor = "pri";
            } else {
                AquaPicDrivers.AnalogInput.SetChannelMode (ic, Mode.Auto);
                d.progressBar.enableTouch = false;
                d.textBox.enableTouch = false;
                d.button.buttonColor = "grey4";
            }

            d.QueueDraw ();
        }

        protected void OnValueChanged (object sender, float value) {
            AnalogChannelDisplay d = sender as AnalogChannelDisplay;

            IndividualControl ic;
            ic.Group = (byte)cardId;
            ic.Individual = AquaPicDrivers.AnalogInput.GetChannelIndex (cardId, d.label.text);

            Mode m = AquaPicDrivers.AnalogInput.GetChannelMode (ic);

            if (m == Mode.Manual)
                AquaPicDrivers.AnalogInput.SetChannelValue (ic, value);

            d.QueueDraw ();
        }

        protected void GetCardData () {
            string[] names = AquaPicDrivers.AnalogInput.GetAllChannelNames (cardId);
            float[] values = AquaPicDrivers.AnalogInput.GetAllChannelValues (cardId);
            Mode[] modes = AquaPicDrivers.AnalogInput.GetAllChannelModes (cardId);

            int i = 0;
            foreach (var d in displays) {
                d.label.text = names [i];
                d.currentValue = values [i];

                if (modes [i] == Mode.Auto) {
                    d.progressBar.enableTouch = false;
                    d.textBox.enableTouch = false;
                    d.button.buttonColor = "grey4";
                } else {
                    d.progressBar.enableTouch = true;
                    d.textBox.enableTouch = true;
                    d.button.buttonColor = "pri";
                }

                d.QueueDraw ();

                ++i;
            }
        }
    }
}

