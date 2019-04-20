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
using AquaPic.Modules.Temperature;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class HeaterSettingsDialog : TouchSettingsDialog
    {
        public string heaterName { get; private set; }
        string groupName;

        public HeaterSettingsDialog (string groupName, HeaterSettings settings, Window parent)
            : base (settings.name, settings.name.IsNotEmpty (), parent) 
        {
            heaterName = settings.name;
            this.groupName = groupName;

            var t = new SettingsTextBox ("Name");
            if (heaterName.IsNotEmpty ()) {
                t.textBox.text = heaterName;
            } else {
                t.textBox.text = "Enter name";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text)) {
                    args.keepText = false;
                }  else if (Temperature.HeaterNameExists (groupName, args.text)) {
                    MessageBox.Show ("Heater name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Outlet");
            if (heaterName.IsNotEmpty ()) {
                var ic = settings.channel;
                c.combo.comboList.Add (string.Format ("Current: {0}.i{1}", ic.Group, ic.Individual));
                c.combo.activeIndex = 0;
            }
            c.combo.nonActiveMessage = "Please select outlet";
            c.combo.comboList.AddRange (Driver.Power.GetAllAvaiableChannels ());
            AddSetting (c);

            DrawSettings ();
        }

        protected void ParseOutlet (string s, ref string g, ref int i) {
            int idx = s.IndexOf ('.');
            string powerStripName = s.Substring (0, idx);
            g = powerStripName;
            i = Convert.ToByte (s.Substring (idx + 2));
        }

        protected override bool OnSave (object sender) {
            var heaterSettings = new HeaterSettings ();

            heaterSettings.name = (string)settings["Name"].setting;
            if (heaterSettings.name == "Enter name") {
                MessageBox.Show ("Invalid probe name");
                return false;
            }

            string outletString = (string)settings["Outlet"].setting;
            if (outletString.IsEmpty ()) {
                MessageBox.Show ("Please select an channel");
                return false;
            }
            heaterSettings.channel = ParseIndividualControl (outletString);

            Temperature.UpdateHeater (groupName, heaterName, heaterSettings);
            heaterName = heaterSettings.name;

            return true;
        }

        protected override bool OnDelete (object sender) {
            Temperature.RemoveHeater (groupName, heaterName);
            return true;
        }
    }
}

