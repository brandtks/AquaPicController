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
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    public class WaterGroupSettings : TouchSettingsDialog
    {
        string groupName;
        public string waterLevelGroupName {
            get {
                return groupName;
            }
        }

        public WaterGroupSettings (string name, bool includeDelete)
            : base (name + " Water", includeDelete) {
            groupName = name;

            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;

            var t = new SettingsTextBox ();
            t.text = "Name";
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = groupName;
                t.textBox.enableTouch = false;
                t.textBox.TextChangedEvent += (sender, args) => {
                    MessageBox.Show ("Can not change water group name during runtime");
                    args.keepText = false;
                };
            } else {
                t.textBox.text = "Enter name";
                t.textBox.TextChangedEvent += (sender, args) => {
                    if (string.IsNullOrWhiteSpace (args.text))
                        args.keepText = false;
                    else if (!WaterLevel.WaterLevelGroupNameOk (args.text)) {
                        MessageBox.Show ("Water level group name already exists");
                        args.keepText = false;
                    }
                };
            }
            AddSetting (t);

            var s = new SettingSelectorSwitch ();
            s.text = "Enable High Alarm";
            if (groupName.IsNotEmpty ()) {
                if (WaterLevel.GetWaterLevelGroupHighAnalogAlarmEnable (groupName)) {
                    s.selectorSwitch.currentSelected = 0;
                } else {
                    s.selectorSwitch.currentSelected = 1;
                }
            } else {
                s.selectorSwitch.currentSelected = 1;
            }
            AddSetting (s);

            t = new SettingsTextBox ();
            t.text = "High Alarm";
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = WaterLevel.GetWaterLevelGroupHighAnalogAlarmSetpoint (groupName).ToString ();
            } else {
                t.textBox.text = "0.0";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float highAlarmStpnt = Convert.ToSingle (args.text);

                    if (highAlarmStpnt < 0.0f) {
                        MessageBox.Show ("High alarm setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }

                    float lowAlarmStpnt = Convert.ToSingle (((SettingsTextBox)settings["Low Alarm"]).textBox.text);

                    if (lowAlarmStpnt >= highAlarmStpnt) {
                        MessageBox.Show ("Low alarm setpoint can't be greater than or equal to high setpoint");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper high alarm setpoint format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            s = new SettingSelectorSwitch ();
            s.text = "Enable Low Alarm";
            if (groupName.IsNotEmpty ()) {
                if (WaterLevel.GetWaterLevelGroupLowAnalogAlarmEnable (groupName)) {
                    s.selectorSwitch.currentSelected = 0;
                } else {
                    s.selectorSwitch.currentSelected = 1;
                }
            } else {
                s.selectorSwitch.currentSelected = 1;
            }
            AddSetting (s);

            t = new SettingsTextBox ();
            t.text = "Low Alarm";
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = WaterLevel.GetWaterLevelGroupLowAnalogAlarmSetpoint (groupName).ToString ();
            } else {
                t.textBox.text = "0.0";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float lowAlarmStpnt = Convert.ToSingle (args.text);

                    if (lowAlarmStpnt < 0.0f) {
                        MessageBox.Show ("Low alarm setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }

                    float highAlarmStpnt = Convert.ToSingle (((SettingsTextBox)settings["High Alarm"]).textBox.text);

                    if (lowAlarmStpnt >= highAlarmStpnt) {
                        MessageBox.Show ("Low alarm setpoint can't be greater than or equal to high setpoint");
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

        protected bool OnSave (object sender) {
            var name = (settings["Name"] as SettingsTextBox).textBox.text;

            var highAnalogAlarmSetpoint = Convert.ToSingle ((settings["High Alarm"] as SettingsTextBox).textBox.text);
            var enableHighAnalogAlarm = (settings["Enable High Alarm"] as SettingSelectorSwitch).selectorSwitch.currentSelected == 0;
            var lowAnalogAlarmSetpoint = Convert.ToSingle ((settings["Low Alarm"] as SettingsTextBox).textBox.text);
            var enableLowAnalogAlarm = (settings["Enable Low Alarm"] as SettingSelectorSwitch).selectorSwitch.currentSelected == 0;

            var path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            if (groupName.IsEmpty ()) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid water group name");
                    return false;
                }

                WaterLevel.AddWaterLevelGroup (
                    name,
                    highAnalogAlarmSetpoint,
                    enableHighAnalogAlarm,
                    lowAnalogAlarmSetpoint,
                    enableLowAnalogAlarm);

                var jobj = new JObject ();

                jobj.Add (new JProperty ("name", name));
                jobj.Add (new JProperty ("highAnalogAlarmSetpoint", highAnalogAlarmSetpoint.ToString ()));
                jobj.Add (new JProperty ("enableHighAnalogAlarm", enableHighAnalogAlarm.ToString ()));
                jobj.Add (new JProperty ("lowAnalogAlarmSetpoint", lowAnalogAlarmSetpoint.ToString ()));
                jobj.Add (new JProperty ("enableLowAnalogAlarm", enableLowAnalogAlarm.ToString ()));

                (jo["waterLevelGroups"] as JArray).Add (jobj);

                groupName = name;
            } else {
                WaterLevel.SetWaterLevelGroupHighAnalogAlarmSetpoint (groupName, highAnalogAlarmSetpoint);
                WaterLevel.SetWaterLevelGroupHighAnalogAlarmEnable (groupName, enableHighAnalogAlarm);
                WaterLevel.SetWaterLevelGroupLowAnalogAlarmSetpoint (groupName, lowAnalogAlarmSetpoint);
                WaterLevel.SetWaterLevelGroupLowAnalogAlarmEnable (groupName, enableLowAnalogAlarm);

                var ja = jo["waterLevelGroups"] as JArray;
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

                ja[arrIdx]["highAnalogAlarmSetpoint"] = highAnalogAlarmSetpoint.ToString ();
                ja[arrIdx]["enableHighAnalogAlarm"] = enableHighAnalogAlarm.ToString ();
                ja[arrIdx]["lowAnalogAlarmSetpoint"] = lowAnalogAlarmSetpoint.ToString ();
                ja[arrIdx]["enableLowAnalogAlarm"] = enableLowAnalogAlarm.ToString ();
            }

            File.WriteAllText (path, jo.ToString ());

            return true;
        }

        protected bool OnDelete (object sender) {
            var path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            string json = File.ReadAllText (path);
            var jo = (JObject)JToken.Parse (json);

            var ja = jo["waterLevelGroups"] as JArray;
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

            ((JArray)jo["waterLevelGroups"]).RemoveAt (arrIdx);
            File.WriteAllText (path, jo.ToString ());
            WaterLevel.RemoveWaterLevelGroup (groupName);
            return true;
        }
    }
}

