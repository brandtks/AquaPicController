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
using System.Linq;

namespace AquaPic.UserInterface
{
    public class PowerWindow : SceneBase
    {
        PowerOutletSlider[] selectors;
        string powerStripName;
        TouchComboBox combo;
        TouchButton settingsButton;
        PowerWindowGraphics graphics;
        PowerBase power = AquaPicDrivers.Power;

        public PowerWindow (params object[] options) : base () {
            powerStripName = power.firstCard;
            if (powerStripName.IsNotEmpty ()) {
                sceneTitle = "Power Strip";
            } else {
                sceneTitle = "No Power Strips Added";
            }

            graphics = new PowerWindowGraphics ();
            Put (graphics, 50, 80);
            if (powerStripName.IsNotEmpty ()) {
                graphics.Show ();
            } else {
                graphics.Visible = false;
            }

            int x, y;
            selectors = new PowerOutletSlider[8];
            for (int i = 0; i < 8; ++i) {
                selectors[i] = new PowerOutletSlider (i);
                selectors[i].ss.SelectorChangedEvent += OnSelectorChanged;

                if ((i % 2) == 0) { // even numbers top row
                    x = ((i - (i / 2)) * 185) + 50;
                    y = 85;
                } else {
                    x = (((i - (i / 2)) - 1) * 185) + 50;
                    y = 275;
                }
                Put (selectors[i], x, y);

                if (powerStripName.IsNotEmpty ()) {
                    selectors[i].Show ();
                    var ic = IndividualControl.Empty;
                    ic.Group = powerStripName;
                    ic.Individual = i;
                    selectors[i].Subscribe (ic);
                } else {
                    selectors[i].Visible = false;
                }
            }

            settingsButton = new TouchButton ();
            settingsButton.SetSizeRequest (30, 30);
            settingsButton.buttonColor = "pri";
            settingsButton.text = Convert.ToChar (0x2699).ToString ();
            settingsButton.ButtonReleaseEvent += OnSettingsRelease;
            Put (settingsButton, 755, 35);
            settingsButton.Show ();

            combo = new TouchComboBox (power.GetAllCardNames ());
            combo.comboList.Add ("New power strip...");
            combo.activeText = powerStripName;
            combo.WidthRequest = 200;
            combo.ComboChangedEvent += OnComboChanged;
            Put (combo, 550, 35);
            combo.Show ();

            GetPowerData ();

            Show ();
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs args) {
            if (args.activeText != "New power strip...") {
                for (int i = 0; i < selectors.Length; ++i) {
                    selectors[i].Unsubscribe ();
                }

                powerStripName = args.activeText;
                GetPowerData ();

                var ic = IndividualControl.Empty;
                ic.Group = powerStripName;
                for (int i = 0; i < selectors.Length; ++i) {
                    ic.Individual = i;
                    selectors[i].Subscribe (ic);
                }
            } else {
                var parent = Toplevel as Window;
                var s = new PowerSettings (string.Empty, false, parent);
                s.Run ();

                if (s.outcome == TouchSettingsOutcome.Added) {
                    powerStripName = s.newPowerStripName;
                    combo.comboList.Insert (combo.comboList.Count - 1, powerStripName);
                    foreach (var sel in selectors) {
                        sel.Visible = true;
                    }
                    graphics.Visible = true;
                    combo.Visible = false;
                    combo.Visible = true;
                    sceneTitle = "Power Strip";
                    GetPowerData ();
                }

                combo.activeText = powerStripName;
            }

            QueueDraw ();
        }

        protected void OnSettingsRelease (object sender, ButtonReleaseEventArgs args) {
            if (powerStripName.IsNotEmpty ()) {
                var parent = Toplevel as Window;
                var s = new PowerSettings (powerStripName, power.CheckCardEmpty (powerStripName), parent);
                s.Run ();

                if (s.outcome == TouchSettingsOutcome.Deleted) {
                    combo.comboList.Remove (powerStripName);
                    if (power.cardCount == 0) {
                        powerStripName = string.Empty;
                        sceneTitle = "No Power Strips Added";
                        foreach (var sel in selectors) {
                            sel.Visible = false;
                        }
                        graphics.Visible = false;
                        combo.activeIndex = -1;
                    } else {
                        powerStripName = AquaPicDrivers.Power.firstCard;
                        combo.activeText = powerStripName;
                        GetPowerData ();
                    }
                    QueueDraw ();
                }
            }
        }

        protected void GetPowerData () {
            if (powerStripName.IsNotEmpty ()) {
                var states = power.GetAllChannelValues (powerStripName);
                var modes = power.GetAllChannelModes (powerStripName);
                var names = power.GetAllChannelNames (powerStripName);

                for (var i = 0; i < states.Length; ++i) {
                    var s = selectors[i];
                    s.outletName.text = names[i];

                    if (states[i]) {
                        s.statusLabel.text = "On";
                        s.statusLabel.textColor = "secb";
                    } else {
                        s.statusLabel.text = "Off";
                        s.statusLabel.textColor = "grey4";
                    }

                    if (modes[i] == Mode.Auto) {
                        s.ss.currentSelected = 1;
                    } else { // mode is manual
                        if (states[i] == MyState.On) {
                            s.ss.currentSelected = 2;
                        } else {
                            s.ss.currentSelected = 0;
                        }
                    }
                    s.QueueDraw ();
                }
            }
        }

        protected void OnSelectorChanged (object sender, SelectorChangedEventArgs e) {
            var ss = sender as TouchSelectorSwitch;
            var ic = IndividualControl.Empty;
            ic.Group = powerStripName;
            ic.Individual = ss.id;

            if (ss.currentSelected == 1) { // auto
                power.SetChannelMode (ic, Mode.Auto);
            } else if (ss.currentSelected == 0) { // manual and state off
                power.SetChannelMode (ic, Mode.Manual);
                power.SetChannelValue (ic, false);
            } else if (ss.currentSelected == 2) {// manual and state on
                power.SetChannelMode (ic, Mode.Manual);
                power.SetChannelValue (ic, true);
            }
        }

        protected void OnExposeEvent (object sender, ExposeEventArgs args) {
            if (powerStripName.IsNotEmpty ()) {
                using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                    TouchColor.SetSource (cr, "grey3", 0.75);

                    double midY = 272.5;

                    for (int i = 0; i < 3; ++i) {
                        cr.MoveTo (60 + (i * 185), midY);
                        cr.LineTo (220 + (i * 185), midY);
                        cr.ClosePath ();
                        cr.Stroke ();

                        cr.MoveTo (232.5 + (i * 185), 115);
                        cr.LineTo (232.5 + (i * 185), 425);
                        cr.ClosePath ();
                        cr.Stroke ();
                    }

                    cr.MoveTo (615, midY);
                    cr.LineTo (775, midY);
                    cr.ClosePath ();
                    cr.Stroke ();
                }
            }
        }

        public override void Dispose () {
            if (powerStripName.IsNotEmpty ()) {
                for (int i = 0; i < selectors.Length; ++i) {
                    selectors[i].Unsubscribe ();
                }
            }

            base.Dispose ();
        }

        class PowerWindowGraphics : EventBox
        {
            public PowerWindowGraphics () {
                Visible = true;
                VisibleWindow = false;

                WidthRequest = 750;
                HeightRequest = 336;

                ExposeEvent += OnExpose;
            }

            protected virtual void OnExpose (object sender, ExposeEventArgs args) {
                using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                    TouchColor.SetSource (cr, "grey3", 0.75);

                    double midY = 272.5;

                    for (int i = 0; i < 3; ++i) {
                        cr.MoveTo (60 + (i * 185), midY);
                        cr.LineTo (220 + (i * 185), midY);
                        cr.ClosePath ();
                        cr.Stroke ();

                        cr.MoveTo (232.5 + (i * 185), 115);
                        cr.LineTo (232.5 + (i * 185), 425);
                        cr.ClosePath ();
                        cr.Stroke ();
                    }

                    cr.MoveTo (615, midY);
                    cr.LineTo (775, midY);
                    cr.ClosePath ();
                    cr.Stroke ();
                }
            }
        }
    }
}

