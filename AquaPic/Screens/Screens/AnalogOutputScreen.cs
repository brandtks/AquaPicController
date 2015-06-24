using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Drivers;
using AquaPic.Utilites;

namespace AquaPic
{
    public class AnalogOutputWindow : MyBackgroundWidget
    {
        TouchComboBox combo;
        int cardId;
        AnalogChannelDisplay[] displays;
        uint timerId;

        public AnalogOutputWindow (params object[] options) : base () {
            cardId = 0;

            MyBox box1 = new MyBox (780, 395);
            Put (box1, 10, 30);
            box1.Show ();

            displays = new AnalogChannelDisplay[4];
            for (int i = 0; i < 4; ++i) {
                displays [i] = new AnalogChannelDisplay ();
                displays [i].typeLabel.Visible = true;
                Put (displays [i], 20, 75 + (i * 60));
            }

            string[] names = AnalogOutput.GetAllCardNames ();
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
            float[] values = AnalogOutput.GetAllValues (cardId);

            int i = 0;
            foreach (var d in displays) {
                d.currentValue = values [i];
                d.QueueDraw ();

                ++i;
            }

            return true;
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int id = AnalogOutput.GetCardIndex (e.ActiveText);
            if (id != -1) {
                cardId = id;
            }
        }

        protected void GetCardData () {
            string[] names = AnalogOutput.GetAllChannelNames (cardId);
            float[] values = AnalogOutput.GetAllValues (cardId);
            AnalogType[] types = AnalogOutput.GetAllAnalogTypes (cardId);

            int i = 0;
            foreach (var d in displays) {
                d.label.text = names [i];
                d.currentValue = values [i];
                d.typeLabel.text = Utils.GetDescription (types [i]);
                d.QueueDraw ();

                ++i;
            }
        }
    }
}

