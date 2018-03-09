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
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class LightingSettings : TouchSettingsDialog
    {
        public LightingSettings () : base ("Lighting") {
            var t = new SettingsTextBox ("Latitude");
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

            t = new SettingsTextBox ("Longitude");
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

            t = new SettingsTextBox ("Default Rise");
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

            t = new SettingsTextBox ("Default Set");
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

            t = new SettingsTextBox ("Min Sunrise");
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

            t = new SettingsTextBox ("Max Sunrise");
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

            t = new SettingsTextBox ("Min Sunset");
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

            t = new SettingsTextBox ("Max Sunset");
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

        protected override bool OnSave (object sender) {
            Lighting.latitude = Convert.ToDouble (settings["Latitude"].setting);
            Lighting.longitude = Convert.ToDouble (settings["Longitude"].setting);
            Lighting.defaultSunRise = Time.Parse ((string)settings["Default Rise"].setting);
            Lighting.defaultSunSet = Time.Parse ((string)settings["Default Set"].setting);
            Lighting.minSunRise = Time.Parse ((string)settings ["Min Sunrise"].setting);
            Lighting.maxSunRise = Time.Parse ((string)settings["Max Sunrise"].setting);
            Lighting.minSunSet = Time.Parse ((string)settings["Min Sunset"].setting);
            Lighting.maxSunSet = Time.Parse ((string)settings["Max Sunset"].setting);

            var jo = SettingsHelper.OpenSettingsFile ("lightingProperties");

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

            SettingsHelper.SaveSettingsFile ("lightingProperties", jo);
            Lighting.UpdateRiseSetTimes ();
            return true;
        }
    }
}

