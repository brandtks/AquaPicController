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

            if (heaterIdx == -1) {
                if (((SettingComboBox)settings ["Outlet"]).combo.Active != -1) {
                    string name = ((SettingTextBox)settings ["Name"]).textBox.text;
                    if (name == "Enter name") {
                        MessageBox.Show ("Invalid heater name");
                        return false;
                    }

                    IndividualControl ic = new IndividualControl ();
                    ParseOutlet (str, ref ic.Group, ref ic.Individual);

                    Temperature.AddHeater (name, ic.Group, ic.Individual);

                    JObject jo = new JObject ();

                    jo.Add (new JProperty ("name", Temperature.GetHeaterName (Temperature.GetHeaterCount () - 1)));
                    jo.Add (new JProperty ("powerStrip", Power.GetPowerStripName (ic.Group)));
                    jo.Add (new JProperty ("outlet", ic.Individual.ToString ()));

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

                    Console.WriteLine ("joText: {0}", joText);

                    string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
                    path = System.IO.Path.Combine (path, "Settings");
                    path = System.IO.Path.Combine (path, "tempProperties.json");

                    string text = File.ReadAllText (path);

                    string heaterText = "heaters";
                    int start = text.IndexOf (heaterText) + heaterText.Length + 1;
                    string temp = text.Substring (start);

                    // this '}' is the very last curly bracket and we want the second to last
                    int end = temp.LastIndexOf ('}');
                    temp = temp.Substring (0, end);
                    //this will the the second to last curly bracket
                    end = temp.LastIndexOf ('}');

                    if (end != -1) { // there are other heaters saved in heaters array
                        joText = joText.Insert (0, ",\n");
                        text = text.Insert (end + start + 1, joText);
                    } else { // first heater to be saved
                        start += (temp.IndexOf ('[') + 1);
                        joText = joText.Insert (0, "\n");
                        end = heaterText.IndexOf ('[');
                        text = text.Insert (end + start + 1, joText);
                    }

                    File.WriteAllText (path, text);
                } else {
                    MessageBox.Show ("Please select an outlet");
                    return false;
                }
            } else if (!str.StartsWith ("Current:")) {
                string name = ((SettingTextBox)settings ["Name"]).textBox.text;
                Temperature.SetHeaterName (heaterIdx, name);

                IndividualControl ic = Temperature.GetHeaterIndividualControl (heaterIdx);
                int previousOutletId = ic.Individual;
                ParseOutlet (str, ref ic.Group, ref ic.Individual);

                Temperature.SetHeaterIndividualControl (heaterIdx, ic);

                JObject jo = new JObject ();

                jo.Add (new JProperty ("name", Temperature.GetHeaterName (heaterIdx)));
                jo.Add (new JProperty ("powerStrip", Power.GetPowerStripName (ic.Group)));
                jo.Add (new JProperty ("outlet", ic.Individual.ToString ()));

                string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
                path = System.IO.Path.Combine (path, "Settings");
                path = System.IO.Path.Combine (path, "tempProperties.json");

                string text = File.ReadAllText (path);

                int start = text.IndexOf (string.Format ("\"name\": \"{0}\"", Temperature.GetHeaterName (heaterIdx)));
                string endSearch = string.Format ("\"outlet\": \"{0}\"", previousOutletId);
                int endLength = endSearch.Length;
                int end = text.IndexOf (endSearch) + endLength;
                string globSet = text.Substring (start, end - start);
                 
                string joText = jo.ToString ();
                int jStart = joText.IndexOf ('"');
                int jEnd = joText.LastIndexOf ('"');
                joText = joText.Substring (jStart, jEnd - jStart + 1);

                text = text.Replace (globSet, joText);

                File.WriteAllText (path, text);
            }

            return true;
        }

        protected bool OnDelete (object sender) {
            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string text = File.ReadAllText (path);

            string startSearch = string.Format ("\"name\": \"{0}\"", Temperature.GetHeaterName (heaterIdx));
            int start = text.IndexOf (startSearch);

            string endSearch = string.Format ("\"outlet\": \"{0}\"", Temperature.GetHeaterIndividualControl (heaterIdx).Individual);
            int endLength = endSearch.Length;
            int objectEnd = text.IndexOf (endSearch, start) + endLength - 1;

            //finds the open curly backet that starts the next heater's object
            int end = text.IndexOf ('{', objectEnd);
            if (end == -1) { //this heater object is the last in the array
                end = text.IndexOf ('}', objectEnd);
                //grabs all the text before this heater's object
                string temp = text.Substring (0, start);
                //finds the last common which seperates this heater from the previous
                start = temp.LastIndexOf (',');

                //makes sure this isn't the last heater we are deleting
                int lastObjectCheck = temp.LastIndexOf ('[');
                if (start < lastObjectCheck)
                    start = lastObjectCheck + 1;
            } else {
                //subtact one from the index since it currently points to the curly bracket for the next heater object
                --end;
                //removes all other heater objects so we can find the opening curly bracket
                string deleteText = text.Substring (0, end);
                //the last opening curly bracket will be the beginning of this heater's object
                start = deleteText.LastIndexOf ('{');
            }

            text = text.Remove (start, end - start + 1);

            File.WriteAllText (path, text);

            Temperature.RemoveHeater (heaterIdx);

            return true;
        }
    }
}

