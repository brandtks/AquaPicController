#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    public class LightingSettings : TouchSettingsDialog
    {
        public LightingSettings () : base ("Lighting") {
            SaveEvent += OnSave;

            var t = new SettingsTextBox ();
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

            t = new SettingsTextBox ();
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

            t = new SettingsTextBox ();
            t.text = "Default Rise";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.defaultSunRise.ToShortTimeString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.ToShortTimeString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ();
            t.text = "Default Set";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.defaultSunSet.ToShortTimeString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.ToShortTimeString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ();
            t.text = "Min Sunrise";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.minSunRise.ToShortTimeString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.ToShortTimeString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ();
            t.text = "Max Sunrise";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.maxSunRise.ToShortTimeString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.ToShortTimeString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ();
            t.text = "Min Sunset";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.minSunSet.ToShortTimeString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.ToShortTimeString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ();
            t.text = "Max Sunset";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.maxSunSet.ToShortTimeString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = Time.Parse (args.text);
                    args.text = time.ToShortTimeString ();
                } catch {
                    MessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            Lighting.latitude = Convert.ToDouble ((settings["Latitude"] as SettingsTextBox).textBox.text);
            Lighting.longitude = Convert.ToDouble ((settings["Longitude"] as SettingsTextBox).textBox.text);
            Lighting.defaultSunRise = Time.Parse ((settings["Default Rise"] as SettingsTextBox).textBox.text);
            Lighting.defaultSunSet = Time.Parse ((settings["Default Set"] as SettingsTextBox).textBox.text);
            Lighting.minSunRise = Time.Parse ((settings ["Min Sunrise"] as SettingsTextBox).textBox.text);
            Lighting.maxSunRise = Time.Parse ((settings["Max Sunrise"] as SettingsTextBox).textBox.text);
            Lighting.minSunSet = Time.Parse ((settings["Min Sunset"] as SettingsTextBox).textBox.text);
            Lighting.maxSunSet = Time.Parse ((settings["Max Sunset"] as SettingsTextBox).textBox.text);

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "lightingProperties.json");

            string json = File.ReadAllText (path);
            var jo = (JObject)JToken.Parse (json);

            jo["latitude"] = Lighting.latitude.ToString ();
            jo["longitude"] = Lighting.longitude.ToString ();

            jo["defaultSunRise"]["hour"] = Lighting.defaultSunRise.hour.ToString ();
            jo["defaultSunRise"]["minute"] = Lighting.defaultSunRise.minute.ToString ();

            jo["defaultSunSet"]["hour"] = Lighting.defaultSunSet.hour.ToString ();
            jo["defaultSunSet"]["minute"] = Lighting.defaultSunSet.minute.ToString ();

            jo["minSunRise"]["hour"] = Lighting.minSunRise.hour.ToString ();
            jo["minSunRise"]["minute"] = Lighting.minSunRise.minute.ToString ();

            jo["maxSunRise"]["hour"] = Lighting.maxSunRise.hour.ToString ();
            jo["maxSunRise"]["minute"] = Lighting.maxSunRise.minute.ToString ();

            jo["minSunSet"]["hour"] = Lighting.minSunSet.hour.ToString ();
            jo["minSunSet"]["minute"] = Lighting.minSunSet.minute.ToString ();

            jo["maxSunSet"]["hour"] = Lighting.maxSunSet.hour.ToString ();
            jo["maxSunSet"]["minute"] = Lighting.maxSunSet.minute.ToString ();

            File.WriteAllText (path, jo.ToString ());

            Lighting.UpdateRiseSetTimes ();

            return true;
        }
    }
}

