using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic
{
    public class EditPluginWindow : MyBackgroundWidget
    {
        public EditPluginWindow (params object[] options) : base () {
            var box = new MyBox (780, 395);
            Put (box, 10, 30);

            var backButton = new TouchButton ();
            backButton.text = "back";
            backButton.SetSizeRequest (100, 30);
            backButton.ButtonReleaseEvent += (o, args) => GuiGlobal.ChangeScreens ("Plugins");
            Put (backButton, 15, 390);

            BaseScript script = null;
            foreach (var o in options) {
                script = o as BaseScript;

                if (script != null) {
                    var tb = new TouchTextBox ();
                    tb.WidthRequest = 200;
                    tb.text = script.name;
                    Put (tb, 15, 35);

                    break;
                }
            }

            if (script == null) {
                var label = new TouchLabel ();
                label.text = "No Plugin Selected";
                label.textColor = "secb";
                label.textSize = 13;
                label.WidthRequest = 780;
                label.textAlignment = MyAlignment.Center;
                Put (label, 10, 35);

                backButton.text = "Select Plugin";
            }

            ShowAll ();
        }
    }
}

