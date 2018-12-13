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
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class AtoSettings : TouchSettingsDialog
    {
        public string groupName { get; private set; }

        public AtoSettings (AutoTopOffGroupSettings settings, Window parent)
            : base (settings.name + " Auto Top Off", settings.name.IsNotEmpty (), parent) 
        {
            groupName = settings.name;

            var t = new SettingsTextBox ("Name");
            t.textBox.text = groupName.IsNotEmpty () ? groupName : "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (args.text.IsEmpty ())
                    args.keepText = false;
                else if (!AutoTopOff.AtoGroupNameOk (args.text)) {
                    MessageBox.Show ("ATO group name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var s = new SettingsSelectorSwitch ("Enable");
            if (groupName.IsNotEmpty ()) {
                s.selectorSwitch.currentSelected = settings.enable ? 0 : 1;
            }
            AddSetting (s);

            var c = new SettingsComboBox ("Water Group");
            c.combo.nonActiveMessage = "Select Water Level Group";
            var availableGroups = WaterLevel.GetAllWaterLevelGroupNames ();
            c.combo.comboList.AddRange (availableGroups);
            if (groupName.IsNotEmpty ()) {
                var index = Array.IndexOf (availableGroups, settings.waterLevelGroupName);
                c.combo.activeIndex = index;
            }
            AddSetting (c);

            t = new SettingsTextBox ("Request Bit Name");
            t.textBox.text = groupName.IsNotEmpty () ? settings.requestBitName : "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (args.text.IsEmpty ()) {
                    args.keepText = false;
                }
            };
            AddSetting (t);

            s = new SettingsSelectorSwitch ("Use Analog");
            if (groupName.IsNotEmpty ()) {
                s.selectorSwitch.currentSelected = settings.useAnalogSensors ? 0 : 1;
            }
            AddSetting (s);

            t = new SettingsTextBox ("Analog Off");
            t.textBox.text = groupName.IsNotEmpty () ? settings.analogOffSetpoint.ToString () : "0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float offStpnt = Convert.ToSingle (args.text);

                    if (offStpnt < 0f) {
                        MessageBox.Show ("Analog on setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper analog off setpoint format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ("Analog On");
            t.textBox.text = groupName.IsNotEmpty () ? settings.analogOnSetpoint.ToString () : "0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float onStpnt = Convert.ToSingle (args.text);

                    if (onStpnt < 0f) {
                        MessageBox.Show ("Analog on setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper analog on setpoint format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            s = new SettingsSelectorSwitch ("Use Float Switch");
            if (groupName.IsNotEmpty ()) {
                s.selectorSwitch.currentSelected = settings.useFloatSwitches ? 0 : 1;
            }
            AddSetting (s);

            t = new SettingsTextBox ("Max Runtime");
            t.textBox.text = groupName.IsNotEmpty () ? 
                string.Format ("{0} mins", settings.maximumRuntime) : 
                "1 min";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (args.text.IsEmpty ())
                    args.keepText = false;

                var time = ParseTime (args.text);
                if (time >= 0) {
                    args.text = string.Format ("{0} mins", time);
                } else {
                    MessageBox.Show ("Improper format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ("Cooldown");
            t.textBox.text = groupName.IsNotEmpty () ?
                string.Format ("{0} mins", settings.minimumCooldown) :
                "10 min";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (args.text.IsEmpty ())
                    args.keepText = false;

                var time = ParseTime (args.text);
                if (time >= 0) {
                    args.text = string.Format ("{0} mins", time);
                } else {
                    MessageBox.Show ("Improper format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            DrawSettings ();
        }

        protected int ParseTime (string input) {
            int time = -1;

            int idx = input.IndexOf ("mins", StringComparison.InvariantCultureIgnoreCase);
            try {
                if (idx == -1)
                    time = Convert.ToInt32 (input);
                else {
                    var timeString = input.Substring (0, idx);
                    time = Convert.ToInt32 (timeString);
                }
            } catch {
                time = -1;
            }

            return time;
        }

        protected override bool OnSave (object sender) {
            var atoSettings = new AutoTopOffGroupSettings ();

            atoSettings.name = (string)settings["Name"].setting;
            if (atoSettings.name == "Enter name") {
                MessageBox.Show ("Invalid ATO name");
                return false;
            }

            atoSettings.enable = (int)settings["Enable"].setting == 0;

            atoSettings.waterLevelGroupName = (string)settings["Water Group"].setting;
            if (atoSettings.waterLevelGroupName.IsEmpty ()) {
                MessageBox.Show ("Please select a Water Level Group");
                return false;
            }

            atoSettings.requestBitName = (string)settings["Request Bit Name"].setting;
            if (atoSettings.requestBitName == "Enter name") {
                MessageBox.Show ("Invalid request bit name");
                return false;
            }

            atoSettings.useAnalogSensors = (int)settings["Use Analog"].setting == 0;
            atoSettings.analogOnSetpoint = Convert.ToSingle (settings["Analog On"].setting);
            atoSettings.analogOffSetpoint = Convert.ToSingle (settings["Analog Off"].setting);

            if (atoSettings.analogOnSetpoint > atoSettings.analogOffSetpoint) {
                MessageBox.Show ("Analog On Setpoint can not be higher than the analog off setpoint");
                return false;
            }

            atoSettings.useFloatSwitches = (int)settings["Use Float Switch"].setting == 0;

            atoSettings.maximumRuntime = (uint)ParseTime ((string)settings["Max Runtime"].setting);
            atoSettings.minimumCooldown = (uint)ParseTime ((string)settings["Cooldown"].setting);

            AutoTopOff.UpdateAtoGroup (groupName, atoSettings);
            groupName = atoSettings.name;

            return true;
        }

        protected override bool OnDelete (object sender) {
            AutoTopOff.RemoveAtoGroup (groupName);
            return true;
        }
    }
}

