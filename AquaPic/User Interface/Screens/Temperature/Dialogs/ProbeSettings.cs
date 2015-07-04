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

        public ProbeSettings (int probeIdx) : base ("Temperature Probe") {
            SaveEvent += OnSave;

            this.probeIdx = probeIdx;

            var c = new SettingComboBox ();
            c.label.text = "Input Channel";

            if (this.probeIdx != 1) {
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

        protected bool OnSave (object sender) {
            string s = ((SettingComboBox)settings ["Input Channel"]).combo.activeText;
            if (!s.StartsWith ("Current:")) {
                int idx = s.IndexOf ('.');
                string cardName = s.Substring (0, idx);
                int cardId = AnalogInput.GetCardIndex (cardName);
                byte channelId = Convert.ToByte (s.Substring (idx + 2));

                IndividualControl ic;
                ic.Group = (byte)cardId;
                ic.Individual = channelId;

                Temperature.SetTemperatureProbeIndividualControl (probeIdx, ic);
                
                JObject jo = new JObject ();

                jo.Add (new JProperty ("temperatureProbes", 
                    new JArray (
                        new JObject (
                            new JProperty ("name", Temperature.GetTemperatureProbeName (probeIdx)),
                            new JProperty ("inputCard", AnalogInput.GetCardName (ic.Group)), 
                            new JProperty ("channel", ic.Individual.ToString ())))));

                string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
                path = System.IO.Path.Combine (path, "Settings");
                path = System.IO.Path.Combine (path, "tempProperties.json");

                string text = File.ReadAllText (path);
                int start = text.IndexOf ("\"temperatureProbes");
                int end = text.IndexOf ("\"heaters");

                string globSet = text.Substring (start, end - start);
                end = globSet.LastIndexOf (']');
                globSet = globSet.Substring (0, end + 1);

                string joText = jo.ToString ();
                int jStart = joText.IndexOf ('"');
                int jEnd = joText.LastIndexOf (']');
                joText = joText.Substring (jStart, jEnd - jStart + 1);

                text = text.Replace (globSet, joText);

                File.WriteAllText (path, text);
            }

            return true;
        }
    }
}


