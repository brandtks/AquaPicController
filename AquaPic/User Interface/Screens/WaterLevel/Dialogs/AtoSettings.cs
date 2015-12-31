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
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class AtoSettings : TouchSettingsDialog
    {
        public AtoSettings () : base ("Auto Top Off") {
            SaveEvent += OnSave;

            var s = new SettingSelectorSwitch ();
            s.text = "Enable";
            if (WaterLevel.atoEnabled) {
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

            s = new SettingSelectorSwitch ();
            s.text = "Use Analog";
            if (WaterLevel.atoUseAnalogSensor)
                s.selectorSwitch.CurrentSelected = 0;
            else
                s.selectorSwitch.CurrentSelected = 1;
            AddOptionalSetting (s);

            var t = new SettingTextBox ();
            t.text = "Analog On";
            t.textBox.text = WaterLevel.atoAnalogOnSetpoint.ToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float onStpnt = Convert.ToSingle (args.text);

                    if (onStpnt < 0.0f) {
                        MessageBox.Show ("Analog on setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }
                        
                    float offStpnt = Convert.ToSingle (((SettingTextBox)settings ["Analog Off"]).textBox.text);

                    if (onStpnt >= offStpnt) {
                        MessageBox.Show ("Analog on setpoint can't be greater than or equal to off setpoint");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper analog on setpoint format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

            t = new SettingTextBox ();
            t.text = "Analog Off";
            t.textBox.text = WaterLevel.atoAnalogOffSetpoint.ToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float offStpnt = Convert.ToSingle (args.text);

                    if (offStpnt < 0.0f) {
                        MessageBox.Show ("Analog on setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }

                    float onStpnt = Convert.ToSingle (((SettingTextBox)settings ["Analog On"]).textBox.text);

                    if (onStpnt >= offStpnt) {
                        MessageBox.Show ("Analog on setpoint can't be greater than or equal to off setpoint");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper analog off setpoint format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

            s = new SettingSelectorSwitch ();
            s.text = "Use Float Switch";
            if (WaterLevel.atoUseFloatSwitch)
                s.selectorSwitch.CurrentSelected = 0;
            else
                s.selectorSwitch.CurrentSelected = 1;
            AddOptionalSetting (s);

            var c = new SettingComboBox ();
            c.text = "Pump Outlet";
            string[] availOutlets = Power.GetAllAvaiblableOutlets ();
            if (WaterLevel.atoPumpOutlet.IsNotEmpty ()) {
                IndividualControl ic = WaterLevel.atoPumpOutlet;
                string pwrName = Power.GetPowerStripName (ic.Group);
                pwrName = string.Format ("{0}.p{1}", pwrName, ic.Individual);
                c.combo.List.Add (string.Format ("Current: {0}", pwrName));
                c.combo.Active = 0;
            } else
                c.combo.NonActiveMessage = "Select outlet";
            c.combo.List.AddRange (availOutlets); 
            AddOptionalSetting (c);

            t = new SettingTextBox ();
            t.text = "Max Runtime";
            t.textBox.text = string.Format ("{0} mins", WaterLevel.atoMaxRuntime / 60000);
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                
                try {
                    int idx = args.text.IndexOf ("mins", StringComparison.InvariantCultureIgnoreCase);
                    uint time;
                    if (idx == -1)
                        time = Convert.ToUInt32 (args.text);
                    else {
                        string timeString = args.text.Substring (0, idx);
                        time = Convert.ToUInt32 (timeString);
                    }

                    args.text = string.Format ("{0} mins", time);
                } catch {
                    MessageBox.Show ("Improper format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

            t = new SettingTextBox ();
            t.text = "Cooldown";
            t.textBox.text = string.Format ("{0} mins", WaterLevel.atoCooldown / 60000);
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;

                try {
                    int idx = args.text.IndexOf ("mins", StringComparison.InvariantCultureIgnoreCase);
                    uint time;
                    if (idx == -1)
                        time = Convert.ToUInt32 (args.text);
                    else {
                        string timeString = args.text.Substring (0, idx);
                        time = Convert.ToUInt32 (timeString);
                    }

                    args.text = string.Format ("{0} mins", time);
                } catch {
                    MessageBox.Show ("Improper format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

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
                bool useAnalog;
                SettingSelectorSwitch s = settings ["Use Analog"] as SettingSelectorSwitch;
                if (s.selectorSwitch.CurrentSelected == 0)
                    useAnalog = true;
                else
                    useAnalog = false;

                WaterLevel.atoUseAnalogSensor = useAnalog;

                float analogOnStpnt = Convert.ToSingle (Convert.ToSingle (((SettingTextBox)settings ["Analog On"]).textBox.text));

                if (analogOnStpnt < 0.0f) {
                    MessageBox.Show ("Analog on setpoint can't be negative");
                    return false;
                }

                float analogOffStpnt = Convert.ToSingle (((SettingTextBox)settings ["Analog Off"]).textBox.text);

                if (analogOffStpnt < 0.0f) {
                    MessageBox.Show ("Analog off setpoint can't be negative");
                    return false;
                }

                if (analogOnStpnt >= analogOffStpnt) {
                    MessageBox.Show ("Analog on setpoint can't be greater than or equal to analog off setpoint");
                    return false;
                }

                WaterLevel.atoAnalogOnSetpoint = analogOnStpnt;
                WaterLevel.atoAnalogOffSetpoint = analogOffStpnt;

                bool useFloatSwitch;
                s = settings ["Use Float Switch"] as SettingSelectorSwitch;
                if (s.selectorSwitch.CurrentSelected == 0)
                    useFloatSwitch = true;
                else
                    useFloatSwitch = false;

                WaterLevel.atoUseAnalogSensor = useFloatSwitch;

                try {
                    if (((SettingComboBox)settings ["Pump Outlet"]).combo.Active == -1) {
                        MessageBox.Show ("Please select power outlet for the pump");
                        return false;
                    }

                    string outletString = ((SettingComboBox)settings ["Pump Outlet"]).combo.activeText;

                    if (!outletString.StartsWith ("Current:")) {
                        int idx = outletString.IndexOf ('.');
                        string pwrName = outletString.Substring (0, idx);
                        int pwrId = Power.GetPowerStripIndex (pwrName);
                        int outletId = Convert.ToInt32 (outletString.Substring (idx + 2));

                        IndividualControl ic;
                        ic.Group = pwrId;
                        ic.Individual = outletId;
                        WaterLevel.atoPumpOutlet = ic;
                    }
                } catch (Exception ex) {
                    Logger.AddError (ex.ToString ());
                    MessageBox.Show ("Something went wrong, check logger");
                    return false;
                }

                uint maxRuntime = 0;
                string timeString = ((SettingTextBox)settings ["Max Runtime"]).textBox.text;
                int periodIdx = timeString.IndexOf ("mins", StringComparison.InvariantCultureIgnoreCase);
                if (periodIdx != -1)
                    timeString = timeString.Substring (0, periodIdx);
                maxRuntime = Convert.ToUInt32 (timeString) * 60000;

                WaterLevel.atoMaxRuntime = maxRuntime;

                uint cooldown = 0;
                timeString = ((SettingTextBox)settings ["Cooldown"]).textBox.text;
                periodIdx = timeString.IndexOf ("mins", StringComparison.InvariantCultureIgnoreCase);
                if (periodIdx != -1)
                    timeString = timeString.Substring (0, periodIdx);
                cooldown = Convert.ToUInt32 (timeString) * 60000;
                WaterLevel.atoCooldown = cooldown;
            }

            //this has to be last because if for whatever reason something above this crashes we need leave the module disable
            WaterLevel.atoEnabled = enable;

            jo ["AutoTopOff"] ["enableAto"] = WaterLevel.atoEnabled.ToString ();
            jo ["AutoTopOff"] ["useAnalogSensor"] = WaterLevel.atoUseAnalogSensor.ToString ();
            jo ["AutoTopOff"] ["analogOnSetpoint"]= WaterLevel.atoAnalogOnSetpoint.ToString ();
            jo ["AutoTopOff"] ["analogOffSetpoint"]= WaterLevel.atoAnalogOffSetpoint.ToString ();
            jo ["AutoTopOff"] ["useFloatSwitch"]= WaterLevel.atoUseFloatSwitch.ToString ();
            if (WaterLevel.atoPumpOutlet.IsNotEmpty ()) {
                jo ["AutoTopOff"] ["powerStrip"] = Power.GetPowerStripName (WaterLevel.atoPumpOutlet.Group);
                jo ["AutoTopOff"] ["outlet"] = WaterLevel.atoPumpOutlet.Individual.ToString ();
            } else {
                jo ["AutoTopOff"] ["powerStrip"] = string.Empty;
                jo ["AutoTopOff"] ["outlet"] = string.Empty;
            }
            jo ["AutoTopOff"] ["maxPumpOnTime"] = string.Format ("{0:D2}:00:00", WaterLevel.atoMaxRuntime / 60000);
            jo ["AutoTopOff"] ["minPumpOffTime"] = string.Format ("{0:D2}:00:00", WaterLevel.atoCooldown / 60000);

            File.WriteAllText (path, jo.ToString ());

            return true;
        }
    }
}