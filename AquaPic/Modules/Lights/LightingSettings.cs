using System;
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Utilites;

namespace AquaPic
{
    public class LightingSettings : TouchSettingsDialog
    {
        public LightingSettings () : base ("Lighting") {
            SaveEvent += OnSave;

            var t = new TouchLabelTextBox ();
            t.label.text = "Latitude";
            t.textBox.text = Lighting.latitude.ToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "Longitude";
            t.textBox.text = Lighting.longitude.ToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "Time Zone";
            t.textBox.text = Lighting.timeZone.ToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "Min Sunrise";
            t.textBox.text = Lighting.minSunRise.TimeToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "Max Sunrise";
            t.textBox.text = Lighting.maxSunRise.TimeToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "Min Sunset";
            t.textBox.text = Lighting.minSunSet.TimeToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "Max Sunset";
            t.textBox.text = Lighting.maxSunSet.TimeToString ();
            settings.Add (t.label.text, t);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            bool success;

            try {
                Lighting.latitude = Convert.ToDouble (settings ["Latitude"].textBox.text);
            } catch {
                MessageBox.Show ("Improper latitude format");
                return false;
            }

            try {
                Lighting.longitude = Convert.ToDouble (settings ["Longitude"].textBox.text);
            } catch {
                MessageBox.Show ("Improper longitude format");
                return false;
            }

            try {
                Lighting.timeZone = Convert.ToInt32 (settings ["Time Zone"].textBox.text);
            } catch {
                MessageBox.Show ("Improper time zone format");
                return false;
            }

            try {
                Lighting.minSunRise = ToTime (settings ["Min Sunrise"].textBox.text);
            } catch {
                MessageBox.Show ("Improper time format, ##:##");
                return false;
            }

            try {
                Lighting.maxSunRise = ToTime (settings ["Max Sunrise"].textBox.text);
            } catch {
                MessageBox.Show ("Improper time format, ##:##");
                return false;
            }

            try {
                Lighting.minSunSet = ToTime (settings ["Min Sunset"].textBox.text);
            } catch {
                MessageBox.Show ("Improper time format, ##:##");
                return false;
            }

            try {
                Lighting.maxSunSet = ToTime (settings ["Max Sunset"].textBox.text);
            } catch {
                MessageBox.Show ("Improper time format, ##:##");
                return false;
            }

            return true;
        }

        protected Time ToTime (string value) {
            int pos = value.IndexOf (":");
            if (pos != 2)
                throw new Exception ();

            string hourString = value.Substring (0, 2);
            byte hour = Convert.ToByte (hourString);

            string minString = value.Substring (3, 2);
            byte min = Convert.ToByte (minString);

            return new Time (hour, min);
        }
    }
}

