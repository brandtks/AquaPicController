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
using AquaPic.Modules;
using AquaPic.Globals;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class FixtureSettings : TouchSettingsDialog
    {
        string fixtureName;
        public string newOrUpdatedFixtureName {
            get {
                return fixtureName;
            }
        }

        public FixtureSettings (string fixtureName, bool includeDelete, Window parent)
            : base (fixtureName, includeDelete, parent) 
        {
            this.fixtureName = fixtureName;

            var t = new SettingsTextBox ("Name");
            if (this.fixtureName.IsNotEmpty ()) {
                t.textBox.text = this.fixtureName;
            } else {
                t.textBox.text = "Enter name";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!Lighting.FixtureNameOk (args.text)) {
                    MessageBox.Show ("Heater name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Outlet");
            if (this.fixtureName.IsNotEmpty ()) {
                IndividualControl ic = Lighting.GetFixtureOutletIndividualControl (this.fixtureName);
                c.combo.comboList.Add (string.Format ("Current: {0}.p{1}", ic.Group, ic.Individual));
                c.combo.activeIndex = 0;
            } else {
                c.combo.nonActiveMessage = "Select outlet";
            }
            c.combo.comboList.AddRange (Power.GetAllAvailableOutlets ());
            AddSetting (c);

            var s = new SettingsSelectorSwitch ("Temp Lockout");
            if (this.fixtureName.IsNotEmpty ()) {
                if (Lighting.GetFixtureTemperatureLockout (this.fixtureName))
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 0;
            AddSetting (s);

            bool isDimming;
            if (this.fixtureName.IsNotEmpty ())
                isDimming = Lighting.IsDimmingFixture (this.fixtureName);
            else
                isDimming = false;

            s = new SettingsSelectorSwitch ("Dimming Fixture", "Yes", "No");
            if (isDimming) {
                s.selectorSwitch.currentSelected = 0;
                showOptional = true;
            } else {
                s.selectorSwitch.currentSelected = 1;
                showOptional = false;
            }
            s.selectorSwitch.SelectorChangedEvent += (sender, args) => {
                if (args.currentSelectedIndex == 0)
                    showOptional = true;
                else
                    showOptional = false;

                UpdateSettingsVisibility ();
            };
            AddSetting (s);

            c = new SettingsComboBox ("Dimming Channel");
            if ((this.fixtureName.IsNotEmpty ()) && (isDimming)) {
                IndividualControl ic = Lighting.GetDimmingChannelIndividualControl (this.fixtureName);
                string cardName = ic.Group;
                c.combo.comboList.Add (string.Format ("Current: {0}.q{1}", cardName, ic.Individual));
                c.combo.activeIndex = 0;
            } else {
                c.combo.nonActiveMessage = "Select outlet";
            }
            c.combo.comboList.AddRange (AquaPicDrivers.AnalogOutput.GetAllAvaiableChannels ());
            AddOptionalSetting (c);

            DrawSettings ();
        }

        protected void ParseOutlet (string s, ref string g, ref int i) {
            int idx = s.IndexOf ('.');
            g = s.Substring (0, idx);
            i = Convert.ToInt32 (s.Substring (idx + 2));
        }

        protected void ParseChannnel (string s, ref string g, ref int i) {
            int idx = s.IndexOf ('.');
            g = s.Substring (0, idx);
            i = Convert.ToInt32 (s.Substring (idx + 2));
        }

        protected override bool OnSave (object sender) {
            var fixtureSettings = new LightingFixtureSettings ();
            fixtureSettings.name = ((SettingsTextBox)settings["Name"]).textBox.text;
            if (fixtureSettings.name == "Enter name") {
                MessageBox.Show ("Please enter a fixture name");
                return false;
            }

            var outletCombo = ((SettingsComboBox)settings["Outlet"]).combo;
            if (outletCombo.activeIndex == -1) {
                MessageBox.Show ("Please select a power outlet");
                return false;
            }
            var outletString = outletCombo.activeText;
            fixtureSettings.powerOutlet = ParseIndividualControl (outletString);

            fixtureSettings.highTempLockout = false;
            if (((SettingsSelectorSwitch)settings["Temp Lockout"]).selectorSwitch.currentSelected == 0) {
                fixtureSettings.highTempLockout = true;
            }

            bool dimmingFixture = false;
            if (((SettingsSelectorSwitch)settings["Dimming Fixture"]).selectorSwitch.currentSelected == 0) {
                dimmingFixture = true;
            }

            fixtureSettings.dimmingChannel = IndividualControl.Empty;
            if (dimmingFixture) {
                var channelCombo = ((SettingsComboBox)settings["Dimming Channel"]).combo;
                if (channelCombo.activeIndex == -1) {
                    MessageBox.Show ("Please select a dimming channel");
                    return false;
                }
                var channelString = channelCombo.activeText;
                fixtureSettings.dimmingChannel = ParseIndividualControl (channelString);
            }

            fixtureSettings.lightingStates = new LightingState[0];

            Lighting.UpdateLighting (fixtureName, fixtureSettings);
            fixtureName = fixtureSettings.name;

            return true;
        }

        /*
        protected override bool OnSave (object sender) {
            string name = ((SettingsTextBox)settings["Name"]).textBox.text;

            string outletStr = ((SettingsComboBox)settings["Outlet"]).combo.activeText;
            IndividualControl outletIc = IndividualControl.Empty;

            bool highTempLockout = true;
            try {
                SettingsSelectorSwitch s = settings["Temp Lockout"] as SettingsSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    highTempLockout = false;
            } catch {
                return false;
            }

            bool dimmingFixture = true;
            try {
                SettingsSelectorSwitch s = settings["Dimming Fixture"] as SettingsSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    dimmingFixture = false;
            } catch {
                return false;
            }

            var chStr = ((SettingsComboBox)settings["Dimming Channel"]).combo.activeText;
            var chIc = IndividualControl.Empty;

            if (fixtureName.IsEmpty ()) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid probe name");
                    return false;
                }

                if ((settings["Outlet"] as SettingsComboBox).combo.activeIndex == -1) {
                    MessageBox.Show ("Please select an outlet");
                    return false;
                }

                ParseOutlet (outletStr, ref outletIc.Group, ref outletIc.Individual);

                var lightingStates = new LightingState[0];

                if (dimmingFixture) {
                    if (((SettingsComboBox)settings["Dimming Channel"]).combo.activeIndex == -1) {
                        MessageBox.Show ("Please select a dimming channel");
                        return false;
                    }

                    ParseChannnel (chStr, ref chIc.Group, ref chIc.Individual);

                    Lighting.AddLight (name, outletIc, chIc, lightingStates, highTempLockout);
                } else {
                    Lighting.AddLight (name, outletIc, lightingStates, highTempLockout);
                }

                fixtureName = name;
            } else {
                bool isDimming = Lighting.IsDimmingFixture (fixtureName);
                // isDimming and dimmingFixture must match, whether that's true or false
                if (isDimming == dimmingFixture) {
                    string oldName = fixtureName;
                    if (oldName != name) {
                        Lighting.SetFixtureName (fixtureName, name);
                        fixtureName = name;
                    }

                    if (!outletStr.StartsWith ("Current:")) {
                        ParseOutlet (outletStr, ref outletIc.Group, ref outletIc.Individual);
                        Lighting.SetFixtureOutletIndividualControl (fixtureName, outletIc);
                    } else {
                        outletIc = Lighting.GetFixtureOutletIndividualControl (fixtureName);
                    }

                    Lighting.SetFixtureTemperatureLockout (fixtureName, highTempLockout);

                    if (dimmingFixture) {
                        if (!chStr.StartsWith ("Current:")) {
                            ParseChannnel (chStr, ref chIc.Group, ref chIc.Individual);
                            Lighting.SetDimmingChannelIndividualControl (fixtureName, chIc);
                        }
                        chIc = Lighting.GetDimmingChannelIndividualControl (fixtureName);
                    }

                    Lighting.UpdateFixtureSettingsToFile (fixtureName, oldName);
                } else {
                    MessageBox.Show ("Can't change dimmablility");
                    return false;
                }
            }

            return true;
        }
        */

        protected override bool OnDelete (object sender) {
            Lighting.RemoveLight (fixtureName);
            return true;
        }
    }
}

