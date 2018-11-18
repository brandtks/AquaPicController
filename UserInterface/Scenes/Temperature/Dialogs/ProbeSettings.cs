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
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class ProbeSettings : TouchSettingsDialog
    {
        string probeName;
        public string newOrUpdatedProbeName {
            get {
                return probeName;
            }
        }

        public ProbeSettings (string name, bool includeDelete, Window parent)
            : base (name, includeDelete, parent) {
            probeName = name;

            var t = new SettingsTextBox ("Name");
            if (probeName.IsNotEmpty ()) {
                t.textBox.text = probeName;
            } else {
                t.textBox.text = "Enter name";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!Temperature.TemperatureProbeNameOk (args.text)) {
                    MessageBox.Show ("Probe name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Input Channel");
            if (probeName.IsNotEmpty ()) {
                IndividualControl ic = Temperature.GetTemperatureProbeIndividualControl (this.probeName);
                string cardName = ic.Group;
                c.combo.comboList.Add (string.Format ("Current: {0}.i{1}", cardName, ic.Individual));
                c.combo.activeIndex = 0;
            } else {
                c.combo.nonActiveMessage = "Please select channel";
            }
            c.combo.comboList.AddRange (AquaPicDrivers.AnalogInput.GetAllAvaiableChannels ());
            AddSetting (c);

            c = new SettingsComboBox ("Temperature Group");
            c.combo.comboList.AddRange (Temperature.GetAllTemperatureGroupNames ());
            c.combo.nonActiveMessage = "Select group";
            if (probeName.IsNotEmpty ()) {
                c.combo.activeText = Temperature.GetTemperatureProbeTemperatureGroupName (this.probeName);
            }
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
            var unparseChannelString = (string)settings["Input Channel"].setting;
            var ic = IndividualControl.Empty;
            var temperatureGroupName = (string)settings["Temperature Group"].setting;

            var jo = SettingsHelper.OpenSettingsFile ("tempProperties") as JObject;
            var ja = jo["temperatureProbes"] as JArray;

            if (probeName.IsEmpty ()) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid probe name");
                    return false;
                }

                if (unparseChannelString.IsEmpty ()) {
                    MessageBox.Show ("Please select an channel");
                    return false;
                }

                if (temperatureGroupName.IsEmpty ()) {
                    MessageBox.Show ("Please select an temperature group");
                    return false;
                }

                ParseChannnel (unparseChannelString, ref ic.Group, ref ic.Individual);

                Temperature.AddTemperatureProbe (
                    name,
                    ic,
                    32.0f,
                    0.0f,
                    100.0f,
                    4095.0f,
                    temperatureGroupName);

                var jobj = new JObject {
                    new JProperty ("name", name),
                    new JProperty ("inputCard", ic.Group),
                    new JProperty ("channel", ic.Individual.ToString ()),
                    new JProperty ("zeroCalibrationActual", "32.0"),
                    new JProperty ("zeroCalibrationValue", "82.0"),
                    new JProperty ("fullScaleCalibrationActual", "100.0"),
                    new JProperty ("fullScaleCalibrationValue", "4095.0"),
                    new JProperty ("temperatureGroup", temperatureGroupName)
                };

                ja.Add (jobj);
                probeName = name;
            } else {
                // Get the oldName for searching the settings file
                var oldName = probeName;
                if (probeName != name) {
                    Temperature.SetTemperatureProbeName (probeName, name);
                    probeName = name;
                }

                if (!unparseChannelString.StartsWith ("Current:")) {
                    ParseChannnel (unparseChannelString, ref ic.Group, ref ic.Individual);
                    Temperature.SetTemperatureProbeIndividualControl (probeName, ic);
                } else {
                    ic = Temperature.GetTemperatureProbeIndividualControl (probeName);
                }

                Temperature.SetTemperatureProbeTemperatureGroupName (probeName, temperatureGroupName);

                int arrayIndex = SettingsHelper.FindSettingsInArray (ja, oldName);
                if (arrayIndex == -1) {
                    MessageBox.Show ("Something went wrong");
                    return false;
                }

                ja[arrayIndex]["name"] = name;
                ja[arrayIndex]["inputCard"] = ic.Group;
                ja[arrayIndex]["channel"] = ic.Individual.ToString ();
                ja[arrayIndex]["temperatureGroup"] = temperatureGroupName;
            }

            SettingsHelper.SaveSettingsFile ("tempProperties", jo);
            return true;
        }

        protected override bool OnDelete (object sender) {
            var jo = SettingsHelper.OpenSettingsFile ("tempProperties") as JObject;
            var ja = jo["temperatureProbes"] as JArray;

            int arrayIndex = SettingsHelper.FindSettingsInArray (ja, probeName);
            if (arrayIndex == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ja.RemoveAt (arrayIndex);
            SettingsHelper.SaveSettingsFile ("tempProperties", jo);
            Temperature.RemoveTemperatureProbe (probeName);
            return true;
        }
    }
}

