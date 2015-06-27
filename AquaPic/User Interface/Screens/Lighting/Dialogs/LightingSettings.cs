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
            t.label.text = "Default Rise";
            t.textBox.text = Lighting.defaultSunRise.TimeToString ();
            settings.Add (t.label.text, t);

            t = new TouchLabelTextBox ();
            t.label.text = "Default Set";
            t.textBox.text = Lighting.defaultSunSet.TimeToString ();
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
                Lighting.defaultSunRise = ToTime (settings ["Default Sunrise"].textBox.text);
            } catch {
                MessageBox.Show ("Improper time format, ##:##");
                return false;
            }

            try {
                Lighting.defaultSunSet = ToTime (settings ["Default Sunset"].textBox.text);
            } catch {
                MessageBox.Show ("Improper time format, ##:##");
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

            JObject jo = new JObject ();

            jo.Add (new JProperty ("latitude", Lighting.latitude.ToString ()));
            jo.Add (new JProperty ("longitude", Lighting.longitude.ToString ()));

            jo.Add (new JProperty ("defaultSunRise", 
                new JObject (
                    new JProperty ("hour", Lighting.defaultSunRise.hour.ToString ()), 
                    new JProperty ("minute", Lighting.defaultSunRise.min.ToString ()))));

            jo.Add (new JProperty ("defaultSunSet", 
                new JObject (
                    new JProperty ("hour", Lighting.defaultSunSet.hour.ToString ()), 
                    new JProperty ("minute", Lighting.defaultSunSet.min.ToString ()))));

            jo.Add (new JProperty ("minSunRise", 
                new JObject (
                    new JProperty ("hour", Lighting.minSunRise.hour.ToString ()), 
                    new JProperty ("minute", Lighting.minSunRise.min.ToString ()))));

            jo.Add (new JProperty ("maxSunRise", 
                new JObject (
                    new JProperty ("hour", Lighting.maxSunRise.hour.ToString ()), 
                    new JProperty ("minute", Lighting.maxSunRise.min.ToString ()))));

            jo.Add (new JProperty ("minSunSet", 
                new JObject (
                    new JProperty ("hour", Lighting.minSunSet.hour.ToString ()), 
                    new JProperty ("minute", Lighting.minSunSet.min.ToString ()))));

            jo.Add (new JProperty ("maxSunSet", 
                new JObject (
                    new JProperty ("hour", Lighting.maxSunSet.hour.ToString ()), 
                    new JProperty ("minute", Lighting.maxSunSet.min.ToString ()))));

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "lightingProperties.json");

            File.WriteAllText (path, jo.ToString ());

            Lighting.UpdateRiseSetTimes ();

            return true;
        }

        protected Time ToTime (string value) {
            int pos = value.IndexOf (":");

            if ((pos != 2) || (pos != 1))
                throw new Exception ();

            string hourString = value.Substring (0, pos);
            int hour = Convert.ToInt32 (hourString);

            if ((hour < 0) || (hour > 23))
                throw new Exception ();

            string minString = value.Substring (pos + 1, 2);
            int min = Convert.ToInt32 (minString);

            if ((min < 0) || (min > 59))
                throw new Exception ();

            pos = value.Length;
            if (pos > 3) {
                string last = value.Substring (pos - 2);
                if (last == "pm") {
                    if ((hour >= 1) && (hour <= 12))
                        hour = (hour + 12) % 24;
                }
            }

            return new Time ((byte)hour, (byte)min);
        }
    }
}

