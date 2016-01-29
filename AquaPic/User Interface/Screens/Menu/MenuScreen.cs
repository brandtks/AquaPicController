using System;
using System.Collections.Generic;
using Gtk;
using Cairo;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class MenuWindow : WindowBase
    {
        public MenuWindow (params object[] options) : base () {
            var box = new TouchGraphicalBox (780, 395);
            Put (box, 10, 30);

            List<string> screenNames = new List<string> ();
            foreach (var screen in AquaPicGUI.allWindows.Keys)
                screenNames.Add (screen);

            screenNames.Sort ();

            int x = 15;
            int y = 35;
            foreach (var name in screenNames) {
                ScreenData screen = AquaPicGUI.allWindows [name];
                if (screen.showInMenu) {
                    var b = new TouchButton ();
                    b.SetSizeRequest (250, 30);
                    b.text = screen.name;
                    b.textColor = "black";
                    b.ButtonReleaseEvent += OnButtonClick;
                    Put (b, x, y);

                    x += 260;  
                    if (x >= 795) {
                        x = 15;
                        y += 40;
                    }
                }
            }

            ShowAll ();
        }

        protected void OnButtonClick (object sender, ButtonReleaseEventArgs args) {
            TouchButton b = sender as TouchButton;

            if (b.text == "Settings") {
                var tl = this.Toplevel;
                AquaPicGUI.ChangeScreens (b.text, tl);
            } else
                AquaPicGUI.ChangeScreens (b.text);
        }
    }
}

