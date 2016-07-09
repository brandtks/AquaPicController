using System;
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
    public class HeaterSettings : TouchSettingsDialog
    {
        int heaterIndex;

        public HeaterSettings (string name, int heaterIndex, bool includeDelete) 
            : base (name, includeDelete) 
        {
            this.heaterIndex = heaterIndex;

            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;

            var t = new SettingTextBox ();
            t.text = "Name";
            if (heaterIndex != -1)
                t.textBox.text = Temperature.GetHeaterName (heaterIndex);
            else
                t.textBox.text = "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!Temperature.HeaterNameOk (args.text)) {
                    MessageBox.Show ("Heater name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingComboBox ();
            c.label.text = "Outlet";
            if (heaterIndex != -1) {
                IndividualControl ic = Temperature.GetHeaterIndividualControl (heaterIndex);
                string psName = Power.GetPowerStripName (ic.Group);
                c.combo.comboList.Add (string.Format ("Current: {0}.p{1}", psName, ic.Individual));
                c.combo.active = 0;
            } else {
                c.combo.nonActiveMessage = "Select outlet";
            }
            c.combo.comboList.AddRange (Power.GetAllAvaiblableOutlets ());
            AddSetting (c);

            c = new SettingComboBox ();
            c.label.text = "Temperature Group";
            c.combo.comboList.AddRange (Temperature.GetAllTemperatureGroupNames ());
            c.combo.nonActiveMessage = "Select group";
            if (heaterIndex != -1) {
                c.combo.activeText = Temperature.GetHeaterTemperatureGroupName (heaterIndex);
            }
            AddSetting (c);

            DrawSettings ();
        }

        protected void ParseOutlet (string s, ref int g, ref int i) {
            int idx = s.IndexOf ('.');
            string psName = s.Substring (0, idx);
            g = (byte)Power.GetPowerStripIndex (psName);
            i = Convert.ToByte (s.Substring (idx + 2));
        }
        
        protected bool OnSave (object sender) {
            var unparseOutletString = (settings ["Outlet"] as SettingComboBox).combo.activeText;
            var name = (settings["Name"] as SettingTextBox).textBox.text;
            var temperatureGroupName = (settings["Temperature Group"] as SettingComboBox).combo.activeText;

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            if (heaterIndex == -1) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid heater name");
                    return false;
                }

                if ((settings["Outlet"] as SettingComboBox).combo.active == -1) {
                    MessageBox.Show ("Please select an outlet");
                    return false;
                }

                if ((settings["Temperature Group"] as SettingComboBox).combo.active == -1) {
                    MessageBox.Show ("Please select an temperature group");
                    return false;
                }

                IndividualControl ic = IndividualControl.Empty;
                ParseOutlet (unparseOutletString, ref ic.Group, ref ic.Individual);

                Temperature.AddHeater (name, ic.Group, ic.Individual, temperatureGroupName);

                JObject jobj = new JObject ();

                jobj.Add (new JProperty ("name", name));
                jobj.Add (new JProperty ("powerStrip", Power.GetPowerStripName (ic.Group)));
                jobj.Add (new JProperty ("outlet", ic.Individual.ToString ()));
                jobj.Add (new JProperty ("temperatureGroup", temperatureGroupName));

                ((JArray)jo["heaters"]).Add (jobj);
            } else {
                if ((settings["Temperature Group"] as SettingComboBox).combo.active == -1) {
                    MessageBox.Show ("Please select an temperature group");
                    return false;
                }
                
                string oldName = Temperature.GetHeaterName (heaterIndex);
                if (oldName != name)
                    Temperature.SetHeaterName (heaterIndex, name);
                
                IndividualControl ic = IndividualControl.Empty;
                if (!unparseOutletString.StartsWith ("Current:")) {
                    ParseOutlet (unparseOutletString, ref ic.Group, ref ic.Individual);
                    Temperature.SetHeaterIndividualControl (heaterIndex, ic);
                } else {
                    ic = Temperature.GetHeaterIndividualControl (heaterIndex);
                }

                string oldTemperatureGroup = Temperature.GetHeaterTemperatureGroupName (heaterIndex);
                if (oldTemperatureGroup != temperatureGroupName) {
                    Temperature.SetHeaterTemperatureGroupName (heaterIndex, temperatureGroupName);
                }

                JArray ja = jo ["heaters"] as JArray;

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
                ja[arrIdx]["powerStrip"] = Power.GetPowerStripName (ic.Group);
                ja[arrIdx]["outlet"] = ic.Individual.ToString ();
                ja[arrIdx]["temperatureGroup"] = temperatureGroupName;
            }

            File.WriteAllText (path, jo.ToString ());

            return true;
        }

        protected bool OnDelete (object sender) {
            string name = Temperature.GetHeaterName (heaterIndex);

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            JArray ja = jo["heaters"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja[i]["name"];
                if (name == n) {
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

            Temperature.RemoveHeater (heaterIndex);

            return true;
        }
    }
}

