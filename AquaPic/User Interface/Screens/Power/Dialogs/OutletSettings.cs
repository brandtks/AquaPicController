using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gtk;
using Cairo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyWidgetLibrary;
using AquaPic.Utilites;
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
            tv.ModifyBase (StateType.Normal, MyColor.NewGtkColor ("grey4"));
            tv.ModifyText (StateType.Normal, MyColor.NewGtkColor ("black"));

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
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
                    TouchMessageBox.Show ("Code Ok");
            };
            fix.Put (verifyBtn, 275, 365);

            var t = new SettingTextBox ();
            t.text = "Name";
            if (includeDelete)
                t.textBox.text = Power.GetOutletName (ic);
            else
                t.textBox.text = "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!Power.OutletNameOk (args.text)) {
                    TouchMessageBox.Show ("Heater name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var s = new SettingSelectorSwitch ("On", "Off");
            s.text = "Fallback";
            if (includeDelete) {
                if (Power.GetOutletFallback (ic) == MyState.On)
                    s.selectorSwitch.CurrentSelected = 0;
                else
                    s.selectorSwitch.CurrentSelected = 1;
            } else
                s.selectorSwitch.CurrentSelected = 1;
            AddSetting (s);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            string name = ((SettingTextBox)settings ["Name"]).textBox.text;

            MyState fallback = MyState.Off;
            try {
                SettingSelectorSwitch s = settings ["Fallback"] as SettingSelectorSwitch;
                if (s.selectorSwitch.CurrentSelected == 0)
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

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
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
                    TouchMessageBox.Show ("Something went wrong");
                    return false;
                }

                ja [arrIdx] ["name"] = name;
                ja [arrIdx] ["fallback"] = fallback.ToString ();
                ja [arrIdx] ["conditions"] = jcond;
            } else {
                if (name == "Enter name") {
                    TouchMessageBox.Show ("Invalid outlet name");
                    return false;
                }

                var c = Power.AddOutlet (ic, name, fallback);
                c.ConditionChecker = () => {
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

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
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
                TouchMessageBox.Show ("Something went wrong");
                return false;
            }

            ja.RemoveAt (arrIdx);

            File.WriteAllText (path, ja.ToString ());

            return true;
        }

        protected bool CheckCode (out IOutletScript script, out string[] conditions) {
            if (tv.Buffer.Text == "These are not the scripts you're looking for") {
                TouchMessageBox.Show ("Invalid script");
                script = null;
                conditions = null;
                return false;
            }

            conditions = tv.Buffer.Text.Split (new string[] {Environment.NewLine, "\n"}, StringSplitOptions.None);
            List<string> lConditions = new List<string> ();
            foreach (var cond in conditions) {
                if (!string.IsNullOrWhiteSpace (cond))
                    lConditions.Add (cond);
            }
            conditions = lConditions.ToArray ();

            try {
                script = Script.CompileNoThrowOutletConditionCheck (conditions);
                script.OutletConditionCheck ();
            } catch (Exception ex) {
                TouchMessageBox.Show (ex.Message);
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

