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
using Cairo;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Drivers;
using AquaPic.Globals;

namespace AquaPic.UserInterface
{
    public class DigitalInputWindow : SceneBase
    {
        TouchComboBox combo;
        string card;
        DigitalDisplay[] displays;
        TouchButton settingsButton;

        public DigitalInputWindow (params object[] options) : base () {
            card = AquaPicDrivers.DigitalInput.firstCard;
            if (card.IsNotEmpty ()) {
                sceneTitle = "Digital Inputs Cards";
            } else {
                sceneTitle = "No Digital Input Cards Added";
            }

            displays = new DigitalDisplay[6];

            for (int i = 0; i < 6; ++i) {
                displays[i] = new DigitalDisplay ();
                displays[i].ForceButtonReleaseEvent += OnForceRelease;
                displays[i].StateSelectedChangedEvent += OnSelectorChanged;
                int x, y;
                if (i < 3) {
                    x = (i * 150) + 175;
                    y = 125;
                } else {
                    x = ((i - 3) * 150) + 175;
                    y = 275;
                }
                Put (displays[i], x, y);
                if (card.IsNotEmpty ()) {
                    displays[i].Show ();
                } else {
                    displays[i].Visible = false;
                }
            }

            settingsButton = new TouchButton ();
            settingsButton.SetSizeRequest (30, 30);
            settingsButton.buttonColor = "grey4";
            settingsButton.text = Convert.ToChar (0x2699).ToString ();
            settingsButton.ButtonReleaseEvent += OnGlobalSettingsRelease;
            Put (settingsButton, 755, 35);
            settingsButton.Show ();

            combo = new TouchComboBox (AquaPicDrivers.DigitalInput.GetAllCardNames ());
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
                var states = AquaPicDrivers.DigitalInput.GetAllChannelValues (card);

                for (int i = 0; i < states.Length; ++i) {
                    if (states[i]) {
                        displays[i].textBox.textColor = "pri";
                        displays[i].textBox.text = "Closed";
                    } else {
                        displays[i].textBox.textColor = "seca";
                        displays[i].textBox.text = "Open";
                    }

                    displays[i].textBox.QueueDraw ();
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
                    a.keepText = CardSettingsHelper.OnAddressSetEvent (a.text, ref card, AquaPicDrivers.DigitalInput);

                    if (a.keepText) {
                        combo.comboList.Insert (combo.comboList.Count - 1, card);
                        foreach (var display in displays) {
                            display.Visible = true;
                        }
                        combo.activeText = card;
                        combo.Visible = false;
                        combo.Visible = true;
                        sceneTitle = "Digital Input Cards";
                        GetCardData ();
                    }
                };

                numberInput.Run ();
                numberInput.Destroy ();

                // The number input was canceled
                if (combo.activeText == "New card...") {
                    card = AquaPicDrivers.DigitalInput.firstCard;
                    combo.activeText = card;
                    GetCardData ();
                }
            }

            QueueDraw ();
        }

        protected void OnGlobalSettingsRelease (object sender, ButtonReleaseEventArgs args) {
            if (card.IsNotEmpty ()) {
                if (AquaPicDrivers.DigitalInput.CheckCardEmpty (card)) {
                    var parent = Toplevel as Window;
                    if (parent != null) {
                        if (!parent.IsTopLevel)
                            parent = null;
                    }

                    var ms = new TouchDialog ("Are you sure you with to delete " + card, parent);

                    ms.Response += (o, a) => {
                        if (a.ResponseId == ResponseType.Yes) {
                            var deleted = CardSettingsHelper.OnCardDeleteEvent (card, AquaPicDrivers.DigitalInput);
                            if (deleted) {
                                combo.comboList.Remove (card);
                                if (AquaPicDrivers.DigitalInput.cardCount == 0) {
                                    card = string.Empty;
                                    sceneTitle = "No Digial Input Cards Added";
                                    foreach (var display in displays) {
                                        display.Visible = false;
                                    }
                                    combo.activeIndex = -1;
                                    settingsButton.buttonColor = "grey1";
                                } else {
                                    card = AquaPicDrivers.DigitalInput.firstCard;
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
            var d = sender as DigitalDisplay;

            var ic = IndividualControl.Empty;
            ic.Group = card;
            ic.Individual = AquaPicDrivers.DigitalInput.GetChannelIndex (card, d.label.text);

            Mode m = AquaPicDrivers.DigitalInput.GetChannelMode (ic);

            if (m == Mode.Auto) {
                AquaPicDrivers.DigitalInput.SetChannelMode (ic, Mode.Manual);
                d.selector.Visible = true;
                d.button.buttonColor = "pri";
            } else {
                AquaPicDrivers.DigitalInput.SetChannelMode (ic, Mode.Auto);
                d.selector.Visible = false;
                d.button.buttonColor = "grey4";
            }

            d.QueueDraw ();
        }

        protected void OnSelectorChanged (object sender, SelectorChangedEventArgs args) {
            var d = sender as DigitalDisplay;

            var ic = IndividualControl.Empty;
            ic.Group = card;
            ic.Individual = AquaPicDrivers.DigitalInput.GetChannelIndex (card, d.label.text);

            bool oldState = AquaPicDrivers.DigitalInput.GetChannelValue (ic);
            bool newState = args.currentSelectedIndex == 1;

            if (!oldState && newState) {
                AquaPicDrivers.DigitalInput.SetChannelValue (ic, true);
                d.textBox.textColor = "pri";
                d.textBox.text = "Closed";
            } else if (oldState && !newState) {
                AquaPicDrivers.DigitalInput.SetChannelValue (ic, false);
                d.textBox.textColor = "seca";
                d.textBox.text = "Open";
            }

            d.QueueDraw ();
        }

        protected void GetCardData () {
            if (card.IsNotEmpty ()) {
                var states = AquaPicDrivers.DigitalInput.GetAllChannelValues (card);
                var modes = AquaPicDrivers.DigitalInput.GetAllChannelModes (card);
                var names = AquaPicDrivers.DigitalInput.GetAllChannelNames (card);

                int i = 0;
                foreach (var d in displays) {
                    d.label.text = names[i];

                    if (states[i]) {
                        d.textBox.textColor = "pri";
                        d.textBox.text = "Closed";
                        d.selector.currentSelected = 1;
                    } else {
                        d.textBox.textColor = "seca";
                        d.textBox.text = "Open";
                        d.selector.currentSelected = 0;
                    }

                    if (modes[i] == Mode.Auto) {
                        d.selector.Visible = false;
                        d.button.buttonColor = "grey4";
                    } else {
                        d.selector.Visible = true;
                        d.button.buttonColor = "pri";
                    }

                    d.QueueDraw ();

                    ++i;
                }

                if (AquaPicDrivers.DigitalInput.CheckCardEmpty (card)) {
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

