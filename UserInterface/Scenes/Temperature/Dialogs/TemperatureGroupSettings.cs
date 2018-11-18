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
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    public class TemperatureGroupSettings : TouchSettingsDialog
    {
        string groupName;
        public string temperatureGroupName {
            get {
                return groupName;
            }
        }

        public TemperatureGroupSettings (string name, bool includeDelete, Window parent)
            : base (name + " Temperature", includeDelete, parent) {
            groupName = name;

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
                    if (string.IsNullOrWhiteSpace (args.text))
                        args.keepText = false;
                    else if (!Temperature.TemperatureGroupNameOk (args.text)) {
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
            settings.Add (t.label.text, t);

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
            settings.Add (t.label.text, t);

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
            settings.Add (t.label.text, t);

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
            settings.Add (t.label.text, t);

            DrawSettings ();
        }

        protected override bool OnSave (object sender) {
            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            if (groupName.IsEmpty ()) {
                var name = (settings["Name"] as SettingsTextBox).textBox.text;
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid temperature group name");
                    return false;
                }

                var highTemperatureAlarmSetpoint = Convert.ToSingle ((settings["High Alarm"] as SettingsTextBox).textBox.text);
                var lowTemperatureAlarmSetpoint = Convert.ToSingle ((settings["Low Alarm"] as SettingsTextBox).textBox.text);
                var temperatureSetpoint = Convert.ToSingle ((settings["Setpoint"] as SettingsTextBox).textBox.text);
                var temperatureDeadband = Convert.ToSingle ((settings["Deadband"] as SettingsTextBox).textBox.text);

                Temperature.AddTemperatureGroup (
                    name,
                    highTemperatureAlarmSetpoint,
                    lowTemperatureAlarmSetpoint,
                    temperatureSetpoint,
                    temperatureDeadband);

                JObject jobj = new JObject ();

                jobj.Add (new JProperty ("name", name));
                jobj.Add (new JProperty ("highTemperatureAlarmSetpoint", highTemperatureAlarmSetpoint.ToString ()));
                jobj.Add (new JProperty ("lowTemperatureAlarmSetpoint", lowTemperatureAlarmSetpoint.ToString ()));
                jobj.Add (new JProperty ("temperatureSetpoint", temperatureSetpoint.ToString ()));
                jobj.Add (new JProperty ("temperatureDeadband", temperatureDeadband.ToString ()));

                (jo["temperatureGroups"] as JArray).Add (jobj);

                //Get new groups name
                groupName = name;
            } else {
                var highTemperatureAlarmSetpoint = Convert.ToSingle ((settings["High Alarm"] as SettingsTextBox).textBox.text);
                var lowTemperatureAlarmSetpoint = Convert.ToSingle ((settings["Low Alarm"] as SettingsTextBox).textBox.text);
                var temperatureSetpoint = Convert.ToSingle ((settings["Setpoint"] as SettingsTextBox).textBox.text);
                var temperatureDeadband = Convert.ToSingle ((settings["Deadband"] as SettingsTextBox).textBox.text);

                Temperature.SetTemperatureGroupHighTemperatureAlarmSetpoint (groupName, highTemperatureAlarmSetpoint);
                Temperature.SetTemperatureGroupLowTemperatureAlarmSetpoint (groupName, lowTemperatureAlarmSetpoint);
                Temperature.SetTemperatureGroupTemperatureSetpoint (groupName, temperatureSetpoint);
                Temperature.SetTemperatureGroupTemperatureDeadband (groupName, temperatureDeadband);

                JArray ja = jo["temperatureGroups"] as JArray;

                int arrIdx = -1;
                for (int i = 0; i < ja.Count; ++i) {
                    string n = (string)ja[i]["name"];
                    if (groupName == n) {
                        arrIdx = i;
                        break;
                    }
                }

                if (arrIdx == -1) {
                    MessageBox.Show ("Something went wrong");
                    return false;
                }

                ja[arrIdx]["highTemperatureAlarmSetpoint"] = highTemperatureAlarmSetpoint.ToString ();
                ja[arrIdx]["lowTemperatureAlarmSetpoint"] = lowTemperatureAlarmSetpoint.ToString ();
                ja[arrIdx]["temperatureSetpoint"] = temperatureSetpoint.ToString ();
                ja[arrIdx]["temperatureDeadband"] = temperatureDeadband.ToString ();
            }

            File.WriteAllText (path, jo.ToString ());

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

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            JArray ja = jo["temperatureGroups"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja[i]["name"];
                if (groupName == n) {
                    arrIdx = i;
                    break;
                }
            }

            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ((JArray)jo["temperatureGroups"]).RemoveAt (arrIdx);

            File.WriteAllText (path, jo.ToString ());

            Temperature.RemoveTemperatureGroup (groupName);

            return true;
        }
    }
}

