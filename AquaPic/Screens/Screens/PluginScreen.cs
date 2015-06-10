using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic
{
    public class PluginWindow : MyBackgroundWidget
    {
        public PluginWindow () : base () {
            var box = new MyBox (780, 395);
            Put (box, 10, 30);

            int x = 15;
            int y = 35;
            foreach (var p in Plugin.AllPlugins.Values) {
                var b = new TouchButton ();
                b.SetSizeRequest (250, 30);
                b.text = p.name;
                b.textColor = "black";
                if (p.flags.HasFlag (ScriptFlags.Compiled))
                    b.buttonColor = "pri";
                else
                    b.buttonColor = "compl";
                Put (b, x, y);

                x += 260;

                if (x >= 795) {
                    x = 15;
                    y += 40;
                }
            }

            ShowAll ();
        }
    }
}