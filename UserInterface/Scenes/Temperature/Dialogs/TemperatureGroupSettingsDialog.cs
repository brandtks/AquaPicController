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
using System.IO;
using Gtk;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules.Temperature;

namespace AquaPic.UserInterface
{
    public class TemperatureGroupSettingsDialog : TouchSettingsDialog
    {
        public string groupName { get; private set; }

        public TemperatureGroupSettingsDialog (TemperatureGroupSettings settings, Window parent)
            : base (settings.name + " Temperature", settings.name.IsNotEmpty (), parent) 
        {
            groupName = settings.name;

            var t = new SettingsTextBox ("Name");
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = groupName;
                t.textBox.enableTouch = false;
                t.textBox.TextChangedEvent += (sender, args) => {
                    MessageBox.Show ("Can not change temperature group name during runtime");
                    args.keepText = false;
                };
            } else {
                t.textBox.text = "Enter name";
                t.textBox.TextChangedEvent += (sender, args) => {
                    if (args.text.IsNotEmpty ()) {
                        args.keepText = false;
                    }else if (Temperature.TemperatureGroupNameExists (args.text)) {
                        MessageBox.Show ("Temperature group name already exists");
                        args.keepText = false;
                    }
                };
            }
            AddSetting (t);

            t = new SettingsTextBox ("Setpoint");
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = Temperature.GetTemperatureGroupTemperatureSetpoint (groupName).ToString ();
            } else {
                t.textBox.text = "0.0";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToSingle (args.text);
                } catch {
                    MessageBox.Show ("Improper floating point number format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ("Deadband");
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = Temperature.GetTemperatureGroupTemperatureDeadband (groupName).ToString ();
            } else {
                t.textBox.text = "0.5";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToSingle (args.text);
                } catch {
                    MessageBox.Show ("Improper floating point number format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ("High Alarm");
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = Temperature.GetTemperatureGroupHighTemperatureAlarmSetpoint (groupName).ToString ();
            } else {
                t.textBox.text = "100.0";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToSingle (args.text);
                } catch {
                    MessageBox.Show ("Improper floating point number format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ("Low Alarm");
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = Temperature.GetTemperatureGroupLowTemperatureAlarmSetpoint (groupName).ToString ();
            } else {
                t.textBox.text = "0.0";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToSingle (args.text);
                } catch {
                    MessageBox.Show ("Improper floating point number format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            DrawSettings ();
        }

        protected override bool OnSave (object sender) {
            var groupSettings = new TemperatureGroupSettings ();

            groupSettings.name = (string)settings["Name"].setting;
            if (groupSettings.name == "Enter name") {
                MessageBox.Show ("Invalid probe name");
                return false;
            }
            groupSettings.highTemperatureAlarmSetpoint = Convert.ToSingle (settings["High Alarm"].setting);
            groupSettings.lowTemperatureAlarmSetpoint = Convert.ToSingle (settings["Low Alarm"].setting);
            groupSettings.temperatureSetpoint = Convert.ToSingle (settings["Setpoint"].setting);
            groupSettings.temperatureDeadband = Convert.ToSingle (settings["Deadband"].setting);

            Temperature.UpdateTemperatureGroup (groupName, groupSettings);
            groupName = groupSettings.name;

            return true;
        }

        protected override bool OnDelete (object sender) {
            if (groupName == Temperature.defaultTemperatureGroup) {
                var parent = Toplevel as Window;
                var ms = new TouchDialog (groupName + " is the default temperature group.\n" +
                    "Are you sure you want to delete this group", parent);

                bool confirmed = false;
                ms.Response += (o, a) => {
                    if (a.ResponseId == ResponseType.Yes) {
                        confirmed = true;
                    }
                };

                ms.Run ();
                ms.Destroy ();

                if (!confirmed) {
                    return false;
                }
            }

            Temperature.RemoveTemperatureGroup (groupName);
            return true;
        }
    }
}

