using System;
using System.IO;
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Utilites;

namespace AquaPic
{
    public class TemperatureSettings : TouchSettingsDialog
    {
        public TemperatureSettings () : base ("Temperature") {
            SaveEvent += OnSave;

            var t = new TouchLabelTextBox ();
            t.label.text = "Setpoint";
            t.textBox.text = Temperature.temperatureSetpoint.ToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "Deadband";
            t.textBox.text = (Temperature.temperatureDeadband * 2).ToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "High Alarm";
            t.textBox.text = Temperature.highTempAlarmSetpoint.ToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "Low Alarm";
            t.textBox.text = Temperature.lowTempAlarmSetpoint.ToString ();
            settings.Add (t.label.text, t);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            try {
                Temperature.temperatureSetpoint = Convert.ToSingle (settings ["Setpoint"].textBox.text);
            } catch {
                MessageBox.Show ("Improper setpoint format");
                return false;
            }

            try {
                Temperature.temperatureDeadband = Convert.ToSingle (settings ["Deadband"].textBox.text) / 2;
            } catch {
                MessageBox.Show ("Improper deadband format");
                return false;
            }

            try {
                Temperature.highTempAlarmSetpoint = Convert.ToSingle (settings ["High Alarm"].textBox.text);
            } catch {
                MessageBox.Show ("Improper high alarm setpoint format");
                return false;
            }

            try {
                Temperature.lowTempAlarmSetpoint = Convert.ToSingle (settings ["Low Alarm"].textBox.text);
            } catch {
                MessageBox.Show ("Improper low alarm setpoint format");
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

            File.WriteAllText (path, jo.ToString ());

            return true;
        }
    }
}

