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
using AquaPic.Globals;
using AquaPic.Drivers;
using AquaPic.Runtime;

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

        public AnalogSensorSettings (string name, bool includeDelete)
            : base (name, includeDelete)
        {
            analogSensorName = name;
            var analogSensorNameNotEmpty = analogSensorName.IsNotEmpty ();

            var t = new SettingsTextBox ("Name");
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

            var c = new SettingsComboBox ("Input Channel");
            string[] availCh = AquaPicDrivers.AnalogInput.GetAllAvaiableChannels ();
            if (analogSensorNameNotEmpty) {
                IndividualControl ic = WaterLevel.GetAnalogLevelSensorIndividualControl (name);
                if (ic.IsNotEmpty ()) {
                    string chName = ic.Group;
                    chName = string.Format ("{0}.i{1}", chName, ic.Individual);
                    c.combo.comboList.Add (string.Format ("Current: {0}", chName));
                    c.combo.activeIndex = 0;
                }
            }
            c.combo.nonActiveMessage = "Select outlet";
            c.combo.comboList.AddRange (availCh); 
            AddSetting (c);

            c = new SettingsComboBox ("Water Level Group");
            c.combo.comboList.AddRange (WaterLevel.GetAllWaterLevelGroupNames ());
            c.combo.comboList.Add ("None");
            if (analogSensorNameNotEmpty) {
                var currentWaterGroup = WaterLevel.GetAnalogLevelSensorWaterLevelGroupName (name);
                if (currentWaterGroup.IsEmpty ()) {
                    currentWaterGroup = "None";
                }
                c.combo.activeText = currentWaterGroup;
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
            var name = (string)settings["Name"].setting;
            string channelName = (string)settings["Input Channel"].setting;
            var ic = IndividualControl.Empty;

            var groupName = (string)settings["Water Level Group"].setting;
            if (groupName.IsEmpty ()) {
                MessageBox.Show ("Please select an water level group");
                return false;
            }
            if (groupName == "None") {
                groupName = string.Empty;
            }

			var jo = SettingsHelper.OpenSettingsFile ("waterLevelProperties") as JObject;
            var ja = jo["analogSensors"] as JArray;

            if (analogSensorName.IsEmpty ()) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid probe name");
                    return false;
                }

                if (channelName.IsEmpty ()) {
                    MessageBox.Show ("Please select an channel");
                    return false;
                }

                ParseChannnel (channelName, ref ic.Group, ref ic.Individual);

                WaterLevel.AddAnalogLevelSensor (
                    name,
                    groupName, 
                    ic,
                    819.2f,
                    10f,
                    3003.73f
                );

                var jobj = new JObject {
                    new JProperty ("name", name),
                    new JProperty ("waterLevelGroupName", groupName),
                    new JProperty ("inputCard", ic.Group),
                    new JProperty ("channel", ic.Individual.ToString ()),
                    new JProperty ("zeroScaleCalibrationValue", "819.2"),
                    new JProperty ("fullScaleCalibrationActual", "10.0"),
                    new JProperty ("fullScaleCalibrationValue", "3003.73")
                };

                ja.Add (jobj);
                analogSensorName = name;
            } else {
                string oldName = analogSensorName;
                if (analogSensorName != name) {
                    WaterLevel.SetAnalogLevelSensorName (analogSensorName, name);
                    analogSensorName = name;
                }

                WaterLevel.SetAnalogLevelSensorWaterLevelGroupName (analogSensorName, groupName);

                if (!channelName.StartsWith ("Current:")) {
                    ParseChannnel (channelName, ref ic.Group, ref ic.Individual);
                    WaterLevel.SetAnalogLevelSensorIndividualControl (analogSensorName, ic);
                } else {
                    ic = WaterLevel.GetAnalogLevelSensorIndividualControl (analogSensorName);
                }

                int arrIdx = SettingsHelper.FindSettingsInArray (ja, oldName);
                if (arrIdx == -1) {
                    MessageBox.Show ("Something went wrong");
                    return false;
                }

                ja[arrIdx]["name"] = analogSensorName;
                ja[arrIdx]["waterLevelGroupName"] = groupName;
                ja[arrIdx]["inputCard"] = ic.Group;
                ja[arrIdx]["channel"] = ic.Individual.ToString ();
            }

            SettingsHelper.SaveSettingsFile ("waterLevelProperties", jo);
            return true;
        }

        protected override bool OnDelete (object sender) {
			var jo = SettingsHelper.OpenSettingsFile ("waterLevelProperties") as JObject;
            var ja = jo["analogSensors"] as JArray;

            int arrIdx = SettingsHelper.FindSettingsInArray (ja, analogSensorName);
            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ja.RemoveAt (arrIdx);
            SettingsHelper.SaveSettingsFile ("waterLevelProperties", jo);
            WaterLevel.RemoveAnalogLevelSensor (analogSensorName);
            return true;
        }
    }
}

