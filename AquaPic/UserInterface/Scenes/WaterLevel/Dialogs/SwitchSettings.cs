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
using AquaPic.Globals;
using AquaPic.Drivers;
using AquaPic.Sensors;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class SwitchSettings : TouchSettingsDialog
    {
        string switchName;
        public string newOrUpdatedFloatSwitchName {
            get {
                return switchName;
            }
        }

        public SwitchSettings (string name, bool includeDelete)
            : base (name, includeDelete) {
            switchName = name;

            var t = new SettingsTextBox ("Name");
            if (switchName.IsNotEmpty ()) {
                t.textBox.text = switchName;
            } else {
                t.textBox.text = "Enter name";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!WaterLevel.FloatSwitchNameOk (args.text)) {
                    MessageBox.Show ("Switch name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Input");
            if (switchName.IsNotEmpty ()) {
                IndividualControl ic = WaterLevel.GetFloatSwitchIndividualControl (switchName);
                string cardName = ic.Group;
                c.combo.comboList.Add (string.Format ("Current: {0}.i{1}", cardName, ic.Individual));
                c.combo.activeIndex = 0;
            } else {
                c.combo.nonActiveMessage = "Please select channel";
            }
            c.combo.comboList.AddRange (AquaPicDrivers.DigitalInput.GetAllAvaiableChannels ());
            AddSetting (c);

            t = new SettingsTextBox ("Physical Level");
            if (switchName.IsNotEmpty ()) {
                t.textBox.text = WaterLevel.GetFloatSwitchPhysicalLevel (this.switchName).ToString ();
            } else {
                t.textBox.text = "0.0";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float physicalLevel = Convert.ToSingle (args.text);

                    if (physicalLevel <= 0.0f) {
                        MessageBox.Show ("Physical level can not be less than or equal to 0");
                        args.keepText = false;
                    }

                } catch {
                    MessageBox.Show ("Improper high alarm setpoint format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            c = new SettingsComboBox ("Type");
            string[] types = Enum.GetNames (typeof (SwitchType));
            c.combo.comboList.AddRange (types);
            c.combo.nonActiveMessage = "Please select type";
            if (switchName.IsNotEmpty ()) {
                c.combo.activeText = WaterLevel.GetFloatSwitchType (switchName).ToString ();
            }
            AddSetting (c);

            c = new SettingsComboBox ("Function");
            string[] functions = Enum.GetNames (typeof (SwitchFunction));
            c.combo.comboList.AddRange (functions);
            c.combo.comboList.Remove ("None");
            c.combo.nonActiveMessage = "Please select function";
            if (switchName.IsNotEmpty ()) {
                c.combo.activeText = WaterLevel.GetFloatSwitchFunction (this.switchName).ToString ();
            }
            AddSetting (c);

            t = new SettingsTextBox ("Time Offset");
            if (switchName.IsNotEmpty ())
                t.textBox.text = string.Format ("{0} secs", WaterLevel.GetFloatSwitchTimeOffset (switchName) / 1000);
            else
                t.textBox.text = "Enter time";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                try {
                    int idx = args.text.IndexOf ("secs", StringComparison.InvariantCultureIgnoreCase);
                    uint time;
                    if (idx == -1)
                        time = Convert.ToUInt32 (args.text);
                    else {
                        string timeString = args.text.Substring (0, idx);
                        time = Convert.ToUInt32 (timeString);
                    }

                    args.text = string.Format ("{0} secs", time);
                } catch {
                    MessageBox.Show ("Improper format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            c = new SettingsComboBox ("Water Level Group");
            c.combo.comboList.AddRange (WaterLevel.GetAllWaterLevelGroupNames ());
            c.combo.comboList.Add ("None");
            if (switchName.IsNotEmpty ()) {
                var currentGroupName = WaterLevel.GetFloatSwitchWaterLevelGroupName (switchName);
                if (currentGroupName.IsEmpty ()) {
                    currentGroupName = "None";
                }
                c.combo.activeText = currentGroupName;
            }
            c.combo.nonActiveMessage = "Select group";
            AddSetting (c);

            DrawSettings ();
        }

        protected void ParseChannnel (string s, ref string g, ref int i) {
            int idx = s.IndexOf ('.');
            g = s.Substring (0, idx);
            i = Convert.ToInt32 (s.Substring (idx + 2));
        }

        protected override bool OnSave (object sender) {
            string name = (string)settings["Name"].setting;
            // This check is here instead of only in adding a new switch because we're doing other general checks 
            // and we want error messages to be displayed logically
            if (name == "Enter name") {
                MessageBox.Show ("Invalid probe name");
                return false;
            }

            string channelName = (string)settings["Input"].setting;
            if (channelName.IsEmpty ()) {
                MessageBox.Show ("Please select an channel");
                return false;
            }
            var ic = IndividualControl.Empty;

            float physicalLevel = Convert.ToSingle (settings["Physical Level"].setting);

            string typeString = (string)settings["Type"].setting;
            SwitchType type;
            if (typeString.IsNotEmpty ()) {
                type = (SwitchType)Enum.Parse (typeof (SwitchType), typeString);
            } else {
                MessageBox.Show ("Please select switch type");
                return false;
            }

            string functionString = (string)settings["Function"].setting;
            SwitchFunction function;
            if (functionString.IsNotEmpty ()) {
                function = (SwitchFunction)Enum.Parse (typeof (SwitchFunction), functionString);
            } else {
                MessageBox.Show ("Please select switch function");
                return false;
            }

            if (!CheckFunctionAgainstType (function, type)) {
                return false;
            }

            uint timeOffset = 0;
            string timeOffsetString = (string)settings["Time Offset"].setting;
            if (timeOffsetString != "Enter time") {
                int idx = timeOffsetString.IndexOf ("secs", StringComparison.InvariantCultureIgnoreCase);
                if (idx != -1)
                    timeOffsetString = timeOffsetString.Substring (0, idx);

                timeOffset = Convert.ToUInt32 (timeOffsetString) * 1000;
            } else {
                MessageBox.Show ("Please enter delay time for float switch");
                return false;
            }

            string groupName = (string)settings["Water Level Group"].setting;
            if (groupName.IsEmpty ()) {
                MessageBox.Show ("Please select an water level group");
                return false;
            }
            if (groupName == "None") {
                groupName = string.Empty;
            }

            var jo = SettingsHelper.OpenSettingsFile ("waterLevelProperties") as JObject;
            var ja = jo["floatSwitches"] as JArray;

            if (switchName.IsEmpty ()) {
                ParseChannnel (channelName, ref ic.Group, ref ic.Individual);

                WaterLevel.AddFloatSwitch (name, ic, physicalLevel, type, function, timeOffset, groupName);

                var jobj = new JObject {
                    new JProperty ("name", name),
                    new JProperty ("inputCard", ic.Group),
                    new JProperty ("channel", ic.Individual.ToString ()),
                    new JProperty ("physicalLevel", physicalLevel.ToString ()),
                    new JProperty ("switchType", type.ToString ()),
                    new JProperty ("switchFuntion", function.ToString ()),
                    new JProperty ("timeOffset", string.Format ("00:{0:D2}:{1:D2}", timeOffset / 1000, timeOffset % 1000)),
                    new JProperty ("waterLevelGroupName", groupName)
                };

                ja.Add (jobj);
                switchName = name;
            } else {
                string oldName = switchName;
                if (switchName != name) {
                    WaterLevel.SetFloatSwitchName (switchName, name);
                    switchName = name;
                }

                if (!channelName.StartsWith ("Current:")) {
                    ParseChannnel (channelName, ref ic.Group, ref ic.Individual);
                    WaterLevel.SetFloatSwitchIndividualControl (switchName, ic);
                } else {
                    ic = WaterLevel.GetFloatSwitchIndividualControl (switchName);
                }

                WaterLevel.SetFloatSwitchPhysicalLevel (switchName, physicalLevel);
                WaterLevel.SetFloatSwitchType (switchName, type);
                WaterLevel.SetFloatSwitchFunction (switchName, function);
                WaterLevel.SetFloatSwitchTimeOffset (switchName, timeOffset);
                WaterLevel.SetFloatSwitchWaterLevelGroupName (switchName, groupName);

                int arrIdx = SettingsHelper.FindSettingsInArray (ja, oldName);
                if (arrIdx == -1) {
                    MessageBox.Show ("Something went wrong");
                    return false;
                }

                ja[arrIdx]["name"] = switchName;
                ja[arrIdx]["inputCard"] = ic.Group;
                ja[arrIdx]["channel"] = ic.Individual.ToString ();
                ja[arrIdx]["physicalLevel"] = physicalLevel.ToString ();
                ja[arrIdx]["switchType"] = type.ToString ();
                ja[arrIdx]["switchFuntion"] = function.ToString ();
                ja[arrIdx]["timeOffset"] = string.Format ("00:{0:D2}:{1:D2}", timeOffset / 1000, timeOffset % 1000);
                ja[arrIdx]["waterLevelGroupName"] = groupName;
            }

            SettingsHelper.SaveSettingsFile ("waterLevelProperties", jo);
            return true;
        }

        protected override bool OnDelete (object sender) {
            var jo = SettingsHelper.OpenSettingsFile ("waterLevelProperties") as JObject;
            var ja = jo["floatSwitches"] as JArray;

            int arrIdx = SettingsHelper.FindSettingsInArray (ja, switchName);
            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ja.RemoveAt (arrIdx);
            SettingsHelper.SaveSettingsFile ("waterLevelProperties", jo);
            WaterLevel.RemoveFloatSwitch (switchName);
            return true;
        }

        protected bool CheckFunctionAgainstType (SwitchFunction function, SwitchType type) {
            var message = string.Empty;
            var confirmed = true;

            if ((function == SwitchFunction.HighLevel) && (type != SwitchType.NormallyClosed)) {
                message = "High level switch should be normally closed";
            } else if ((function == SwitchFunction.LowLevel) && (type != SwitchType.NormallyClosed)) {
                message = "Low level switch should be normally closed";
            } else if ((function == SwitchFunction.ATO) && (type != SwitchType.NormallyOpened)) {
                message = "ATO switch should be normally opened";
            }

            if (message.IsNotEmpty ()) {
                var parent = Toplevel as Window;
                if (parent != null) {
                    if (!parent.IsTopLevel)
                        parent = null;
                }

                var ms = new TouchDialog (message + "\n" +
                    "Are you sure you want to use this configuration", parent);

                ms.Response += (o, a) => {
                    if (a.ResponseId == ResponseType.Yes) {
                        confirmed = true;
                    }
                };

                ms.Run ();
                ms.Destroy ();
            }

            return confirmed;
        }
    }
}

