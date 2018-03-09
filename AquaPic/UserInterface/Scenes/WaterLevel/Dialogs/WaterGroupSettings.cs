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
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;
using AquaPic.Runtime;

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
            : base (name + " Water", includeDelete) 
        {
            groupName = name;

            var t = new SettingsTextBox ("Name");
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

            var b = new SettingsBlank ("blank1");
            AddSetting (b);

            var s = new SettingsSelectorSwitch ("Enable High Alarm");
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

            t = new SettingsTextBox ("High Alarm");
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

            s = new SettingsSelectorSwitch ("Enable Low Alarm");
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

            t = new SettingsTextBox ("Low Alarm");
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

        protected override bool OnSave (object sender) {
            var name = (string)settings["Name"].setting;

            var highAnalogAlarmSetpoint = Convert.ToSingle (settings["High Alarm"].setting);
            var enableHighAnalogAlarm = (int)settings["Enable High Alarm"].setting == 0;
            var lowAnalogAlarmSetpoint = Convert.ToSingle (settings["Low Alarm"].setting);
            var enableLowAnalogAlarm = (int)settings["Enable Low Alarm"].setting == 0;

            JObject jo = SettingsHelper.OpenSettingsFile ("waterLevelProperties");
            var ja = jo["waterLevelGroups"] as JArray;

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

                var jobj = new JObject {
                    new JProperty ("name", name),
                    new JProperty ("highAnalogAlarmSetpoint", highAnalogAlarmSetpoint.ToString ()),
                    new JProperty ("enableHighAnalogAlarm", enableHighAnalogAlarm.ToString ()),
                    new JProperty ("lowAnalogAlarmSetpoint", lowAnalogAlarmSetpoint.ToString ()),
                    new JProperty ("enableLowAnalogAlarm", enableLowAnalogAlarm.ToString ())
                };

                ja.Add (jobj);
                groupName = name;
            } else {
                WaterLevel.SetWaterLevelGroupHighAnalogAlarmSetpoint (groupName, highAnalogAlarmSetpoint);
                WaterLevel.SetWaterLevelGroupHighAnalogAlarmEnable (groupName, enableHighAnalogAlarm);
                WaterLevel.SetWaterLevelGroupLowAnalogAlarmSetpoint (groupName, lowAnalogAlarmSetpoint);
                WaterLevel.SetWaterLevelGroupLowAnalogAlarmEnable (groupName, enableLowAnalogAlarm);


                int arrIdx = SettingsHelper.FindSettingsInArray (ja, groupName);
                if (arrIdx == -1) {
                    MessageBox.Show ("Something went wrong");
                    return false;
                }
                ja[arrIdx]["highAnalogAlarmSetpoint"] = highAnalogAlarmSetpoint.ToString ();
                ja[arrIdx]["enableHighAnalogAlarm"] = enableHighAnalogAlarm.ToString ();
                ja[arrIdx]["lowAnalogAlarmSetpoint"] = lowAnalogAlarmSetpoint.ToString ();
                ja[arrIdx]["enableLowAnalogAlarm"] = enableLowAnalogAlarm.ToString ();
            }

            SettingsHelper.SaveSettingsFile ("waterLevelProperties", jo);
            return true;
        }

        protected override bool OnDelete (object sender) {
            var jo = SettingsHelper.OpenSettingsFile ("waterLevelProperties");
            var ja = jo["waterLevelGroups"] as JArray;

            int arrIdx = SettingsHelper.FindSettingsInArray (ja, groupName);
            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ja.RemoveAt (arrIdx);
            SettingsHelper.SaveSettingsFile ("waterLevelProperties", jo);
            WaterLevel.RemoveWaterLevelGroup (groupName);
            return true;
        }
    }
}

