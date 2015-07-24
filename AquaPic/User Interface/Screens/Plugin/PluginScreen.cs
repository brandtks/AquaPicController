/***************************************************************************************************/
/*        NOT COMPILED                                                                             */
/***************************************************************************************************/

using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class PluginWindow : WindowBase
    {
        public PluginWindow (params object[] options) : base () {
            var box = new MyBox (780, 395);
            Put (box, 10, 30);

            int x = 15;
            int y = 35;
            foreach (var p in Plugin.AllPlugins.Values) {
                var b = new TouchButton ();
                b.SetSizeRequest (250, 30);
                b.text = p.name;
                b.textColor = "black";
                if (!p.flags.HasFlag (ScriptFlags.Compiled))
                    b.buttonColor = "compl";
                b.ButtonReleaseEvent += OnButtonClick;
                Put (b, x, y);

                x += 260;

                if (x >= 795) {
                    x = 15;
                    y += 40;
                }
            }

            ShowAll ();
        }

        protected void OnButtonClick (object sender, ButtonReleaseEventArgs args) {
            TouchButton b = sender as TouchButton;
            GuiGlobal.ChangeScreens ("Edit Plugin", Plugin.AllPlugins [b.text]);
        }
    }
}