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
using AquaPic.Globals;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class HeaterSettings : TouchSettingsDialog
    {
        string heaterName;
        public string newOrUpdatedHeaterName {
            get {
                return heaterName;
            }
        }

        public HeaterSettings (string heaterName, bool includeDelete, Window parent)
            : base (heaterName, includeDelete, parent) {
            this.heaterName = heaterName;

            var t = new SettingsTextBox ("Name");
            if (this.heaterName.IsNotEmpty ()) {
                t.textBox.text = this.heaterName;
            } else {
                t.textBox.text = "Enter name";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!Temperature.HeaterNameOk (args.text)) {
                    MessageBox.Show ("Heater name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Outlet");
            if (this.heaterName.IsNotEmpty ()) {
                IndividualControl ic = Temperature.GetHeaterIndividualControl (this.heaterName);
                string psName = ic.Group;
                c.combo.comboList.Add (string.Format ("Current: {0}.p{1}", psName, ic.Individual));
                c.combo.activeIndex = 0;
            } else {
                c.combo.nonActiveMessage = "Select outlet";
            }
            c.combo.comboList.AddRange (Power.GetAllAvailableOutlets ());
            AddSetting (c);

            c = new SettingsComboBox ("Temperature Group");
            c.combo.comboList.AddRange (Temperature.GetAllTemperatureGroupNames ());
            c.combo.nonActiveMessage = "Select group";
            if (this.heaterName.IsNotEmpty ()) {
                c.combo.activeText = Temperature.GetHeaterTemperatureGroupName (this.heaterName);
            }
            AddSetting (c);

            DrawSettings ();
        }

        protected void ParseOutlet (string s, ref string g, ref int i) {
            int idx = s.IndexOf ('.');
            string powerStripName = s.Substring (0, idx);
            g = powerStripName;
            i = Convert.ToByte (s.Substring (idx + 2));
        }

        protected override bool OnSave (object sender) {
            var unparseOutletString = (settings["Outlet"] as SettingsComboBox).combo.activeText;
            var name = (settings["Name"] as SettingsTextBox).textBox.text;
            var temperatureGroupName = (settings["Temperature Group"] as SettingsComboBox).combo.activeText;

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            var jo = (JObject)JToken.Parse (json);

            if (heaterName.IsEmpty ()) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid heater name");
                    return false;
                }

                if ((settings["Outlet"] as SettingsComboBox).combo.activeIndex == -1) {
                    MessageBox.Show ("Please select an outlet");
                    return false;
                }

                if ((settings["Temperature Group"] as SettingsComboBox).combo.activeIndex == -1) {
                    MessageBox.Show ("Please select an temperature group");
                    return false;
                }

                IndividualControl ic = IndividualControl.Empty;
                ParseOutlet (unparseOutletString, ref ic.Group, ref ic.Individual);

                Temperature.AddHeater (name, ic, temperatureGroupName);

                JObject jobj = new JObject ();

                jobj.Add (new JProperty ("name", name));
                jobj.Add (new JProperty ("powerStrip", ic.Group));
                jobj.Add (new JProperty ("outlet", ic.Individual.ToString ()));
                jobj.Add (new JProperty ("temperatureGroup", temperatureGroupName));

                ((JArray)jo["heaters"]).Add (jobj);

                heaterName = name;
            } else {
                if ((settings["Temperature Group"] as SettingsComboBox).combo.activeIndex == -1) {
                    MessageBox.Show ("Please select an temperature group");
                    return false;
                }

                string oldName = heaterName;
                if (heaterName != name) {
                    Temperature.SetHeaterName (heaterName, name);
                    heaterName = name;
                }

                IndividualControl ic = IndividualControl.Empty;
                if (!unparseOutletString.StartsWith ("Current:")) {
                    ParseOutlet (unparseOutletString, ref ic.Group, ref ic.Individual);
                    Temperature.SetHeaterIndividualControl (heaterName, ic);
                } else {
                    ic = Temperature.GetHeaterIndividualControl (heaterName);
                }

                string oldTemperatureGroup = Temperature.GetHeaterTemperatureGroupName (heaterName);
                if (oldTemperatureGroup != temperatureGroupName) {
                    Temperature.SetHeaterTemperatureGroupName (heaterName, temperatureGroupName);
                }

                JArray ja = jo["heaters"] as JArray;

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

                ja[arrIdx]["name"] = name;
                ja[arrIdx]["powerStrip"] = ic.Group;
                ja[arrIdx]["outlet"] = ic.Individual.ToString ();
                ja[arrIdx]["temperatureGroup"] = temperatureGroupName;
            }

            File.WriteAllText (path, jo.ToString ());

            return true;
        }

        protected override bool OnDelete (object sender) {
            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            JArray ja = jo["heaters"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja[i]["name"];
                if (heaterName == n) {
                    arrIdx = i;
                    break;
                }
            }

            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ((JArray)jo["heaters"]).RemoveAt (arrIdx);

            File.WriteAllText (path, jo.ToString ());

            Temperature.RemoveHeater (heaterName);

            return true;
        }
    }
}

