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
using GoodtimeDevelopment.Utilites;
using AquaPic.Drivers;
using AquaPic.Globals;

namespace AquaPic.UserInterface
{
    public class AnalogOutputWindow : SceneBase
    {
        TouchComboBox combo;
        int cardId;
        AnalogChannelDisplay[] displays;

        public AnalogOutputWindow (params object[] options) : base () {
            sceneTitle = "Analog Output Cards";

            if (AquaPicDrivers.AnalogOutput.cardCount == 0) {
                cardId = -1;
                sceneTitle = "No Analog Output Cards Added";
                Show ();
                return;
            }

            cardId = 0;

            displays = new AnalogChannelDisplay[4];
            for (int i = 0; i < 4; ++i) {
                displays [i] = new AnalogChannelDisplay ();
                displays [i].divisionSteps = 1000;
                displays [i].typeLabel.Visible = true;
                displays [i].ForceButtonReleaseEvent += OnForceRelease;
                displays [i].ValueChangedEvent += OnValueChanged;
				displays[i].settingsButton.buttonColor = "grey1";
                Put (displays [i], 70, 90 + (i * 75));
                displays [i].Show ();
            }

            string[] names = AquaPicDrivers.AnalogOutput.GetAllCardNames ();
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

            var values = AquaPicDrivers.AnalogOutput.GetAllChannelValues (cardId);

            int i = 0;
            foreach (var d in displays) {
                d.currentValue = values [i];
                d.QueueDraw ();

                ++i;
            }

            return true;
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int id = AquaPicDrivers.AnalogOutput.GetCardIndex (e.activeText);
            if (id != -1) {
                cardId = id;
                GetCardData ();
            }
        }

        protected void OnForceRelease (object sender, ButtonReleaseEventArgs args) {
            var d = sender as AnalogChannelDisplay;

            var ic = IndividualControl.Empty;
            ic.Group = cardId;
            ic.Individual = AquaPicDrivers.AnalogOutput.GetChannelIndex (cardId, d.label.text);

            var m = AquaPicDrivers.AnalogOutput.GetChannelMode (ic);

            if (m == Mode.Auto) {
                AquaPicDrivers.AnalogOutput.SetChannelMode (ic, Mode.Manual);
                d.progressBar.enableTouch = true;
                d.textBox.enableTouch = true;
                d.forceButton.buttonColor = "pri";
                d.typeLabel.Visible = false;
            } else {
                AquaPicDrivers.AnalogOutput.SetChannelMode (ic, Mode.Auto);
                d.progressBar.enableTouch = false;
                d.textBox.enableTouch = false;
                d.forceButton.buttonColor = "grey4";
                d.typeLabel.Visible = true;
            }

            d.QueueDraw ();
        }

        protected void OnValueChanged (object sender, float value) {
            var d = sender as AnalogChannelDisplay;

            var ic = IndividualControl.Empty;
            ic.Group = cardId;
            ic.Individual = AquaPicDrivers.AnalogOutput.GetChannelIndex (cardId, d.label.text);

            var m = AquaPicDrivers.AnalogOutput.GetChannelMode (ic);

            if (m == Mode.Manual) {
                // To add more resolution easily the scale steps from 0 to 1000, but the card
                // needs 0 to 100 so divide by 10
                AquaPicDrivers.AnalogOutput.SetChannelValue (ic, value / 10f);
            }

            d.QueueDraw ();
        }

        protected void GetCardData () {
            var names = AquaPicDrivers.AnalogOutput.GetAllChannelNames (cardId);
            var values = AquaPicDrivers.AnalogOutput.GetAllChannelValues (cardId);
            var types = AquaPicDrivers.AnalogOutput.GetAllChannelTypes (cardId);
            var modes = AquaPicDrivers.AnalogOutput.GetAllChannelModes (cardId);

            int i = 0;
            foreach (var d in displays) {
                d.label.text = names [i];
                d.currentValue = values [i];
                d.typeLabel.text = Utils.GetDescription (types [i]);

                if (modes [i] == Mode.Auto) {
                    d.progressBar.enableTouch = false;
                    d.textBox.enableTouch = false;
                    d.forceButton.buttonColor = "grey4";
                } else {
                    d.progressBar.enableTouch = true;
                    d.textBox.enableTouch = true;
                    d.forceButton.buttonColor = "pri";
                }

                d.QueueDraw ();

                ++i;
            }
        }
    }
}

