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
    public class LevelSettings : TouchSettingsDialog
    {
        public LevelSettings () : base ("Water Level") {
            SaveEvent += OnSave;

            var t = new TouchLabelTextBox ();
            t.label.text = "High Alarm";
            t.textBox.text = WaterLevel.highLevelAlarmSetpoint.ToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "Low Alarm";
            t.textBox.text = WaterLevel.lowLevelAlarmSetpoint.ToString ();
            settings.Add (t.label.text, t);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            try {
                WaterLevel.highLevelAlarmSetpoint = Convert.ToSingle (settings ["High Alarm"].textBox.text);
            } catch {
                MessageBox.Show ("Improper high alarm setpoint format");
                return false;
            }

            try {
                WaterLevel.lowLevelAlarmSetpoint = Convert.ToSingle (settings ["Low Alarm"].textBox.text);
            } catch {
                MessageBox.Show ("Improper low alarm setpoint format");
                return false;
            }

            JObject jo = new JObject ();

            jo.Add (new JProperty ("highLevelAlarmSetpoint", WaterLevel.highLevelAlarmSetpoint.ToString ()));
            jo.Add (new JProperty ("lowLevelAlarmSetpoint", WaterLevel.lowLevelAlarmSetpoint.ToString ()));

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            File.WriteAllText (path, jo.ToString ());

            return true;
        }
    }
}

