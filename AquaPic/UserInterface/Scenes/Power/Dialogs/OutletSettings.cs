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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Globals;
using AquaPic.Drivers;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class OutletSettings : TouchSettingsDialog
    {
        public TextView tv;
        public IndividualControl ic;

        public OutletSettings (string name, bool includeDelete, IndividualControl ic)
            : base (name, includeDelete, 400)
        {
            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;

            this.ic = ic;

            tv = new TextView ();
            tv.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            tv.ModifyBase (StateType.Normal, TouchColor.NewGtkColor ("grey4"));
            tv.ModifyText (StateType.Normal, TouchColor.NewGtkColor ("black"));

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "powerProperties.json");

            string jstring = File.ReadAllText (path);
            JArray ja = (JArray)JToken.Parse (jstring);
            JObject jo = null;

            string code = "These are not the scripts you're looking for.";
            string psName = Power.GetPowerStripName (ic.Group);
            string outletId = ic.Individual.ToString ();
            foreach (var jt in ja) {
                jo = jt as JObject;
                string n = (string)jo ["powerStrip"];
                if (n == psName) {
                    n = (string)jo ["outlet"];
                    if (n == outletId) {
                        StringBuilder sb = new StringBuilder ();
                        JArray ja2 = (JArray)jo ["conditions"];
                        foreach (var jt2 in ja2)
                            sb.AppendLine ((string)jt2);
                        code = sb.ToString ();
                        break;
                    }
                }
            }

            TextBuffer tb = tv.Buffer;
            tb.Text = code;

            ScrolledWindow sw = new ScrolledWindow ();
            sw.SetSizeRequest (590, 200);
            sw.Add (tv);
            tv.Show ();
            fix.Put (sw, 5, 160);
            sw.Show ();

            var verifyBtn = new TouchButton ();
            verifyBtn.text = "Verify";
            verifyBtn.SetSizeRequest (100, 30);
            verifyBtn.ButtonReleaseEvent += (o, args) => {
                if (CheckCode ())
                    MessageBox.Show ("Code Ok");
            };
            fix.Put (verifyBtn, 275, 365);

            var t = new SettingsTextBox ();
            t.text = "Name";
            if (includeDelete)
                t.textBox.text = Power.GetOutletName (ic);
            else
                t.textBox.text = "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!Power.OutletNameOk (args.text)) {
                    MessageBox.Show ("Heater name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var s = new SettingSelectorSwitch ("On", "Off");
            s.text = "Fallback";
            if (includeDelete) {
                if (Power.GetOutletFallback (ic) == MyState.On)
                    s.selectorSwitch.currentSelected = 0;
                else
                    s.selectorSwitch.currentSelected = 1;
            } else
                s.selectorSwitch.currentSelected = 1;
            AddSetting (s);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            string name = ((SettingsTextBox)settings ["Name"]).textBox.text;

            MyState fallback = MyState.Off;
            try {
                SettingSelectorSwitch s = settings ["Fallback"] as SettingSelectorSwitch;
                if (s.selectorSwitch.currentSelected == 0)
                    fallback = MyState.On;
                else
                    fallback = MyState.Off;
            } catch {
                return false;
            }

            IOutletScript script;
            string[] conditions;
            if (!CheckCode (out script, out conditions))
                return false;

            JArray jcond = new JArray ();
            foreach (var cond in conditions)
                jcond.Add (cond);

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "powerProperties.json");

            string jstring = File.ReadAllText (path);
            JArray ja = (JArray)JToken.Parse (jstring);

            if (includeDelete) {
                string oldName = Power.GetOutletName (ic);
                if (oldName != name)
                    Power.SetOutletName (ic, name);

                MyState oldFallback = Power.GetOutletFallback (ic);
                if (oldFallback != fallback)
                    Power.SetOutletFallback (ic, fallback);

                Power.SetOutletConditionCheck (ic, script);

                int arrIdx = -1;
                string psName = Power.GetPowerStripName (ic.Group);
                string outletId = ic.Individual.ToString ();
                for (int i = 0; i < ja.Count; ++i) {
                    string n = (string)ja [i] ["powerStrip"];
                    if (n == psName) {
                        n = (string)ja [i] ["outlet"];
                        if (n == outletId) {
                            arrIdx = i;
                            break;
                        }
                    }
                }

                if (arrIdx == -1) {
                    MessageBox.Show ("Something went wrong");
                    return false;
                }

                ja [arrIdx] ["name"] = name;
                ja [arrIdx] ["fallback"] = fallback.ToString ();
                ja [arrIdx] ["conditions"] = jcond;
            } else {
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid outlet name");
                    return false;
                }

                var c = Power.AddOutlet (ic, name, fallback);
                c.ConditionGetter = () => {
                    return script.OutletConditionCheck ();
                };

                JObject jo = new JObject ();
                jo.Add (new JProperty ("name", name));
                jo.Add (new JProperty ("powerStrip", Power.GetPowerStripName (ic.Group)));
                jo.Add (new JProperty ("outlet", ic.Individual.ToString ()));
                jo.Add (new JProperty ("fallback", fallback.ToString ()));
                jo.Add (new JProperty ("conditions", jcond));
                ja.Add (jo);
            }

            File.WriteAllText (path, ja.ToString ());

            return true;
        }

        protected bool OnDelete (object sender) {
            Power.RemoveOutlet (ic);

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "powerProperties.json");

            string jstring = File.ReadAllText (path);
            JArray ja = (JArray)JToken.Parse (jstring);

            int arrIdx = -1;
            string psName = Power.GetPowerStripName (ic.Group);
            string outletId = ic.Individual.ToString ();
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja [i] ["powerStrip"];
                if (n == psName) {
                    n = (string)ja [i] ["outlet"];
                    if (n == outletId) {
                        arrIdx = i;
                        break;
                    }
                }
            }

            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            List<string> code = new List<string> ();
            JArray ja2 = (JArray)ja [arrIdx] ["conditions"];
            foreach (var jt2 in ja2)
                code.Add ((string)jt2);

            Script.UndoPreprocessor (code);

            ja.RemoveAt (arrIdx);

            File.WriteAllText (path, ja.ToString ());

            return true;
        }

        protected bool CheckCode (out IOutletScript script, out string[] conditions) {
            if (tv.Buffer.Text == "These are not the scripts you're looking for") {
                MessageBox.Show ("Invalid script");
                script = null;
                conditions = null;
                return false;
            }

            conditions = tv.Buffer.Text.Split (new string[] {Environment.NewLine, "\n"}, StringSplitOptions.None);
//            List<string> cond = new List<string> ();
//            foreach (var c in conditions)
//                cond.Add (c);

            try {
                Script.UndoPreprocessor (conditions);
                script = Script.CompileOutletConditionCheckNoCatch (conditions);
                script.OutletConditionCheck ();
            } catch (Exception ex) {
                MessageBox.Show (ex.Message);
                script = null;
                conditions = null;
                return false;
            }

            return true;
        }

        protected bool CheckCode () {
            IOutletScript script = null;
            string[] conditions;
            return CheckCode (out script, out conditions);
        }
    }
}

