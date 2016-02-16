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
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class AnalogSensorSettings : TouchSettingsDialog
    {
        public AnalogSensorSettings () : base ("Analog Water Sensor") {
            SaveEvent += OnSave;

            var s = new SettingSelectorSwitch ();
            s.text = "Enable";
            if (WaterLevel.analogSensorEnabled) {
                s.selectorSwitch.CurrentSelected = 0;
                showOptional = true;
            } else {
                s.selectorSwitch.CurrentSelected = 1;
                showOptional = false;
            }
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
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float highAlarmStpnt = Convert.ToSingle (args.text);

                    if (highAlarmStpnt < 0.0f) {
                        MessageBox.Show ("High alarm setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }

                    float lowAlarmStpnt = Convert.ToSingle (((SettingTextBox)settings ["Low Alarm"]).textBox.text);

                    if (lowAlarmStpnt >= highAlarmStpnt) {
                        MessageBox.Show ("Low alarm setpoint can't be greater than or equal to high setpoint");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper high alarm setpoint format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

            t = new SettingTextBox ();
            t.text = "Low Alarm";
            t.textBox.text = WaterLevel.lowAnalogLevelAlarmSetpoint.ToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float lowAlarmStpnt = Convert.ToSingle (args.text);

                    if (lowAlarmStpnt < 0.0f) {
                        MessageBox.Show ("Low alarm setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }
                        
                    float highAlarmStpnt = Convert.ToSingle (((SettingTextBox)settings ["High Alarm"]).textBox.text);

                    if (lowAlarmStpnt >= highAlarmStpnt) {
                        MessageBox.Show ("Low alarm setpoint can't be greater than or equal to high setpoint");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper low alarm setpoint format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

            var c = new SettingComboBox ();
            c.text = "Sensor Channel";
            string[] availCh = AquaPicDrivers.AnalogInput.GetAllAvaiableChannels ();
            if ((WaterLevel.analogSensorEnabled) || (WaterLevel.analogSensorChannel.IsNotEmpty ())) {
                IndividualControl ic = WaterLevel.analogSensorChannel;
                string chName = AquaPicDrivers.AnalogInput.GetCardName (ic.Group);
                chName = string.Format ("{0}.i{1}", chName, ic.Individual);
                c.combo.List.Add (string.Format ("Current: {0}", chName));
                c.combo.Active = 0;
            } else
                c.combo.NonActiveMessage = "Select outlet";
            c.combo.List.AddRange (availCh); 
            AddOptionalSetting (c);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            bool enable;
            try {
                SettingSelectorSwitch s = settings ["Enable"] as SettingSelectorSwitch;
                if (s.selectorSwitch.CurrentSelected == 0)
                    enable = true;
                else
                    enable = false;
            } catch {
                return false;
            }
                
            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            string jstring = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (jstring);

            if (enable) {
                float lowAlarmStpnt = Convert.ToSingle (Convert.ToSingle (((SettingTextBox)settings ["Low Alarm"]).textBox.text));

                if (lowAlarmStpnt < 0.0f) {
                    MessageBox.Show ("Low alarm setpoint can't be negative");
                    return false;
                }

                float highAlarmStpnt = Convert.ToSingle (((SettingTextBox)settings ["High Alarm"]).textBox.text);

                if (highAlarmStpnt < 0.0f) {
                    MessageBox.Show ("Low alarm setpoint can't be negative");
                    return false;
                }

                if (lowAlarmStpnt >= highAlarmStpnt) {
                    MessageBox.Show ("Low alarm setpoint can't be greater than or equal to high setpoint");
                    return false;
                }

                WaterLevel.highAnalogLevelAlarmSetpoint = highAlarmStpnt;
                WaterLevel.lowAnalogLevelAlarmSetpoint = lowAlarmStpnt;

                try {
                    if (((SettingComboBox)settings ["Sensor Channel"]).combo.Active == -1) {
                        MessageBox.Show ("Please Select an input channel");
                        return false;
                    }

                    string s = ((SettingComboBox)settings ["Sensor Channel"]).combo.activeText;

                    if (!s.StartsWith ("Current:")) {
                        int idx = s.IndexOf ('.');
                        string cardName = s.Substring (0, idx);
                        int cardId = AquaPicDrivers.AnalogInput.GetCardIndex (cardName);
                        int channelId = Convert.ToInt32 (s.Substring (idx + 2));

                        IndividualControl ic;
                        ic.Group = cardId;
                        ic.Individual = channelId;
                        WaterLevel.analogSensorChannel = ic;
                    }
                } catch (Exception ex) {
                    Logger.AddError (ex.ToString ());
                    MessageBox.Show ("Something went wrong, check logger");
                    return false;
                }

            }

            //this has to be last because if for whatever reason something above this crashes we need leave the module disable
            WaterLevel.analogSensorEnabled = enable;

            jo ["enableAnalogSensor"] = WaterLevel.analogSensorEnabled.ToString ();
            jo ["highAnalogLevelAlarmSetpoint"] = WaterLevel.highAnalogLevelAlarmSetpoint.ToString ();
            jo ["lowAnalogLevelAlarmSetpoint"] = WaterLevel.lowAnalogLevelAlarmSetpoint.ToString ();
            if (WaterLevel.analogSensorChannel.IsNotEmpty ()) {
                jo ["inputCard"] = AquaPicDrivers.AnalogInput.GetCardName (WaterLevel.analogSensorChannel.Group);
                jo ["channel"] = WaterLevel.analogSensorChannel.Individual.ToString ();
            } else {
                jo ["inputCard"] = string.Empty;
                jo ["channel"] = string.Empty;
            }

            File.WriteAllText (path, jo.ToString ());

            return true;
        }
    }
}