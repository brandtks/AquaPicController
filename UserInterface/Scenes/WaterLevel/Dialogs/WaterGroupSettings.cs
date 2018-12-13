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
    public class WaterGroupSettings : TouchSettingsDialog
    {
        public string groupName { get; private set; }

        public WaterGroupSettings (WaterLevelGroupSettings settings, Window parent)
            : base (settings.name + " Water", settings.name.IsNotEmpty (), parent) 
        {
            groupName = settings.name;

            var t = new SettingsTextBox ("Name");
            t.textBox.text = groupName.IsNotEmpty () ? groupName : "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (args.text.IsEmpty ())
                    args.keepText = false;

                if (!WaterLevel.WaterLevelGroupNameOk (args.text)) {
                    MessageBox.Show ("Water level group name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var b = new SettingsBlank ("blank1");
            AddSetting (b);

            var s = new SettingsSelectorSwitch ("Enable High Alarm");
            s.selectorSwitch.currentSelected = 1;
            if (groupName.IsNotEmpty ()) {
                s.selectorSwitch.currentSelected = settings.enableHighAnalogAlarm ? 0 : 1;
            }
            AddSetting (s);

            t = new SettingsTextBox ("High Alarm");
            t.textBox.text = groupName.IsNotEmpty () ?
               settings.highAnalogAlarmSetpoint.ToString () :
               "0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float highAlarmStpnt = Convert.ToSingle (args.text);

                    if (highAlarmStpnt < 0f) {
                        MessageBox.Show ("High alarm setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper high alarm setpoint format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            s = new SettingsSelectorSwitch ("Enable Low Alarm");
            s.selectorSwitch.currentSelected = 1;
            if (groupName.IsNotEmpty ()) {
                s.selectorSwitch.currentSelected = settings.enableLowAnalogAlarm ? 0 : 1;
            } 
            AddSetting (s);

            t = new SettingsTextBox ("Low Alarm");
            t.textBox.text = groupName.IsNotEmpty () ?
                settings.lowAnalogAlarmSetpoint.ToString () :
                "0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float lowAlarmStpnt = Convert.ToSingle (args.text);

                    if (lowAlarmStpnt < 0f) {
                        MessageBox.Show ("Low alarm setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper low alarm setpoint format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            DrawSettings ();
        }

        protected override bool OnSave (object sender) {
            var groupSettings = new WaterLevelGroupSettings ();

            groupSettings.name = (string)settings["Name"].setting;
            if (groupSettings.name == "Enter name") {
                MessageBox.Show ("Invalid water group name");
                return false;
            }

            groupSettings.highAnalogAlarmSetpoint = Convert.ToSingle (settings["High Alarm"].setting);
            groupSettings.enableHighAnalogAlarm = (int)settings["Enable High Alarm"].setting == 0;
            groupSettings.lowAnalogAlarmSetpoint = Convert.ToSingle (settings["Low Alarm"].setting);
            groupSettings.enableLowAnalogAlarm = (int)settings["Enable Low Alarm"].setting == 0;

            WaterLevel.UpdateWaterLevelGroup (groupName, groupSettings);
            groupName = groupSettings.name;

            return true;
        }

        protected override bool OnDelete (object sender) {
            WaterLevel.RemoveWaterLevelGroup (groupName);
            return true;
        }
    }
}

