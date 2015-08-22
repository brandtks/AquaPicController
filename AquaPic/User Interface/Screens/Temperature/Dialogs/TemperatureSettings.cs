using System;
using System.IO;
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class TemperatureSettings : TouchSettingsDialog
    {
        public TemperatureSettings () : base ("Temperature") {
            SaveEvent += OnSave;

            var t = new SettingTextBox ();
            t.text = "Setpoint";
            t.textBox.text = Temperature.temperatureSetpoint.ToString ();
            settings.Add (t.label.text, t);

            t = new SettingTextBox ();
            t.text = "Deadband";
            t.textBox.text = (Temperature.temperatureDeadband * 2).ToString ();
            settings.Add (t.label.text, t);

            t = new SettingTextBox ();
            t.text = "High Alarm";
            t.textBox.text = Temperature.highTempAlarmSetpoint.ToString ();
            settings.Add (t.label.text, t);

            t = new SettingTextBox ();
            t.text = "Low Alarm";
            t.textBox.text = Temperature.lowTempAlarmSetpoint.ToString ();
            settings.Add (t.label.text, t);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            try {
                Temperature.temperatureSetpoint = Convert.ToSingle (((SettingTextBox)settings ["Setpoint"]).textBox.text);
            } catch {
                TouchMessageBox.Show ("Improper setpoint format");
                return false;
            }

            try {
                Temperature.temperatureDeadband = Convert.ToSingle (((SettingTextBox)settings ["Deadband"]).textBox.text) / 2;
            } catch {
                TouchMessageBox.Show ("Improper deadband format");
                return false;
            }

            try {
                Temperature.highTempAlarmSetpoint = Convert.ToSingle (((SettingTextBox)settings ["High Alarm"]).textBox.text);
            } catch {
                TouchMessageBox.Show ("Improper high alarm setpoint format");
                return false;
            }

            try {
                Temperature.lowTempAlarmSetpoint = Convert.ToSingle (((SettingTextBox)settings ["Low Alarm"]).textBox.text);
            } catch {
                TouchMessageBox.Show ("Improper low alarm setpoint format");
                return false;
            }

            JObject jo = new JObject ();

            jo.Add (new JProperty ("highTempAlarmSetpoint", Temperature.highTempAlarmSetpoint.ToString ()));
            jo.Add (new JProperty ("lowTempAlarmSetpoint", Temperature.lowTempAlarmSetpoint.ToString ()));
            jo.Add (new JProperty ("tempSetpoint", Temperature.temperatureSetpoint.ToString ()));
            jo.Add (new JProperty ("deadband", (Temperature.temperatureDeadband * 2).ToString ()));

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string text = File.ReadAllText (path);
            int start = text.IndexOf ('"');
            int end = text.IndexOf ("\"temperatureProbes");

            string globSet = text.Substring (start, end - start);

            end = globSet.LastIndexOf ('"');
            globSet = globSet.Substring (0, end + 1);

            string joText = jo.ToString ();
            int start2 = joText.IndexOf ('"');
            int end2 = joText.LastIndexOf ('"');
            joText = joText.Substring (start2, end2 - start2 + 1);

            text = text.Replace (globSet, joText);

            File.WriteAllText (path, text);
            
            return true;
        }
    }
}

