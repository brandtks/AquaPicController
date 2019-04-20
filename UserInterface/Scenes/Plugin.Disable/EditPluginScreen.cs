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
using System.Text;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Service;

namespace AquaPic.UserInterface
{
    public class EditPluginWindow : WindowBase
    {
        protected TextView tv;
        protected VScrollbar vsb;

        public EditPluginWindow (params object[] options) : base () {
            var box = new MyBox (780, 395);
            Put (box, 10, 30);

            var backButton = new TouchButton ();
            backButton.text = "back";
            backButton.SetSizeRequest (100, 40);
            backButton.ButtonReleaseEvent += (o, args) => GuiGlobal.ChangeScreens ("Plugins");
            Put (backButton, 15, 380);

            var label = new TouchLabel ();
            label.textSize = 13;
            label.WidthRequest = 780;
            label.textAlignment = MyAlignment.Center;

            tv = new TextView ();
            tv.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            tv.ModifyText (StateType.Normal, MyColor.NewGtkColor ("black"));
            tv.ModifyBase (StateType.Normal, MyColor.NewGtkColor ("grey4"));
            tv.CanFocus = false;

            ScrolledWindow sw = new ScrolledWindow ();
            sw.SetSizeRequest (770, 315);
            sw.Add (tv);
            Put (sw, 15, 60);

            BaseScript script = null;
            foreach (var opt in options) {
                script = opt as BaseScript;

                if (script != null) {
                    label.text = script.name;
                    label.textColor = "pri";

                    DisplayErrors (script);

                    var b = new TouchButton ();
                    b.SetSizeRequest (100, 40);
                    b.text = "Recompile and Load";
                    b.ButtonReleaseEvent += (o, args) => {
                        if (script.errors.Count != 0) {
                            script.CompileAndLoad ();
                            if (script.flags.HasFlag (ScriptFlags.Initializer))
                                script.RunInitialize ();
                            DisplayErrors (script);
                            tv.QueueDraw ();
                        } else {
                            MessageBox.Show ("Script is already running.\nCannot recomile and load");
                        }
                    };
                    Put (b, 685, 380);

                    break;
                }
            }

            if (script == null) {
                label.text = "No Plugin Selected";
                label.textColor = "secb";

                backButton.text = "Select Plugin";
            }

            Put (label, 10, 35);
           
            ShowAll ();
        }

        protected void DisplayErrors (BaseScript script) {
            TextBuffer tb = tv.Buffer;
            if (script.errors.Count == 0) {
                tb.Text = "No errors in script";
            } else {
                tb.Text = string.Empty;

                foreach (var error in script.errors) {
                    StringBuilder sb = new StringBuilder ();
                    sb.AppendLine ("Script failed at " + error.errorLocation);
                    sb.AppendLine (error.message);
                    sb.AppendLine ();

                    tb.Text += sb.ToString ();
                }
            }
        }
    }
}

