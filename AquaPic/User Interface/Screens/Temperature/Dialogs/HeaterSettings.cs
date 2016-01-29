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
        int heaterIdx;

        public HeaterSettings (string name, int heaterIdx, bool includeDelete) : base (name, includeDelete) {
            this.heaterIdx = heaterIdx;

            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;

            var t = new SettingTextBox ();
            t.text = "Name";
            if (heaterIdx != -1)
                t.textBox.text = Temperature.GetHeaterName (heaterIdx);
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
            if (heaterIdx != -1) {
                IndividualControl ic = Temperature.GetHeaterIndividualControl (heaterIdx);
                string psName = Power.GetPowerStripName (ic.Group);
                c.combo.List.Add (string.Format ("Current: {0}.p{1}", psName, ic.Individual));
                c.combo.Active = 0;
            } else {
                c.combo.NonActiveMessage = "Select outlet";
            }
            c.combo.List.AddRange (Power.GetAllAvaiblableOutlets ());
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
            string str = ((SettingComboBox)settings ["Outlet"]).combo.activeText;

            string name = ((SettingTextBox)settings ["Name"]).textBox.text;

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            if (heaterIdx == -1) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid heater name");
                    return false;
                }

                if (((SettingComboBox)settings ["Outlet"]).combo.Active == -1) {
                    MessageBox.Show ("Please select an outlet");
                    return false;
                }

                IndividualControl ic = IndividualControl.Empty;
                ParseOutlet (str, ref ic.Group, ref ic.Individual);

                Temperature.AddHeater (name, ic.Group, ic.Individual);
                heaterIdx = Temperature.GetHeaterIndex (name);

                JObject jobj = new JObject ();

                jobj.Add (new JProperty ("name", Temperature.GetHeaterName (heaterIdx)));
                jobj.Add (new JProperty ("powerStrip", Power.GetPowerStripName (ic.Group)));
                jobj.Add (new JProperty ("outlet", ic.Individual.ToString ()));

                ((JArray)jo ["heaters"]).Add (jobj);
            } else {
                string oldName = Temperature.GetHeaterName (heaterIdx);
                if (oldName != name)
                    Temperature.SetHeaterName (heaterIdx, name);
                
                IndividualControl ic = IndividualControl.Empty;
                if (!str.StartsWith ("Current:")) {
                    ParseOutlet (str, ref ic.Group, ref ic.Individual);
                    Temperature.SetHeaterIndividualControl (heaterIdx, ic);
                }

                ic = Temperature.GetHeaterIndividualControl (heaterIdx);

                JArray ja = jo ["heaters"] as JArray;

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

                ((JArray)jo ["heaters"]) [arrIdx] ["name"] = name;
                ((JArray)jo ["heaters"]) [arrIdx] ["powerStrip"] = Power.GetPowerStripName (ic.Group);
                ((JArray)jo ["heaters"]) [arrIdx] ["outlet"] = ic.Individual.ToString ();
            }

            File.WriteAllText (path, jo.ToString ());

            return true;
        }

        protected bool OnDelete (object sender) {
            string name = Temperature.GetHeaterName (heaterIdx);

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            JArray ja = jo ["heaters"] as JArray;

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

            ((JArray)jo ["heaters"]).RemoveAt (arrIdx);

            File.WriteAllText (path, jo.ToString ());

            Temperature.RemoveHeater (heaterIdx);

            return true;
        }
    }
}

