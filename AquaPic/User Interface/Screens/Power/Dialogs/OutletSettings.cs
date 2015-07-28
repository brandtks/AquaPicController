using System;
using System.IO;
using System.Text;
using Gtk;
using Cairo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyWidgetLibrary;
using AquaPic.Utilites;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class OutletSettings : TouchSettingsDialog
    {
        public TextView tv;

        public OutletSettings (string name, bool includeDelete, IndividualControl ic)
            : base (name, includeDelete, 400)
        {
            tv = new TextView ();
            tv.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            tv.ModifyBase (StateType.Normal, MyColor.NewGtkColor ("grey4"));
            tv.ModifyText (StateType.Normal, MyColor.NewGtkColor ("black"));

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "powerProperties.json");

            string jstring = File.ReadAllText (path);
            JArray ja = (JArray)JToken.Parse (jstring);

            string code = "These are not the scripts you're looking for";
            foreach (var jt in ja) {
                JObject jo = jt as JObject;
                string n = (string)jo ["name"];
                if (n == name) {
                    StringBuilder sb = new StringBuilder ();
                    JArray ja2 = (JArray)jo ["conditions"];
                    foreach (var jt2 in ja2)
                        sb.AppendLine ((string)jt2);
                    code = sb.ToString ();
                    break;
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
        }

//        protected bool OnSave (object sender) {
//
//        }
//
//        protected bool OnDelete (object sender) {
//
//        }
    }
}

