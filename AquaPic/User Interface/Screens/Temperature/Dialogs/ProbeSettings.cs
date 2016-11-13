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
    public class ProbeSettings : TouchSettingsDialog
    {
        string probeName;
        public string newOrUpdatedProbeName {
            get {
                return probeName;
            }
        }

        public ProbeSettings (string probeName, bool includeDelete)
            : base (probeName, includeDelete) 
        {
            this.probeName = probeName;
            
            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;

            var t = new SettingsTextBox ();
            t.text = "Name";
            if (this.probeName.IsNotEmpty ()) {
                t.textBox.text = this.probeName;
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

            var c = new SettingsComboBox ();
            c.label.text = "Input Channel";
            if (this.probeName.IsNotEmpty ()) {
                IndividualControl ic = Temperature.GetTemperatureProbeIndividualControl (this.probeName);
                string cardName = AquaPicDrivers.AnalogInput.GetCardName (ic.Group);
                c.combo.comboList.Add (string.Format ("Current: {0}.i{1}", cardName, ic.Individual));
                c.combo.active = 0;
            } else {
                c.combo.nonActiveMessage = "Please select channel";
            }
            c.combo.comboList.AddRange (AquaPicDrivers.AnalogInput.GetAllAvaiableChannels ());
            AddSetting (c);

            c = new SettingsComboBox ();
            c.label.text = "Temperature Group";
            c.combo.comboList.AddRange (Temperature.GetAllTemperatureGroupNames ());
            c.combo.nonActiveMessage = "Select group";
            if (this.probeName.IsNotEmpty ()) {
                c.combo.activeText = Temperature.GetTemperatureProbeTemperatureGroupName (this.probeName);
            }
            AddSetting (c);

            DrawSettings ();
        }

        protected void ParseChannnel (string s, ref int g, ref int i) {
            int idx = s.IndexOf ('.');
            string cardName = s.Substring (0, idx);
            g = AquaPicDrivers.AnalogInput.GetCardIndex (cardName);
            i = Convert.ToInt32 (s.Substring (idx + 2));
        }

        protected bool OnSave (object sender) {
            string unparseProbeChannelString = (settings["Input Channel"] as SettingsComboBox).combo.activeText;
            string name = (settings["Name"] as SettingsTextBox).textBox.text;
            var temperatureGroupName = (settings["Temperature Group"] as SettingsComboBox).combo.activeText;

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            if (probeName.IsEmpty ()) {
                if ((settings["Input Channel"] as SettingsComboBox).combo.active == -1) {
                    MessageBox.Show ("Please select an channel");
                    return false;
                }

                if (name == "Enter name") {
                    MessageBox.Show ("Invalid probe name");
                    return false;
                }

                if ((settings["Temperature Group"] as SettingsComboBox).combo.active == -1) {
                    MessageBox.Show ("Please select an temperature group");
                    return false;
                }

                IndividualControl ic = new IndividualControl ();
                ParseChannnel (unparseProbeChannelString, ref ic.Group, ref ic.Individual);

                Temperature.AddTemperatureProbe (
                    name, 
                    ic.Group,
                    ic.Individual, 
                    32.0f, 
                    0.0f, 
                    100.0f,
                    4095.0f,
                    temperatureGroupName);

                JObject jobj = new JObject ();

                jobj.Add (new JProperty ("name", name));
                jobj.Add (new JProperty ("inputCard", AquaPicDrivers.AnalogInput.GetCardName (ic.Group)));
                jobj.Add (new JProperty ("channel", ic.Individual.ToString ()));
                jobj.Add (new JProperty ("zeroCalibrationActual", "32.0"));
                jobj.Add (new JProperty ("zeroCalibrationValue", "82.0"));
                jobj.Add (new JProperty ("fullScaleCalibrationActual", "100.0"));
                jobj.Add (new JProperty ("fullScaleCalibrationValue", "4095.0"));
                jobj.Add (new JProperty ("temperatureGroup", temperatureGroupName));

                (jo["temperatureProbes"] as JArray).Add (jobj);

                probeName = name;
            } else {
                if ((settings["Temperature Group"] as SettingsComboBox).combo.active == -1) {
                    MessageBox.Show ("Please select an temperature group");
                    return false;
                }

                var oldName = probeName;
                if (probeName != name) {
                    Temperature.SetTemperatureProbeName (probeName, name);
                    probeName = name;
                }

                IndividualControl ic = IndividualControl.Empty;
                if (!unparseProbeChannelString.StartsWith ("Current:")) {
                    ParseChannnel (unparseProbeChannelString, ref ic.Group, ref ic.Individual);
                    Temperature.SetTemperatureProbeIndividualControl (probeName, ic);
                } else {
                    ic = Temperature.GetTemperatureProbeIndividualControl (probeName);
                }

                string oldTemperatureGroup = Temperature.GetTemperatureProbeTemperatureGroupName (probeName);
                if (oldTemperatureGroup != temperatureGroupName) {
                    Temperature.SetTemperatureProbeTemperatureGroupName (probeName, temperatureGroupName);
                }

                JArray ja = jo["temperatureProbes"] as JArray;

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
                ja[arrIdx]["inputCard"] = AquaPicDrivers.AnalogInput.GetCardName (ic.Group);
                ja[arrIdx]["channel"] = ic.Individual.ToString ();
                ja[arrIdx]["temperatureGroup"] = temperatureGroupName;
            }

            File.WriteAllText (path, jo.ToString ());

            return true;
        }

        protected bool OnDelete (object sender) {
            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string text = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (text);

            JArray ja = jo["temperatureProbes"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja [i] ["name"];
                if (probeName == n) {
                    arrIdx = i;
                    break;
                }
            }

            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ((JArray)jo["temperatureProbes"]).RemoveAt (arrIdx);

            File.WriteAllText (path, jo.ToString ());

            Temperature.RemoveTemperatureProbe (probeName);

            return true;
        }
    }
}


