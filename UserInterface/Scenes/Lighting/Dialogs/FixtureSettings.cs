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
using AquaPic.Gadgets.Device;
using AquaPic.Gadgets.Device.Lighting;
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
                if (args.text.IsEmpty ()) {
                    args.keepText = false;
                } else if (Devices.Lighting.GadgetNameExists (args.text)) {
                    MessageBox.Show ("Heater name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Outlet");
            if (fixtureName.IsNotEmpty ()) {
                var ic = settings.channel;
                c.combo.comboList.Add (string.Format ("Current: {0}.p{1}", ic.Group, ic.Individual));
                c.combo.activeIndex = 0;
            }
            c.combo.nonActiveMessage = "Select outlet";
            c.combo.comboList.AddRange (Driver.Power.GetAllAvaiableChannels ());
            AddSetting (c);

            var s = new SettingsSelectorSwitch ("Temp Lockout");
            if (fixtureName.IsNotEmpty ()) {
                s.selectorSwitch.currentSelected = settings.highTempLockout ? 0 : 1;
            }
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
            c.combo.comboList.AddRange (Driver.AnalogOutput.GetAllAvaiableChannels ());
            AddOptionalSetting (c);

            DrawSettings ();
        }

        protected override bool OnSave (object sender) {
            var fixtureSettings = new LightingFixtureSettings ();

            fixtureSettings.name = (string)settings["Name"].setting;
            if (fixtureSettings.name == "Enter name") {
                MessageBox.Show ("Please enter a fixture name");
                return false;
            }

            var outletString = (string)settings["Outlet"].setting;
            if (outletString.IsEmpty ()) {
                MessageBox.Show ("Please select a power outlet");
                return false;
            }
            fixtureSettings.channel = ParseIndividualControl (outletString);

            fixtureSettings.highTempLockout = (int)settings["Temp Lockout"].setting == 0;
            bool dimmingFixture = (int)settings["Dimming Fixture"].setting == 0;

            fixtureSettings.dimmingChannel = IndividualControl.Empty;
            if (dimmingFixture) {
                var channelString = (string)settings["Dimming Channel"].setting;
                if (channelString.IsEmpty ()) {
                    MessageBox.Show ("Please select a dimming channel");
                    return false;
                }
                fixtureSettings.dimmingChannel = ParseIndividualControl (channelString);
            }

            fixtureSettings.lightingStates = new LightingState[0];

            Devices.Lighting.UpdateGadget (fixtureName, fixtureSettings);
            fixtureName = fixtureSettings.name;

            return true;
        }

        protected override bool OnDelete (object sender) {
            Devices.Lighting.RemoveGadget (fixtureName);
            return true;
        }
    }
}

