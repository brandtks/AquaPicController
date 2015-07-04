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
            DeleteEvent += OnDelete;

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
                    IndividualControl ic = new IndividualControl ();
                    ParseOutlet (str, ref ic.Group, ref ic.Individual);

                    Temperature.AddHeater (ic.Group, ic.Individual);

                    JObject jo = new JObject ();

                    jo.Add (new JProperty ("name", Temperature.GetHeaterName (Temperature.GetHeaterCount () - 1)));
                    jo.Add (new JProperty ("powerStrip", Power.GetPowerStripName (ic.Group)));
                    jo.Add (new JProperty ("outlet", ic.Individual.ToString ()));

                    string joText = jo.ToString ();

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
            int start = text.IndexOf (string.Format ("\"name\": \"{0}\"", Temperature.GetHeaterName (heaterIdx)));
            string endSearch = string.Format ("\"outlet\": \"{0}\"", Temperature.GetHeaterIndividualControl (heaterIdx).Individual);
            int endLength = endSearch.Length;
            int end = text.IndexOf (endSearch) + endLength - 1;
            end = text.IndexOf ('}', end);
            if (text [end + 1] == ',')
                ++end;

            string deleteText = text.Substring (0, end);
            start = deleteText.LastIndexOf ('{');

            text = text.Remove (start, end - start + 1);

            File.WriteAllText (path, text);

            Temperature.RemoveHeater (heaterIdx);

            return true;
        }
    }
}

