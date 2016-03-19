using System;
using System.IO;
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TouchWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Utilites;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class FixtureSettings : TouchSettingsDialog
    {
        int fixtureIdx;

        public FixtureSettings (string name, int fixtureId, bool includeDelete) : base (name, includeDelete) {
            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;

            fixtureIdx = fixtureId;

            var t = new SettingTextBox ();
            t.text = "Name";
            if (fixtureIdx != -1)
                t.textBox.text = Lighting.GetFixtureName (fixtureIdx);
            else
                t.textBox.text = "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!Lighting.FixtureNameOk (args.text)) {
                    MessageBox.Show ("Heater name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingComboBox ();
            c.label.text = "Outlet";
            if (fixtureIdx != -1) {
                IndividualControl ic = Lighting.GetFixtureOutletIndividualControl (fixtureIdx);
                string psName = Power.GetPowerStripName (ic.Group);
                c.combo.List.Add (string.Format ("Current: {0}.p{1}", psName, ic.Individual));
                c.combo.Active = 0;
            } else {
                c.combo.NonActiveMessage = "Select outlet";
            }
            c.combo.List.AddRange (Power.GetAllAvaiblableOutlets ());
            AddSetting (c);

            var s = new SettingSelectorSwitch ("Day", "Night");
            s.text = "Lighting Time";
            if (fixtureIdx != -1) {
                if (Lighting.GetFixtureLightingTime (fixtureIdx) == LightingTime.Daytime)
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 0;
            AddSetting (s);

            s = new SettingSelectorSwitch ();
            s.text = "Temp Lockout";
            if (fixtureIdx != -1) {
                if (Lighting.GetFixtureTemperatureLockout (fixtureIdx))
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 0;
            AddSetting (s);

            s = new SettingSelectorSwitch ();
            s.text = "Auto Time Update";
            if (fixtureIdx != -1) {
                if (Lighting.GetFixtureMode (fixtureIdx) == Mode.Auto)
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 1;
            AddSetting (s);

            /*
            t = new SettingTextBox ();
            t.text = "Default On Time";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.defaultSunRise;
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time.Parse (args.text);
                } catch {
                    MessageBox.Show ("Incorrect time format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Default Off Time";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.defaultSunRise;
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time.Parse (args.text);
                } catch {
                    MessageBox.Show ("Incorrect time format");
                    args.keepText = false;
                }
            };
            AddSetting (t);
            */

            t = new SettingTextBox ();
            t.text = "On Time Offset";
            if (fixtureIdx != -1)
                t.textBox.text = Lighting.GetFixtureOnTimeOffset (fixtureIdx).ToString ();
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

            t = new SettingTextBox ();
            t.text = "Off Time Offset";
            t.textBox.includeTimeFunctions = true;
            if (fixtureIdx != -1)
                t.textBox.text = Lighting.GetFixtureOffTimeOffset (fixtureIdx).ToString ();
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
            if (fixtureIdx != -1)
                isDimming = Lighting.IsDimmingFixture (fixtureIdx);
            else
                isDimming = false;

            s = new SettingSelectorSwitch ("Yes", "No");
            s.text = "Dimming Fixture";
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

            c = new SettingComboBox ();
            c.label.text = "Dimming Channel";
            if ((fixtureIdx != -1) && (isDimming)) {
                IndividualControl ic = Lighting.GetDimmingChannelIndividualControl (fixtureIdx);
                string cardName = AquaPicDrivers.AnalogOutput.GetCardName (ic.Group);
                c.combo.List.Add (string.Format ("Current: {0}.q{1}", cardName, ic.Individual));
                c.combo.Active = 0;
            } else {
                c.combo.NonActiveMessage = "Select outlet";
            }
            c.combo.List.AddRange (AquaPicDrivers.AnalogOutput.GetAllAvaiableChannels ());
            AddOptionalSetting (c);

            t = new SettingTextBox ();
            t.text = "Max Dimming";
            if ((fixtureIdx != -1) && (isDimming))
                t.textBox.text = Lighting.GetMaxDimmingLevel (fixtureIdx).ToString ();
            else
                t.textBox.text = "100.0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float max = Convert.ToSingle (args.text);
                    float min = Convert.ToSingle (((SettingTextBox)settings ["Min Dimming"]).textBox.text);

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

            t = new SettingTextBox ();
            t.text = "Min Dimming";
            if ((fixtureIdx != -1) && (isDimming))
                t.textBox.text = Lighting.GetMinDimmingLevel (fixtureIdx).ToString ();
            else
                t.textBox.text = "0.0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float min = Convert.ToSingle (args.text);
                    float max = Convert.ToSingle (((SettingTextBox)settings ["Max Dimming"]).textBox.text);

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

            #if SELECTABLE_ANALOG_OUTPUT_TYPE
            s = new SettingSelectorSwitch ("0-10", "PWM");
            s.text = "Dimming Type";
            if ((fixtureIdx != -1) && (isDimming)) {
                if (Lighting.GetDimmingType (fixtureIdx) == AnalogType.ZeroTen)
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 0;
            AddOptionalSetting (s);
            #endif

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
            string name = ((SettingTextBox)settings ["Name"]).textBox.text;

            string outletStr = ((SettingComboBox)settings ["Outlet"]).combo.activeText;
            IndividualControl outletIc = IndividualControl.Empty;

            LightingTime lTime = LightingTime.Daytime;
            try {
                SettingSelectorSwitch s = settings ["Lighting Time"] as SettingSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    lTime = LightingTime.Nighttime;
            } catch {
                return false;
            }

            bool highTempLockout = true;
            try {
                SettingSelectorSwitch s = settings ["Temp Lockout"] as SettingSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    highTempLockout = false;
            } catch {
                return false;
            }

            bool autoTimeUpdate = true;
            try {
                SettingSelectorSwitch s = settings ["Auto Time Update"] as SettingSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    autoTimeUpdate = false;
            } catch {
                return false;
            }

            int onOffset = Convert.ToInt32 (((SettingTextBox)settings ["On Time Offset"]).textBox.text);
            int offOffset = Convert.ToInt32 (((SettingTextBox)settings ["Off Time Offset"]).textBox.text);

            bool dimmingFixture = true;
            try {
                SettingSelectorSwitch s = settings ["Dimming Fixture"] as SettingSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    dimmingFixture = false;
            } catch {
                return false;
            }

            string chStr = ((SettingComboBox)settings ["Dimming Channel"]).combo.activeText;
            IndividualControl chIc = IndividualControl.Empty;

            float maxDimming = Convert.ToSingle (((SettingTextBox)settings ["Max Dimming"]).textBox.text);
            float minDimming = Convert.ToSingle (((SettingTextBox)settings ["Min Dimming"]).textBox.text);

            AnalogType aType = AnalogType.ZeroTen;
            try {
                SettingSelectorSwitch s = settings ["Dimming Type"] as SettingSelectorSwitch;
                if (s.selectorSwitch.currentSelected != 0)
                    aType = AnalogType.PWM;
            } catch {
                return false;
            }

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "lightingProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            if (fixtureIdx == -1) {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid probe name");
                    return false;
                }

                if (((SettingComboBox)settings ["Outlet"]).combo.Active == -1) {
                    MessageBox.Show ("Please select an outlet");
                    return false;
                }

                ParseOutlet (outletStr, ref outletIc.Group, ref outletIc.Individual);

                if (dimmingFixture) {
                    if (((SettingComboBox)settings ["Dimming Channel"]).combo.Active == -1) {
                        MessageBox.Show ("Please select a dimming channel");
                        return false;
                    }

                    ParseChannnel (chStr, ref chIc.Group, ref chIc.Individual);

                    fixtureIdx = Lighting.AddLight (name, outletIc, chIc, minDimming, maxDimming, aType, lTime, highTempLockout);
                } else {
                    fixtureIdx = Lighting.AddLight (name, outletIc, lTime, highTempLockout);
                }

                if (autoTimeUpdate)
                    Lighting.SetupAutoOnOffTime (fixtureIdx, onOffset, offOffset);

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
            } else {
                bool isDimming = Lighting.IsDimmingFixture (fixtureIdx);
                if (isDimming == dimmingFixture) { 
                    string oldName = Lighting.GetFixtureName (fixtureIdx);
                    if (oldName != name)
                        Lighting.SetFixtureName (fixtureIdx, name);

                    if (!outletStr.StartsWith ("Current:")) {
                        ParseOutlet (outletStr, ref outletIc.Group, ref outletIc.Individual);
                        Lighting.SetFixtureOutletIndividualControl (fixtureIdx, outletIc);
                    }
                    outletIc = Lighting.GetFixtureOutletIndividualControl (fixtureIdx);
                        
                    Lighting.SetFixtureLightingTime (fixtureIdx, lTime);

                    Lighting.SetFixtureTemperatureLockout (fixtureIdx, highTempLockout);

                    if (autoTimeUpdate)
                        Lighting.SetupAutoOnOffTime (fixtureIdx, onOffset, offOffset);

                    if (dimmingFixture) {
                        Lighting.SetMaxDimmingLevel (fixtureIdx, maxDimming);
                        Lighting.SetMinDimmingLevel (fixtureIdx, minDimming);

                        Lighting.SetDimmingType (fixtureIdx, aType);

                        if (!chStr.StartsWith ("Current:")) {
                            ParseChannnel (chStr, ref chIc.Group, ref chIc.Individual);
                            Lighting.SetDimmingChannelIndividualControl (fixtureIdx, chIc);
                        }
                        chIc = Lighting.GetDimmingChannelIndividualControl (fixtureIdx);
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

                    ((JArray)jo ["lightingFixtures"]) [arrIdx] ["name"] = name;
                    ((JArray)jo ["lightingFixtures"]) [arrIdx] ["powerStrip"] = Power.GetPowerStripName (outletIc.Group);
                    ((JArray)jo ["lightingFixtures"]) [arrIdx] ["outlet"] = outletIc.Individual.ToString ();
                    if (lTime == LightingTime.Daytime)
                        ((JArray)jo ["lightingFixtures"]) [arrIdx] ["lightingTime"] = "day";
                    else
                        ((JArray)jo ["lightingFixtures"]) [arrIdx] ["lightingTime"] = "night";
                    ((JArray)jo ["lightingFixtures"]) [arrIdx] ["highTempLockout"] = highTempLockout.ToString ();
                    ((JArray)jo ["lightingFixtures"]) [arrIdx] ["autoTimeUpdate"] = autoTimeUpdate.ToString ();
                    ((JArray)jo ["lightingFixtures"]) [arrIdx] ["onTimeOffset"] = onOffset.ToString ();
                    ((JArray)jo ["lightingFixtures"]) [arrIdx] ["offTimeOffset"] = offOffset.ToString ();

                    if (dimmingFixture) {
                        ((JArray)jo ["lightingFixtures"]) [arrIdx] ["dimmingCard"] = AquaPicDrivers.AnalogOutput.GetCardName (chIc.Group);
                        ((JArray)jo ["lightingFixtures"]) [arrIdx] ["channel"] = chIc.Individual.ToString ();
                        ((JArray)jo ["lightingFixtures"]) [arrIdx] ["minDimmingOutput"] = minDimming.ToString ();
                        ((JArray)jo ["lightingFixtures"]) [arrIdx] ["maxDimmingOutput"] = maxDimming.ToString ();
                        ((JArray)jo ["lightingFixtures"]) [arrIdx] ["analogType"] = aType.ToString ();
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
            string name = Lighting.GetFixtureName (fixtureIdx);

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "lightingProperties.json");

            string json = File.ReadAllText (path);

            JObject jo = (JObject)JToken.Parse (json);
            JArray ja = jo ["lightingFixtures"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja [i] ["name"];
                if (name == n) {
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

            Lighting.RemoveLight (fixtureIdx);

            return true;
        }
    }
}

