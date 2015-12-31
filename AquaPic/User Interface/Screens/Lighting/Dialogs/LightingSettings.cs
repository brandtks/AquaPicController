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
    public class LightingSettings : TouchSettingsDialog
    {
        public LightingSettings () : base ("Lighting") {
            SaveEvent += OnSave;

            var t = new SettingTextBox ();
            t.text = "Latitude";
            t.textBox.text = Lighting.latitude.ToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToDouble (args.text);
                } catch {
                    MessageBox.Show ("Improper latitude format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Longitude";
            t.textBox.text = Lighting.longitude.ToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToDouble (args.text);
                } catch {
                    MessageBox.Show ("Improper Longitude format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Default Rise";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.defaultSunRise.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Default Set";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.defaultSunSet.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Min Sunrise";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.minSunRise.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Max Sunrise";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.maxSunRise.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Min Sunset";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.minSunSet.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Max Sunset";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.maxSunSet.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            Lighting.latitude = Convert.ToDouble (((SettingTextBox)settings ["Latitude"]).textBox.text);
            Lighting.longitude = Convert.ToDouble (((SettingTextBox)settings ["Longitude"]).textBox.text);
            Lighting.defaultSunRise = Time.Parse (((SettingTextBox)settings ["Default Sunrise"]).textBox.text);
            Lighting.defaultSunSet = Time.Parse (((SettingTextBox)settings ["Default Sunset"]).textBox.text);
            Lighting.minSunRise = Time.Parse (((SettingTextBox)settings ["Min Sunrise"]).textBox.text);
            Lighting.maxSunRise = Time.Parse (((SettingTextBox)settings ["Max Sunrise"]).textBox.text);
            Lighting.minSunSet = Time.Parse (((SettingTextBox)settings ["Min Sunset"]).textBox.text);
            Lighting.maxSunSet = Time.Parse (((SettingTextBox)settings ["Max Sunset"]).textBox.text);

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
    }
}

