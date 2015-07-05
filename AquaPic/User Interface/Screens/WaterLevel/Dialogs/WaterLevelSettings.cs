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
    public class LevelSettings : TouchSettingsDialog
    {
        public LevelSettings () : base ("Water Level") {
            SaveEvent += OnSave;

            var t = new SettingTextBox ();
            t.text = "High Alarm";
            t.textBox.text = WaterLevel.highLevelAlarmSetpoint.ToString ();
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Low Alarm";
            t.textBox.text = WaterLevel.lowLevelAlarmSetpoint.ToString ();
            AddSetting (t);

            var c = new SettingComboBox ();
            c.text = "Level Input";
            string[] availCh = AnalogInput.GetAllAvaiableChannels ();
            IndividualControl ic = WaterLevel.levelSensor;
            string chName = AnalogInput.GetCardName (ic.Group);
            chName = string.Format ("{0}.i{1}", chName, ic.Individual);
            c.combo.List.Add (string.Format ("Current: {0}", chName));
            c.combo.List.AddRange (availCh);
            c.combo.Active = 0;
            AddSetting (c);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            try {
                WaterLevel.highLevelAlarmSetpoint = Convert.ToSingle (((SettingTextBox)settings ["High Alarm"]).textBox.text);
            } catch {
                MessageBox.Show ("Improper high alarm setpoint format");
                return false;
            }

            try {
                WaterLevel.lowLevelAlarmSetpoint = Convert.ToSingle (((SettingTextBox)settings ["Low Alarm"]).textBox.text);
            } catch {
                MessageBox.Show ("Improper low alarm setpoint format");
                return false;
            }

            try {
                string s = ((SettingComboBox)settings ["Level Input"]).combo.activeText;
                if (!s.StartsWith ("Current:")) {
                    int idx = s.IndexOf ('.');
                    string cardName = s.Substring (0, idx);
                    int cardId = AnalogInput.GetCardIndex (cardName);
                    int channelId = Convert.ToByte (s.Substring (idx + 2));
                   
                    AnalogInput.RemoveChannel (WaterLevel.levelSensor);

                    WaterLevel.levelSensor.Group = (byte)cardId;
                    WaterLevel.levelSensor.Individual = (byte)channelId;

                    AnalogInput.AddChannel (WaterLevel.levelSensor, AnalogType.Level, "Water Level");
                }
            } catch {
                return false;
            }

            JObject jo = new JObject ();

            jo.Add (new JProperty ("highLevelAlarmSetpoint", WaterLevel.highLevelAlarmSetpoint.ToString ()));
            jo.Add (new JProperty ("lowLevelAlarmSetpoint", WaterLevel.lowLevelAlarmSetpoint.ToString ()));
            jo.Add (new JProperty ("sensor", 
                new JObject (
                    new JProperty ("Group", AnalogInput.GetCardName (WaterLevel.levelSensor.Group)), 
                    new JProperty ("Individual", WaterLevel.levelSensor.Individual.ToString ()))));

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            File.WriteAllText (path, jo.ToString ());

            return true;
        }
    }
}

