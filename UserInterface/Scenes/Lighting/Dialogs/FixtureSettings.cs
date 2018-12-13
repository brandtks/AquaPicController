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
        public string fixtureName { get; private set; }

        public FixtureSettings (LightingFixtureSettings settings, Window parent)
            : base (settings.name, settings.name.IsNotEmpty (), parent) 
        {
            fixtureName = settings.name;

            var t = new SettingsTextBox ("Name");
            t.textBox.text = fixtureName.IsNotEmpty () ? fixtureName : "Enter name";
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
            if (fixtureName.IsNotEmpty ()) {
                var ic = settings.powerOutlet;
                c.combo.comboList.Add (string.Format ("Current: {0}.p{1}", ic.Group, ic.Individual));
                c.combo.activeIndex = 0;
            }
            c.combo.nonActiveMessage = "Select outlet";
            c.combo.comboList.AddRange (Power.GetAllAvailableOutlets ());
            AddSetting (c);

            var s = new SettingsSelectorSwitch ("Temp Lockout");
            if (fixtureName.IsNotEmpty ()) {
                if (settings.highTempLockout)
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 0;
            AddSetting (s);

            bool isDimming = settings.dimmingChannel.IsNotEmpty ();

            s = new SettingsSelectorSwitch ("Dimming Fixture", "Yes", "No");
            s.selectorSwitch.currentSelected = isDimming ? 0 : 1;
            showOptional = isDimming;
            s.selectorSwitch.SelectorChangedEvent += (sender, args) => {
                if (args.currentSelectedIndex == 0)
                    showOptional = true;
                else
                    showOptional = false;
            };
            AddSetting (s);

            c = new SettingsComboBox ("Dimming Channel");
            if (fixtureName.IsNotEmpty () && isDimming) {
                var ic = settings.dimmingChannel;
                c.combo.comboList.Add (string.Format ("Current: {0}.q{1}", ic.Group, ic.Individual));
                c.combo.activeIndex = 0;
            }
            c.combo.nonActiveMessage = "Select outlet";
            c.combo.comboList.AddRange (AquaPicDrivers.AnalogOutput.GetAllAvaiableChannels ());
            AddOptionalSetting (c);

            DrawSettings ();
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

        protected override bool OnDelete (object sender) {
            Lighting.RemoveLight (fixtureName);
            return true;
        }
    }
}

