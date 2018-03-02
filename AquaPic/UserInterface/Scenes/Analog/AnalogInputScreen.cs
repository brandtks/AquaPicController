#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using Gtk;
using GoodtimeDevelopment.TouchWidget;
using AquaPic.Drivers;
using AquaPic.Globals;

namespace AquaPic.UserInterface
{
    public class AnalogInputWindow : SceneBase
    {
        TouchComboBox combo;
        int cardId;
        AnalogChannelDisplay[] displays;

        public AnalogInputWindow (params object[] options) : base () {
            sceneTitle = "Analog Inputs Cards";

            if (AquaPicDrivers.AnalogInput.cardCount == 0) {
                cardId = -1;
                sceneTitle = "No Analog Input Cards Added";
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
            combo.activeIndex = cardId;
            combo.WidthRequest = 235;
            combo.ComboChangedEvent += OnComboChanged;
            Put (combo, 550, 35);
            combo.Show ();

            GetCardData ();
            Show ();
        }

        protected override bool OnUpdateTimer () {
            if (cardId == -1) {
                return false;
            }

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
            int id = AquaPicDrivers.AnalogInput.GetCardIndex (e.activeText);
            if (id != -1) {
                cardId = id;
                GetCardData ();
            }
        }

        protected void OnForceRelease (object sender, ButtonReleaseEventArgs args) {
            var d = sender as AnalogChannelDisplay;

            var ic = IndividualControl.Empty;
            ic.Group = cardId;
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
            var d = sender as AnalogChannelDisplay;

            var ic = IndividualControl.Empty;
            ic.Group = cardId;
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

