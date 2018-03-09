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
using AquaPic.Globals;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class FixtureSettings : TouchSettingsDialog
    {
        string fixtureName;
        public string newOrUpdatedFixtureName {
            get {
                return fixtureName;
            }
        }

        public FixtureSettings (string fixtureName, bool includeDelete)
            : base (fixtureName, includeDelete) 
        {
            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;

            this.fixtureName = fixtureName;

            var t = new SettingsTextBox ("Name");
            if (this.fixtureName.IsNotEmpty ()) {
                t.textBox.text = this.fixtureName;
            } else {
                t.textBox.text = "Enter name";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!Lighting.FixtureNameOk (args.text)) {
                    MessageBox.Show ("Heater name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Outlet");
            if (this.fixtureName.IsNotEmpty ()) {
                IndividualControl ic = Lighting.GetFixtureOutletIndividualControl (this.fixtureName);
                string psName = Power.GetPowerStripName (ic.Group);
                c.combo.comboList.Add (string.Format ("Current: {0}.p{1}", psName, ic.Individual));
                c.combo.activeIndex = 0;
            } else {
                c.combo.nonActiveMessage = "Select outlet";
            }
            c.combo.comboList.AddRange (Power.GetAllAvaiblableOutlets ());
            AddSetting (c);

            var s = new SettingsSelectorSwitch ("Lighting Time", "Day", "Night");
            if (this.fixtureName.IsNotEmpty ()) {
                if (Lighting.GetFixtureLightingTime (this.fixtureName) == LightingTime.Daytime)
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 0;
            AddSetting (s);

            s = new SettingsSelectorSwitch ("Temp Lockout");
            if (this.fixtureName.IsNotEmpty ()) {
                if (Lighting.GetFixtureTemperatureLockout (this.fixtureName))
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 0;
            AddSetting (s);

            s = new SettingsSelectorSwitch ("Auto Time Update");
            if (this.fixtureName.IsNotEmpty ()) {
                if (Lighting.GetFixtureMode (this.fixtureName) == Mode.Auto)
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 1;
            AddSetting (s);

            t = new SettingsTextBox ("On Time Offset");
            if (this.fixtureName.IsNotEmpty ())
                t.textBox.text = Lighting.GetFixtureOnTimeOffset (this.fixtureName).ToString ();
            else
                t.textBox.text = "0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToInt32 (args.text);
                } catch {
                    MessageBox.Show ("Improper integer format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingsTextBox ("Off Time Offset");
            t.textBox.includeTimeFunctions = true;
            if (this.fixtureName.IsNotEmpty ())
                t.textBox.text = Lighting.GetFixtureOffTimeOffset (this.fixtureName).ToString ();
            else
                t.textBox.text = "0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToInt32 (args.text);
                } catch {
                    MessageBox.Show ("Improper integer format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            bool isDimming;
            if (this.fixtureName.IsNotEmpty ())
                isDimming = Lighting.IsDimmingFixture (this.fixtureName);
            else
                isDimming = false;

            s = new SettingsSelectorSwitch ("Dimming Fixture", "Yes", "No");
            if (isDimming) {
                s.selectorSwitch.currentSelected = 0;
                showOptional = true;
            } else {
                s.selectorSwitch.currentSelected = 1;
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

            c = new SettingsComboBox ("Dimming Channel");
            if ((this.fixtureName.IsNotEmpty ()) && (isDimming)) {
                IndividualControl ic = Lighting.GetDimmingChannelIndividualControl (this.fixtureName);
                string cardName = AquaPicDrivers.AnalogOutput.GetCardName (ic.Group);
                c.combo.comboList.Add (string.Format ("Current: {0}.q{1}", cardName, ic.Individual));
                c.combo.activeIndex = 0;
            } else {
                c.combo.nonActiveMessage = "Select outlet";
            }
            c.combo.comboList.AddRange (AquaPicDrivers.AnalogOutput.GetAllAvaiableChannels ());
            AddOptionalSetting (c);

            t = new SettingsTextBox ("Max Dimming");
            if ((this.fixtureName.IsNotEmpty ()) && (isDimming))
                t.textBox.text = Lighting.GetMaxDimmingLevel (this.fixtureName).ToString ();
            else
                t.textBox.text = "100.0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float max = Convert.ToSingle (args.text);
                    float min = Convert.ToSingle (((SettingsTextBox)settings ["Min Dimming"]).textBox.text);

                    if (max < min) {
                        MessageBox.Show ("Maximum cannot be less than minimum");
                        args.keepText = false;
                    }
                } catch {
                    MessageBox.Show ("Improper float format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

            t = new SettingsTextBox ("Min Dimming");
            if ((this.fixtureName.IsNotEmpty ()) && (isDimming))
                t.textBox.text = Lighting.GetMinDimmingLevel (this.fixtureName).ToString ();
            else
                t.textBox.text = "0.0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float min = Convert.ToSingle (args.text);
                    float max = Convert.ToSingle (((SettingsTextBox)settings ["Max Dimming"]).textBox.text);

                    if (min > max) {
                        MessageBox.Show ("Minimum cannot be greater than maximum");
                        args.keepText = false;
                    }
                } catch {
                    MessageBox.Show ("Improper float format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

            s = new SettingsSelectorSwitch ("Dimming Type", "0-10V", "PWM");
            if ((this.fixtureName.IsNotEmpty ()) && (isDimming)) {
                if (Lighting.GetDimmingType (fixtureName) == AnalogType.ZeroTen)
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 0;
            AddOptionalSetting (s);

            DrawSettings ();
        }

        protected void ParseOutlet (string s, ref int g, ref int i) {
            int idx = s.IndexOf ('.');
            string psName = s.Substring (0, idx);
            g = (byte)Power.GetPowerStripIndex (psName);
            i = Convert.ToByte (s.Substring (idx + 2));
        }

        protected void ParseChannnel (string s, ref int g, ref int i) {
            int idx = s.IndexOf ('.');
            string cardName = s.Substring (0, idx);
            g = AquaPicDrivers.AnalogOutput.GetCardIndex (cardName);
            i = Convert.ToInt32 (s.Substring (idx + 2));
        }

        protected bool OnSave (object sender) {
            string name = ((SettingsTextBox)settings ["Name"]).textBox.text;

            string outletStr = ((SettingsComboBox)settings ["Outlet"]).combo.activeText;
            IndividualControl outletIc = IndividualControl.Empty;

            LightingTime lTime = LightingTime.Daytime;
            try {
                SettingsSelectorSwitch s = settings ["Lighting Time"] as SettingsSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    lTime = LightingTime.Nighttime;
            } catch {
                return false;
            }

            bool highTempLockout = true;
            try {
                SettingsSelectorSwitch s = settings ["Temp Lockout"] as SettingsSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    highTempLockout = false;
            } catch {
                return false;
            }

            bool autoTimeUpdate = true;
            try {
                SettingsSelectorSwitch s = settings ["Auto Time Update"] as SettingsSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    autoTimeUpdate = false;
            } catch {
                return false;
            }

            int onOffset = Convert.ToInt32 (((SettingsTextBox)settings ["On Time Offset"]).textBox.text);
            int offOffset = Convert.ToInt32 (((SettingsTextBox)settings ["Off Time Offset"]).textBox.text);

            bool dimmingFixture = true;
            try {
                SettingsSelectorSwitch s = settings ["Dimming Fixture"] as SettingsSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    dimmingFixture = false;
            } catch {
                return false;
            }

            var chStr = ((SettingsComboBox)settings ["Dimming Channel"]).combo.activeText;
            var chIc = IndividualControl.Empty;

            var maxDimming = Convert.ToSingle (((SettingsTextBox)settings ["Max Dimming"]).textBox.text);
            var minDimming = Convert.ToSingle (((SettingsTextBox)settings ["Min Dimming"]).textBox.text);

            AnalogType aType = AnalogType.ZeroTen;
            try {
                var s = settings ["Dimming Type"] as SettingsSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    aType = AnalogType.PWM;
            } catch {
                return false;
            }

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "lightingProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            if (fixtureName.IsEmpty ()) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid probe name");
                    return false;
                }

                if ((settings["Outlet"] as SettingsComboBox).combo.activeIndex == -1) {
                    MessageBox.Show ("Please select an outlet");
                    return false;
                }

                ParseOutlet (outletStr, ref outletIc.Group, ref outletIc.Individual);

                if (dimmingFixture) {
                    if (((SettingsComboBox)settings ["Dimming Channel"]).combo.activeIndex == -1) {
                        MessageBox.Show ("Please select a dimming channel");
                        return false;
                    }

                    ParseChannnel (chStr, ref chIc.Group, ref chIc.Individual);

                    Lighting.AddLight (name, outletIc, chIc, minDimming, maxDimming, aType, lTime, highTempLockout);
                } else {
                    Lighting.AddLight (name, outletIc, lTime, highTempLockout);
                }

                if (autoTimeUpdate)
                    Lighting.SetupAutoOnOffTime (name, onOffset, offOffset);

                JObject jobj = new JObject ();
                if (dimmingFixture)
                    jobj.Add (new JProperty ("type", "dimming"));
                else
                    jobj.Add (new JProperty ("type", "notDimmable"));
                jobj.Add (new JProperty ("name", name));
                jobj.Add (new JProperty ("powerStrip", Power.GetPowerStripName (outletIc.Group)));
                jobj.Add (new JProperty ("outlet", outletIc.Individual.ToString ()));
                if (lTime == LightingTime.Daytime)
                    jobj.Add (new JProperty ("lightingTime", "day"));
                else
                    jobj.Add (new JProperty ("lightingTime", "night"));
                jobj.Add (new JProperty ("highTempLockout", highTempLockout.ToString ()));
                if (dimmingFixture) {
                    jobj.Add (new JProperty ("dimmingCard", AquaPicDrivers.AnalogOutput.GetCardName (chIc.Group)));
                    jobj.Add (new JProperty ("channel", chIc.Individual.ToString ()));
                    jobj.Add (new JProperty ("minDimmingOutput", minDimming.ToString ()));
                    jobj.Add (new JProperty ("maxDimmingOutput", maxDimming.ToString ()));
                    jobj.Add (new JProperty ("analogType", aType.ToString ()));
                }
                jobj.Add (new JProperty ("autoTimeUpdate", autoTimeUpdate.ToString ()));
                jobj.Add (new JProperty ("onTimeOffset", onOffset.ToString ()));
                jobj.Add (new JProperty ("offTimeOffset", offOffset.ToString ()));

                ((JArray)jo ["lightingFixtures"]).Add (jobj);

                fixtureName = name;
            } else {
                bool isDimming = Lighting.IsDimmingFixture (fixtureName);
                if (isDimming == dimmingFixture) {
                    string oldName = fixtureName;
                    if (oldName != name) {
                        Lighting.SetFixtureName (fixtureName, name);
                        fixtureName = name;
                    }

                    if (!outletStr.StartsWith ("Current:")) {
                        ParseOutlet (outletStr, ref outletIc.Group, ref outletIc.Individual);
                        Lighting.SetFixtureOutletIndividualControl (fixtureName, outletIc);
                    } else {
                        outletIc = Lighting.GetFixtureOutletIndividualControl (fixtureName);
                    }

                    Lighting.SetFixtureLightingTime (fixtureName, lTime);

                    Lighting.SetFixtureTemperatureLockout (fixtureName, highTempLockout);

                    if (autoTimeUpdate)
                        Lighting.SetupAutoOnOffTime (fixtureName, onOffset, offOffset);

                    if (dimmingFixture) {
                        Lighting.SetMaxDimmingLevel (fixtureName, maxDimming);
                        Lighting.SetMinDimmingLevel (fixtureName, minDimming);

                        Lighting.SetDimmingType (fixtureName, aType);

                        if (!chStr.StartsWith ("Current:")) {
                            ParseChannnel (chStr, ref chIc.Group, ref chIc.Individual);
                            Lighting.SetDimmingChannelIndividualControl (fixtureName, chIc);
                        }
                        chIc = Lighting.GetDimmingChannelIndividualControl (fixtureName);
                    }

                    JArray ja = jo ["lightingFixtures"] as JArray;

                    int arrIdx = -1;
                    for (int i = 0; i < ja.Count; ++i) {
                        string n = (string)ja [i] ["name"];
                        if (oldName == n) {
                            arrIdx = i;
                            break;
                        }
                    }

                    if (arrIdx == -1) {
                        MessageBox.Show ("Something went wrong");
                        return false;
                    }

                    ((JArray)jo["lightingFixtures"])[arrIdx]["name"] = name;
                    ((JArray)jo["lightingFixtures"])[arrIdx]["powerStrip"] = Power.GetPowerStripName (outletIc.Group);
                    ((JArray)jo["lightingFixtures"])[arrIdx]["outlet"] = outletIc.Individual.ToString ();
                    if (lTime == LightingTime.Daytime)
                        ((JArray)jo["lightingFixtures"])[arrIdx]["lightingTime"] = "day";
                    else
                        ((JArray)jo["lightingFixtures"])[arrIdx]["lightingTime"] = "night";
                    ((JArray)jo["lightingFixtures"])[arrIdx]["highTempLockout"] = highTempLockout.ToString ();
                    ((JArray)jo["lightingFixtures"])[arrIdx]["autoTimeUpdate"] = autoTimeUpdate.ToString ();
                    ((JArray)jo["lightingFixtures"])[arrIdx]["onTimeOffset"] = onOffset.ToString ();
                    ((JArray)jo["lightingFixtures"])[arrIdx]["offTimeOffset"] = offOffset.ToString ();

                    if (dimmingFixture) {
                        ((JArray)jo["lightingFixtures"])[arrIdx]["dimmingCard"] = AquaPicDrivers.AnalogOutput.GetCardName (chIc.Group);
                        ((JArray)jo["lightingFixtures"])[arrIdx]["channel"] = chIc.Individual.ToString ();
                        ((JArray)jo["lightingFixtures"])[arrIdx]["minDimmingOutput"] = minDimming.ToString ();
                        ((JArray)jo["lightingFixtures"])[arrIdx]["maxDimmingOutput"] = maxDimming.ToString ();
                        ((JArray)jo["lightingFixtures"])[arrIdx]["analogType"] = aType.ToString ();
                    }
                } else {
                    MessageBox.Show ("Can't change dimmablility");
                    return false;
                }
            }

            File.WriteAllText (path, jo.ToString ());

            return true;
        }

        protected bool OnDelete (object sender) {
            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "lightingProperties.json");

            string json = File.ReadAllText (path);

            JObject jo = (JObject)JToken.Parse (json);
            JArray ja = jo ["lightingFixtures"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja [i] ["name"];
                if (fixtureName == n) {
                    arrIdx = i;
                    break;
                }
            }

            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ((JArray)jo ["lightingFixtures"]).RemoveAt (arrIdx);

            File.WriteAllText (path, jo.ToString ());

            Lighting.RemoveLight (fixtureName);

            return true;
        }
    }
}

