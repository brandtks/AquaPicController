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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;
using AquaPic.Globals;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class AnalogSensorSettings : TouchSettingsDialog
    {
        string analogSensorName;
        public string newOrUpdatedAnalogSensorName {
            get {
                return analogSensorName;
            }
        }

        public AnalogSensorSettings (string analogSensorName, bool includeDelete)
            : base (analogSensorName, includeDelete)
        {
            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;
            this.analogSensorName = analogSensorName;
            var analogSensorNameNotEmpty = analogSensorName.IsNotEmpty ();

            var t = new SettingsTextBox ();
            t.text = "Name";
            if (analogSensorNameNotEmpty) {
                t.textBox.text = analogSensorName;
            } else {
                t.textBox.text = "Enter name";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!WaterLevel.AnalogLevelSensorNameOk (args.text)) {
                    MessageBox.Show ("Switch name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var s = new SettingSelectorSwitch ();
            s.text = "Enable";
            if (analogSensorNameNotEmpty) {
                if (WaterLevel.GetAnalogLevelSensorEnable (analogSensorName)) {
                    s.selectorSwitch.currentSelected = 0;
                    showOptional = true;
                } else {
                    s.selectorSwitch.currentSelected = 1;
                    showOptional = false;
                }
            } else {
                s.selectorSwitch.currentSelected = 1;
                showOptional = false;
            }
            s.selectorSwitch.SelectorChangedEvent += (sender, args) => {
                if (args.currentSelectedIndex == 0)
                    showOptional = true;
                else
                    showOptional = false;

                UpdateSettingsVisibility ();
            };
            AddSetting (s);

            var c = new SettingsComboBox ();
            c.label.text = "Water Level Group";
            c.combo.comboList.AddRange (WaterLevel.GetAllWaterLevelGroupNames ());
            c.combo.nonActiveMessage = "Select group";
            if (analogSensorNameNotEmpty) {
                c.combo.activeText = WaterLevel.GetAnalogLevelSensorWaterLevelGroupName (analogSensorName);
            }
            AddOptionalSetting (c);

            c = new SettingsComboBox ();
            c.text = "Input";
            string[] availCh = AquaPicDrivers.AnalogInput.GetAllAvaiableChannels ();
            if (analogSensorNameNotEmpty) {
                IndividualControl ic = WaterLevel.GetAnalogLevelSensorIndividualControl (analogSensorName);
                if (WaterLevel.GetAnalogLevelSensorEnable (analogSensorName) || ic.IsNotEmpty ()) {
                    string chName = AquaPicDrivers.AnalogInput.GetCardName (ic.Group);
                    chName = string.Format ("{0}.i{1}", chName, ic.Individual);
                    c.combo.comboList.Add (string.Format ("Current: {0}", chName));
                    c.combo.activeIndex = 0;
                }
            }
            c.combo.nonActiveMessage = "Select outlet";
            c.combo.comboList.AddRange (availCh); 
            AddOptionalSetting (c);

            DrawSettings ();
        }

        protected void ParseChannnel (string s, ref int g, ref int i) {
            int idx = s.IndexOf ('.');
            string cardName = s.Substring (0, idx);
            g = AquaPicDrivers.AnalogInput.GetCardIndex (cardName);
            i = Convert.ToInt32 (s.Substring (idx + 2));
        }

        protected bool OnSave (object sender) {
            string name = (settings["Name"] as SettingsTextBox).textBox.text;

            var s = settings["Enable"] as SettingSelectorSwitch;
            var enable = s.selectorSwitch.currentSelected == 0;

            var waterLevelGroupName = (settings["Water Level Group"] as SettingsComboBox).combo.activeText;

            string chName = (settings["Input"] as SettingsComboBox).combo.activeText;
            var ic = IndividualControl.Empty;

            var path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            string jstring = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (jstring);

            if (analogSensorName.IsEmpty ()) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid probe name");
                    return false;
                }

                if (enable) {
                    if ((settings["Water Level Group"] as SettingsComboBox).combo.activeIndex == -1) {
                        MessageBox.Show ("Please select an water level group");
                        return false;
                    }

                    if ((settings["Input"] as SettingsComboBox).combo.activeIndex == -1) {
                        MessageBox.Show ("Please select an channel");
                        return false;
                    }

                    ParseChannnel (chName, ref ic.Group, ref ic.Individual);
                }

                try {
                    WaterLevel.AddAnalogLevelSensor (
                        name,
                        enable,
                        waterLevelGroupName, 
                        ic,
                        819.2f,
                        10f,
                        3003.73f
                    );
                } catch (Exception ex) {
                    MessageBox.Show (ex.Message);
                    return false;
                }

                var jobj = new JObject ();

                jobj.Add (new JProperty ("name", name));
                jobj.Add (new JProperty ("enable", enable.ToString ()));
                jobj.Add (new JProperty ("waterLevelGroupName", waterLevelGroupName));
                jobj.Add (new JProperty ("inputCard", AquaPicDrivers.AnalogInput.GetCardName (ic.Group)));
                jobj.Add (new JProperty ("channel", ic.Individual.ToString ()));
                jobj.Add (new JProperty ("zeroScaleCalibrationValue", "819.2"));
                jobj.Add (new JProperty ("fullScaleCalibrationActual", "10.0"));
                jobj.Add (new JProperty ("fullScaleCalibrationValue", "3003.73"));

                ((JArray)jo["analogSensors"]).Add (jobj);

                analogSensorName = name;
            } else {
                string oldName = analogSensorName;
                if (analogSensorName != name) {
                    WaterLevel.SetAnalogLevelSensorName (analogSensorName, name);
                    analogSensorName = name;
                }

                if (enable) {
                    WaterLevel.SetAnalogLevelSensorWaterLevelGroupName (analogSensorName, waterLevelGroupName);
                    var str = ((SettingsComboBox)settings["Sensor Channel"]).combo.activeText;
                    if (!str.StartsWith ("Current:")) {
                        ParseChannnel (chName, ref ic.Group, ref ic.Individual);
                    }

                    WaterLevel.SetAnalogLevelSensorIndividualControl (analogSensorName, ic);
                }

                WaterLevel.SetAnalogLevelSensorEnable (analogSensorName, enable);

                JArray ja = jo["analogSensors"] as JArray;
                int arrIdx = -1;
                for (int i = 0; i < ja.Count; ++i) {
                    string n = (string)ja[i]["name"];
                    if (oldName == n) {
                        arrIdx = i;
                        break;
                    }
                }

                if (arrIdx == -1) {
                    MessageBox.Show ("Something went wrong");
                    return false;
                }

                ja[arrIdx]["name"] = analogSensorName;
                ja[arrIdx]["enable"] = enable.ToString ();
                ja[arrIdx]["waterLevelGroupName"] = waterLevelGroupName;
                ja[arrIdx]["inputCard"] = AquaPicDrivers.AnalogInput.GetCardName (ic.Group);
                ja[arrIdx]["channel"] = ic.Individual.ToString ();
            }

            File.WriteAllText (path, jo.ToString ());

            return true;
        }

        protected bool OnDelete (object sender) {
            var path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            JArray ja = jo["analogSensors"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja[i]["name"];
                if (analogSensorName == n) {
                    arrIdx = i;
                    break;
                }
            }

            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ((JArray)jo["analogSensors"]).RemoveAt (arrIdx);
            File.WriteAllText (path, jo.ToString ());
            WaterLevel.RemoveAnalogLevelSensor (analogSensorName);
            return true;
        }
    }
}

