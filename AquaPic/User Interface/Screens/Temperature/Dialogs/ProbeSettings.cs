using System;
using System.IO;
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Utilites;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class ProbeSettings : TouchSettingsDialog
    {
        int probeIdx;

        public ProbeSettings (string name, int probeIdx, bool includeDelete) : base (name, includeDelete) {
            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;
            this.probeIdx = probeIdx;

            var t = new SettingTextBox ();
            t.text = "Name";
            if (probeIdx != -1)
                t.textBox.text = Temperature.GetTemperatureProbeName (probeIdx);
            else
                t.textBox.text = "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!Temperature.TemperatureProbeNameOk (args.text)) {
                    TouchMessageBox.Show ("Probe name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

//            var t = new SettingEntry ();
//            t.text = "Name";
//            if (probeIdx != -1)
//                t.entry.Text = Temperature.GetTemperatureProbeName (probeIdx);
//            else
//                t.entry.Text = "Enter name";
//            t.TextChangedEvent += (sender, args) => {
//                if (string.IsNullOrWhiteSpace (args.text))
//                    args.keepText = false;
//                else if (!Temperature.TemperatureProbeNameOk (args.text)) {
//                    TouchMessageBox.Show ("Probe name already exists");
//                    args.keepText = false;
//                }
//            };
//            AddSetting (t);

            var c = new SettingComboBox ();
            c.label.text = "Input Channel";
            if (this.probeIdx != -1) {
                IndividualControl ic = Temperature.GetTemperatureProbeIndividualControl (probeIdx);
                string cardName = AnalogInput.GetCardName (ic.Group);
                c.combo.List.Add (string.Format ("Current: {0}.i{1}", cardName, ic.Individual));
                c.combo.Active = 0;
            } else {
                c.combo.NonActiveMessage = "Please select channel";
            }
            c.combo.List.AddRange (AnalogInput.GetAllAvaiableChannels ());
            AddSetting (c);

            DrawSettings ();
        }

        protected void ParseChannnel (string s, ref int g, ref int i) {
            int idx = s.IndexOf ('.');
            string cardName = s.Substring (0, idx);
            g = AnalogInput.GetCardIndex (cardName);
            i = Convert.ToInt32 (s.Substring (idx + 2));
        }

        protected bool OnSave (object sender) {
            string str = ((SettingComboBox)settings ["Input Channel"]).combo.activeText;

            if (probeIdx == -1) {
                if (((SettingComboBox)settings ["Input Channel"]).combo.Active != -1) {
                    string name = ((SettingTextBox)settings ["Name"]).textBox.text;
                    if (name == "Enter name") {
                        TouchMessageBox.Show ("Invalid probe name");
                        return false;
                    }

                    IndividualControl ic = new IndividualControl ();
                    ParseChannnel (str, ref ic.Group, ref ic.Individual);

                    Temperature.AddTemperatureProbe (name, ic.Group, ic.Individual);
                    probeIdx = Temperature.GetTemperatureProbeIndex (name);

                    JObject jo = new JObject ();

                    jo.Add (new JProperty ("name", Temperature.GetTemperatureProbeName (probeIdx)));
                    jo.Add (new JProperty ("inputCard", AnalogInput.GetCardName (ic.Group))); 
                    jo.Add (new JProperty ("channel", ic.Individual.ToString ()));

                    string joText = jo.ToString ();

                    //format, aka proper indexing, of joText
                    string nl = Environment.NewLine;
                    int lineEnd = joText.IndexOf (nl);
                    string insert = "    ";
                    int insertIdx = 0;
                    while (lineEnd != -1) {
                        joText = joText.Insert (insertIdx, insert);

                        insertIdx += (lineEnd + insert.Length + nl.Length);

                        string formatter = joText.Substring (insertIdx);
                        lineEnd = formatter.IndexOf (nl);
                    }
                    insertIdx = joText.LastIndexOf ('}');
                    joText = joText.Insert (insertIdx, insert);

                    string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
                    path = System.IO.Path.Combine (path, "Settings");
                    path = System.IO.Path.Combine (path, "tempProperties.json");

                    string text = File.ReadAllText (path);

                    string probeText = "temperatureProbes";
                    int start = text.IndexOf (probeText) + probeText.Length + 1;
                    string temp = text.Substring (start);

                    //probe array is first so ] closes out the probe array
                    int end = temp.IndexOf (']');
                    temp = temp.Substring (0, end);
                    //if curly bracket found there are other probes if not no probes currently saved
                    end = temp.LastIndexOf ('}');

                    if (end != -1) { // there are other probes saved in probe array
                        joText = joText.Insert (0, ",\n");
                        text = text.Insert (end + start + 1, joText);
                    } else { // first probe to be saved
                        start += (temp.IndexOf ('[') + 1);
                        joText = joText.Insert (0, "\n");
                        end = probeText.IndexOf ('[');
                        text = text.Insert (end + start + 1, joText);
                    }

                    File.WriteAllText (path, text);

                } else {
                    TouchMessageBox.Show ("Please select an channel");
                    return false;
                }
            } else if (!str.StartsWith ("Current:")) {
                string name = ((SettingTextBox)settings ["Name"]).textBox.text;
                Temperature.SetTemperatureProbeName (probeIdx, name);

                IndividualControl ic = Temperature.GetTemperatureProbeIndividualControl (probeIdx);
                int previousChannelId = ic.Individual;

                ParseChannnel (str, ref ic.Group, ref ic.Individual);

                Temperature.SetTemperatureProbeIndividualControl (probeIdx, ic);
                
                JObject jo = new JObject ();

                jo.Add (new JProperty ("name", Temperature.GetTemperatureProbeName (probeIdx)));
                jo.Add (new JProperty ("inputCard", AnalogInput.GetCardName (ic.Group))); 
                jo.Add (new JProperty ("channel", ic.Individual.ToString ()));

                string joText = jo.ToString ();
                int jStart = joText.IndexOf ('"');
                int jEnd = joText.LastIndexOf ('"');
                joText = joText.Substring (jStart, jEnd - jStart + 1);

                string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
                path = System.IO.Path.Combine (path, "Settings");
                path = System.IO.Path.Combine (path, "tempProperties.json");

                string text = File.ReadAllText (path);

                int start = text.IndexOf (string.Format ("\"name\": \"{0}\"", Temperature.GetTemperatureProbeName (probeIdx)));
                string endSearch = string.Format ("\"outlet\": \"{0}\"", previousChannelId);
                int endLength = endSearch.Length;
                int end = text.IndexOf (endSearch) + endLength;
                string globSet = text.Substring (start, end - start);

                text = text.Replace (globSet, joText);

                File.WriteAllText (path, text);
            }

            return true;
        }

        protected bool OnDelete (object sender) {
            string name = Temperature.GetTemperatureProbeName (probeIdx);

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string text = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (text);

            JArray ja = jo ["temperatureProbes"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja [i] ["name"];
                if (name == n) {
                    arrIdx = i;
                    break;
                }
            }

            if (arrIdx == -1) {
                TouchMessageBox.Show ("Something went wrong");
                return false;
            }

            ((JArray)jo ["temperatureProbes"]).RemoveAt (arrIdx);

            File.WriteAllText (path, jo.ToString ());

            Temperature.RemoveTemperatureProbe (probeIdx);

            return true;
        }
    }
}


