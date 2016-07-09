using System;
using System.Collections.Generic;
using System.IO;
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TouchWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Utilites;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class SwitchSettings : TouchSettingsDialog
    {
        int switchId;

        public SwitchSettings (string name, int switchId, bool includeDelete) : base (name, includeDelete) {
            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;
            this.switchId = switchId;

            var t = new SettingTextBox ();
            t.text = "Name";
            if (switchId != -1)
                t.textBox.text = WaterLevel.GetFloatSwitchName (switchId);
            else
                t.textBox.text = "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!WaterLevel.FloatSwitchNameOk (args.text)) {
                    MessageBox.Show ("Switch name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingComboBox ();
            c.text = "Input";
            if (this.switchId != -1) {
                IndividualControl ic = WaterLevel.GetFloatSwitchIndividualControl (switchId);
                string cardName = AquaPicDrivers.DigitalInput.GetCardName (ic.Group);
                c.combo.comboList.Add (string.Format ("Current: {0}.i{1}", cardName, ic.Individual));
                c.combo.active = 0;
            } else {
                c.combo.nonActiveMessage = "Please select channel";
            }
            c.combo.comboList.AddRange (AquaPicDrivers.DigitalInput.GetAllAvaiableChannels ());
            AddSetting (c);

            t = new SettingTextBox ();
            t.text = "Physical Level";
            if (switchId != -1)
                t.textBox.text = WaterLevel.GetFloatSwitchPhysicalLevel (switchId).ToString ();
            else
                t.textBox.text = "0.0";
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

            c = new SettingComboBox ();
            c.text = "Type";
            string[] types = Enum.GetNames (typeof(SwitchType));
            c.combo.comboList.AddRange (types);
            if (this.switchId != -1) {
                string type = WaterLevel.GetFloatSwitchType (switchId).ToString ();
                for (int i = 0; i < types.Length; ++i) {
                    if (type == types [i]) {
                        c.combo.active = i;
                        break;
                    }
                }
            } else {
                c.combo.nonActiveMessage = "Please select type";
            }
            AddSetting (c);

            c = new SettingComboBox ();
            c.text = "Function";
            string[] functions = Enum.GetNames (typeof(SwitchFunction));
            c.combo.comboList.AddRange (functions);
            c.combo.comboList.Remove ("None");
            if (this.switchId != -1) {
                string function = WaterLevel.GetFloatSwitchFunction (switchId).ToString ();
                for (int i = 0; i < c.combo.comboList.Count; ++i) {
                    if (function == c.combo.comboList [i]) {
                        c.combo.active = i;
                        break;
                    }
                }
            } else {
                c.combo.nonActiveMessage = "Please select function";
            }
            AddSetting (c);

            t = new SettingTextBox ();
            t.text = "Time Offset";
            if (switchId != -1)
                t.textBox.text = string.Format ("{0} secs", WaterLevel.GetFloatSwitchTimeOffset (switchId) / 1000);
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

            DrawSettings ();
        }

        protected void ParseChannnel (string s, ref int g, ref int i) {
            int idx = s.IndexOf ('.');
            string cardName = s.Substring (0, idx);
            g = AquaPicDrivers.DigitalInput.GetCardIndex (cardName);
            i = Convert.ToInt32 (s.Substring (idx + 2));
        }

        protected bool OnSave (object sender) {
            string name = ((SettingTextBox)settings ["Name"]).textBox.text;

            string chName = ((SettingComboBox)settings ["Input"]).combo.activeText;
            IndividualControl ic = new IndividualControl ();

            float physicalLevel = Convert.ToSingle (((SettingTextBox)settings ["Physical Level"]).textBox.text);

            string typeString = ((SettingComboBox)settings ["Type"]).combo.activeText;
            SwitchType type;

            string functionString = ((SettingComboBox)settings ["Function"]).combo.activeText;
            SwitchFunction function;

            uint timeOffset = 0;
            string timeOffsetString = ((SettingTextBox)settings ["Time Offset"]).textBox.text;
            if (timeOffsetString != "Enter time") {
                int idx = timeOffsetString.IndexOf ("secs", StringComparison.InvariantCultureIgnoreCase);
                if (idx != -1)
                    timeOffsetString = timeOffsetString.Substring (0, idx);
                
                timeOffset = Convert.ToUInt32 (timeOffsetString) * 1000;
            }

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            if (switchId == -1) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid probe name");
                    return false;
                }

                if (((SettingComboBox)settings ["Input"]).combo.active == -1) {
                    MessageBox.Show ("Please select an channel");
                    return false;
                }

                ParseChannnel (chName, ref ic.Group, ref ic.Individual);

                if (physicalLevel <= 0.0f) {
                    MessageBox.Show ("Physical level can not be less than or equal to 0");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace (typeString))
                    type = (SwitchType)Enum.Parse (typeof(SwitchType), typeString);
                else {
                    MessageBox.Show ("Please select switch type");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace (functionString))
                    function = (SwitchFunction)Enum.Parse (typeof(SwitchFunction), functionString);
                else {
                    MessageBox.Show ("Please select switch function");
                    return false;
                }

                if (timeOffsetString == "Enter time") {
                    MessageBox.Show ("Please enter delay time for float switch");
                    return false;
                }

                try {
                    WaterLevel.AddFloatSwitch (name, ic, physicalLevel, type, function, timeOffset);
                } catch (Exception ex) {
                    MessageBox.Show (ex.Message);
                    return false;
                }

                switchId = WaterLevel.GetFloatSwitchIndex (name);

                JObject jobj = new JObject ();

                jobj.Add (new JProperty ("name", WaterLevel.GetFloatSwitchName (switchId)));
                jobj.Add (new JProperty ("inputCard", AquaPicDrivers.DigitalInput.GetCardName (ic.Group))); 
                jobj.Add (new JProperty ("channel", ic.Individual.ToString ()));
                jobj.Add (new JProperty ("physicalLevel", WaterLevel.GetFloatSwitchPhysicalLevel (switchId).ToString ()));
                jobj.Add (new JProperty ("switchType", WaterLevel.GetFloatSwitchType (switchId).ToString ())); 
                jobj.Add (new JProperty ("switchFuntion", WaterLevel.GetFloatSwitchFunction (switchId).ToString ()));
                jobj.Add (new JProperty ("timeOffset", string.Format ("00:{0:D2}:{1:D2}", WaterLevel.GetFloatSwitchTimeOffset (switchId) / 1000, WaterLevel.GetFloatSwitchTimeOffset (switchId) % 1000)));

                ((JArray)jo ["floatSwitches"]).Add (jobj);
            } else {
                string oldName = WaterLevel.GetFloatSwitchName (switchId);

                if (oldName != name)
                    WaterLevel.SetFloatSwitchName (switchId, name);

                if (!chName.StartsWith ("Current:")) {
                    ParseChannnel (chName, ref ic.Group, ref ic.Individual);
                    WaterLevel.SetFloatSwitchIndividualControl (switchId, ic);
                }

                WaterLevel.SetFloatSwitchPhysicalLevel (switchId, physicalLevel);

                type = (SwitchType)Enum.Parse (typeof(SwitchType), typeString);
                WaterLevel.SetFloatSwitchType (switchId, type);

                function = (SwitchFunction)Enum.Parse (typeof(SwitchFunction), functionString);
                try {
                    if (function != WaterLevel.GetFloatSwitchFunction (switchId))
                        WaterLevel.SetFloatSwitchFunction (switchId, function);
                } catch {
                    MessageBox.Show ("Function already exists");
                    return false;
                }

                WaterLevel.SetFloatSwitchTimeOffset (switchId, timeOffset);

                JArray ja = jo ["floatSwitches"] as JArray;

                if (ja == null) {
                    Console.WriteLine ("ja is null");
                    return false;
                }

                int arrIdx = -1;
                for (int i = 0; i < ja.Count; ++i) {
                    string n = (string)ja [i] ["name"];
                    if (oldName == n) {
                        arrIdx = i;
                        break;
                    }
                }

                if (arrIdx == -1) {
                    MessageBox.Show ("Something went wrong");
                    return false;
                }

                ((JArray)jo ["floatSwitches"]) [arrIdx] ["name"] = WaterLevel.GetFloatSwitchName (switchId);
                ic = WaterLevel.GetFloatSwitchIndividualControl (switchId);
                ((JArray)jo ["floatSwitches"]) [arrIdx] ["inputCard"] = AquaPicDrivers.DigitalInput.GetCardName (ic.Group);
                ((JArray)jo ["floatSwitches"]) [arrIdx] ["channel"] = ic.Individual.ToString ();
                ((JArray)jo ["floatSwitches"]) [arrIdx] ["physicalLevel"] = WaterLevel.GetFloatSwitchPhysicalLevel (switchId).ToString ();
                ((JArray)jo ["floatSwitches"]) [arrIdx] ["switchType"] = WaterLevel.GetFloatSwitchType (switchId).ToString ();
                ((JArray)jo ["floatSwitches"]) [arrIdx] ["switchFuntion"] = WaterLevel.GetFloatSwitchFunction (switchId).ToString ();
                ((JArray)jo ["floatSwitches"]) [arrIdx] ["timeOffset"] = string.Format ("00:{0:D2}:{1:D2}", WaterLevel.GetFloatSwitchTimeOffset (switchId) / 1000, WaterLevel.GetFloatSwitchTimeOffset (switchId) % 1000);
            }

            File.WriteAllText (path, jo.ToString ());

            return true;
        }

        protected bool OnDelete (object sender) {
            string name = WaterLevel.GetFloatSwitchName (switchId);

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            JArray ja = jo ["floatSwitches"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja [i] ["name"];
                if (name == n) {
                    arrIdx = i;
                    break;
                }
            }

            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ((JArray)jo ["floatSwitches"]).RemoveAt (arrIdx);

            File.WriteAllText (path, jo.ToString ());

            WaterLevel.RemoveFloatSwitch (switchId);

            return true;
        }
    }
}


