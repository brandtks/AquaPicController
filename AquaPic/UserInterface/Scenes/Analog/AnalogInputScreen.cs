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
using System.Globalization;
using Gtk;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;
using AquaPic.Drivers;
using AquaPic.Globals;
using AquaPic.SerialBus;

namespace AquaPic.UserInterface
{
    public class AnalogInputWindow : SceneBase
    {
        TouchComboBox combo;
        string card;
        AnalogChannelDisplay[] displays;
        TouchButton settingsButton;

        public AnalogInputWindow (params object[] options) : base () {
            card = AquaPicDrivers.AnalogInput.firstCard;
            if (card.IsNotEmpty ()) {
                sceneTitle = "Analog Inputs Cards";
            } else {
                sceneTitle = "No Analog Input Cards Added";
            }

            displays = new AnalogChannelDisplay[4];
            for (int i = 0; i < 4; ++i) {
                displays[i] = new AnalogChannelDisplay ();
                displays[i].divisionSteps = 4096;
                displays[i].ForceButtonReleaseEvent += OnForceRelease;
                displays[i].SettingsButtonReleaseEvent += OnSettingsRelease;
                displays[i].ValueChangedEvent += OnValueChanged;
                displays[i].typeLabel.Visible = true;
                Put (displays[i], 70, 90 + (i * 75));
                if (card.IsNotEmpty ()) {
                    displays[i].Show ();
                } else {
                    displays[i].Visible = false;
                }
            }

            settingsButton = new TouchButton ();
            settingsButton.SetSizeRequest (30, 30);
            settingsButton.text = Convert.ToChar (0x2699).ToString ();
            settingsButton.ButtonReleaseEvent += OnGlobalSettingsRelease;
            Put (settingsButton, 755, 35);
            settingsButton.Show ();

            combo = new TouchComboBox (AquaPicDrivers.AnalogInput.GetAllCardNames ());
            combo.comboList.Add ("New card...");
            if (card.IsNotEmpty ()) {
                combo.activeText = card;
            }
            combo.WidthRequest = 200;
            combo.ComboChangedEvent += OnComboChanged;
            Put (combo, 550, 35);
            combo.Show ();

            GetCardData ();
            Show ();
        }

        protected override bool OnUpdateTimer () {
            if (card.IsNotEmpty ()) {
                var values = AquaPicDrivers.AnalogInput.GetAllChannelValues (card);

                int i = 0;
                foreach (var d in displays) {
                    d.currentValue = values[i];
                    d.QueueDraw ();

                    ++i;
                }
            }
            return true;
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs args) {
            if (args.activeText != "New card...") {
                card = args.activeText;
                GetCardData ();
            } else {
                var parent = Toplevel as Window;
                if (parent != null) {
                    if (!parent.IsTopLevel)
                        parent = null;
                }

                var numberInput = new TouchNumberInput (false, parent);
                numberInput.Title = "Address";

                numberInput.TextSetEvent += (o, a) => {
                    a.keepText = CardSettingsHelper.OnAddressSetEvent (a.text, ref card, AquaPicDrivers.AnalogInput);

                    if (a.keepText) {
                        combo.comboList.Insert (combo.comboList.Count - 1, card);
                        foreach (var display in displays) {
                            display.Visible = true;
                        }
                        combo.activeText = card;
                        combo.Visible = false;
                        combo.Visible = true;
                        sceneTitle = "Analog Inputs Cards";
                        GetCardData ();
                    }
                };

                numberInput.Run ();
                numberInput.Destroy ();
            }

            QueueDraw ();
        }

        protected void OnGlobalSettingsRelease (object sender, ButtonReleaseEventArgs args) {
            if (card.IsNotEmpty ()) {
                if (AquaPicDrivers.AnalogInput.CheckCardEmpty (card)) {
                    var parent = Toplevel as Window;
                    if (parent != null) {
                        if (!parent.IsTopLevel)
                            parent = null;
                    }

                    var ms = new TouchDialog ("Are you sure you with to delete " + card, parent);

                    ms.Response += (o, a) => {
                        if (a.ResponseId == ResponseType.Yes) {
                            var deleted = CardSettingsHelper.OnCardDeleteEvent (card, AquaPicDrivers.AnalogInput);
                            if (deleted) {
                                combo.comboList.Remove (card);
                                if (AquaPicDrivers.AnalogInput.cardCount == 0) {
                                    card = string.Empty;
                                    sceneTitle = "No Analog Input Cards Added";
                                    foreach (var display in displays) {
                                        display.Visible = false;
                                    }
                                    combo.activeIndex = -1;
                                    settingsButton.buttonColor = "grey1";
                                } else {
                                    card = AquaPicDrivers.AnalogInput.firstCard;
                                    combo.activeText = card;
                                    GetCardData ();
                                }
                                QueueDraw ();
                            }
                        }
                    };

                    ms.Run ();
                    ms.Destroy ();
                }
            }
        }

        protected void OnForceRelease (object sender, ButtonReleaseEventArgs args) {
            var d = sender as AnalogChannelDisplay;

            var ic = IndividualControl.Empty;
            ic.Group = card;
            ic.Individual = AquaPicDrivers.AnalogInput.GetChannelIndex (card, d.label.text);

            Mode m = AquaPicDrivers.AnalogInput.GetChannelMode (ic);

            if (m == Mode.Auto) {
                AquaPicDrivers.AnalogInput.SetChannelMode (ic, Mode.Manual);
                d.progressBar.enableTouch = true;
                d.textBox.enableTouch = true;
                d.forceButton.buttonColor = "pri";
            } else {
                AquaPicDrivers.AnalogInput.SetChannelMode (ic, Mode.Auto);
                d.progressBar.enableTouch = false;
                d.textBox.enableTouch = false;
                d.forceButton.buttonColor = "grey4";
            }

            d.QueueDraw ();
        }

        protected void OnSettingsRelease (object sender, ButtonReleaseEventArgs args) {
            var d = sender as AnalogChannelDisplay;

            var ic = IndividualControl.Empty;
            ic.Group = card;
            ic.Individual = AquaPicDrivers.AnalogInput.GetChannelIndex (card, d.label.text);

            var parent = Toplevel as Window;
            if (parent != null) {
                if (!parent.IsTopLevel)
                    parent = null;
            }

            var numberInput = new TouchNumberInput (false, parent);
            numberInput.Title = "LPF";

            numberInput.TextSetEvent += (obj, a) => {
                if (a.text.IsNotEmpty ()) {
                    try {
                        var lpf = Convert.ToInt32 (a.text);
                        AquaPicDrivers.AnalogInput.SetChannelLowPassFilterFactor (ic, lpf);
                        d.typeLabel.text = string.Format ("LPF: {0}", lpf);
                    } catch {
                        MessageBox.Show ("Invalid number format");
                    }
                } else {
                    MessageBox.Show ("Low pass filter can't be empty");
                }
            };

            numberInput.Run ();
            numberInput.Destroy ();

            d.QueueDraw ();
        }

        protected void OnValueChanged (object sender, float value) {
            var d = sender as AnalogChannelDisplay;

            var ic = IndividualControl.Empty;
            ic.Group = card;
            ic.Individual = AquaPicDrivers.AnalogInput.GetChannelIndex (card, d.label.text);

            Mode m = AquaPicDrivers.AnalogInput.GetChannelMode (ic);

            if (m == Mode.Manual)
                AquaPicDrivers.AnalogInput.SetChannelValue (ic, value);

            d.QueueDraw ();
        }

        protected void GetCardData () {
            if (card.IsNotEmpty ()) {
                var names = AquaPicDrivers.AnalogInput.GetAllChannelNames (card);
                var values = AquaPicDrivers.AnalogInput.GetAllChannelValues (card);
                var modes = AquaPicDrivers.AnalogInput.GetAllChannelModes (card);
                var factors = AquaPicDrivers.AnalogInput.GetAllChannelLowPassFilterFactors (card);

                int i = 0;
                foreach (var d in displays) {
                    d.label.text = names[i];
                    d.currentValue = values[i];
                    d.typeLabel.text = string.Format ("LPF: {0}", factors[i]);

                    if (modes[i] == Mode.Auto) {
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

                if (AquaPicDrivers.AnalogInput.CheckCardEmpty (card)) {
                    settingsButton.buttonColor = "compl";
                } else {
                    settingsButton.buttonColor = "grey1";
                }
            } else {
                settingsButton.buttonColor = "grey1";
            }
        }
    }
}

