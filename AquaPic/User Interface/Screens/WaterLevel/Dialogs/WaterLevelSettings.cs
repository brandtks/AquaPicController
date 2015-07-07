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

            var s = new SettingSelectorSwitch ();
            s.text = "Enable";
            s.selectorSwitch.SelectorChangedEvent += (sender, args) => {
                if (args.currentSelectedIndex == 0)
                    showOptional = true;
                else
                    showOptional = false;

                UpdateSettingsVisibility ();
            };
            AddSetting (s);

            var t = new SettingTextBox ();
            t.text = "High Alarm";
            t.textBox.text = WaterLevel.highAnalogLevelAlarmSetpoint.ToString ();
            AddOptionalSetting (t);

            t = new SettingTextBox ();
            t.text = "Low Alarm";
            t.textBox.text = WaterLevel.lowAnalogLevelAlarmSetpoint.ToString ();
            AddOptionalSetting (t);

            var c = new SettingComboBox ();
            c.text = "Sensor Channel";
            string[] availCh = AnalogInput.GetAllAvaiableChannels ();
            IndividualControl ic = WaterLevel.analogSensorChannel;
            string chName = AnalogInput.GetCardName (ic.Group);
            chName = string.Format ("{0}.i{1}", chName, ic.Individual);
            c.combo.List.Add (string.Format ("Current: {0}", chName));
            c.combo.List.AddRange (availCh);
            c.combo.Active = 0;
            AddOptionalSetting (c);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            try {
                SettingSelectorSwitch s = settings ["Enable"] as SettingSelectorSwitch;
                if (s.selectorSwitch.CurrentSelected == 0)
                    WaterLevel.SetAnalogSensorEnable (true);
                else
                    WaterLevel.SetAnalogSensorEnable (false);
            } catch {
                return false;
            }

            try {
                WaterLevel.highAnalogLevelAlarmSetpoint = Convert.ToSingle (((SettingTextBox)settings ["High Alarm"]).textBox.text);
            } catch {
                MessageBox.Show ("Improper high alarm setpoint format");
                return false;
            }

            try {
                WaterLevel.lowAnalogLevelAlarmSetpoint = Convert.ToSingle (((SettingTextBox)settings ["Low Alarm"]).textBox.text);
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

                    IndividualControl ic;
                    ic.Group = (byte)cardId;
                    ic.Individual = (byte)channelId;
                    WaterLevel.SetAnalogSensorIndividualControl (ic);
                }
            } catch {
                return false;
            }

            JObject jo = new JObject ();

            jo.Add (new JProperty ("enableAnalogSensor", WaterLevel.analogSensorEnabled.ToString ()));
            jo.Add (new JProperty ("highAnalogLevelAlarmSetpoint", WaterLevel.highAnalogLevelAlarmSetpoint.ToString ()));
            jo.Add (new JProperty ("lowAnalogLevelAlarmSetpoint", WaterLevel.lowAnalogLevelAlarmSetpoint.ToString ()));
            jo.Add (new JProperty ("analogSensorChannel", 
                new JObject (
                    new JProperty ("Group", AnalogInput.GetCardName (WaterLevel.analogSensorChannel.Group)), 
                    new JProperty ("Individual", WaterLevel.analogSensorChannel.Individual.ToString ()))));

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            File.WriteAllText (path, jo.ToString ());

            return true;
        }
    }
}

