using System;
using System.Collections.Generic;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class MenuWindow : MyBackgroundWidget
    {
        public MenuWindow (params object[] options) : base () {
            var box = new MyBox (780, 395);
            Put (box, 10, 30);

            List<string> screenNames = new List<string> ();
            foreach (var screen in GuiGlobal.allWindows.Keys)
                screenNames.Add (screen);

            screenNames.Sort ();

            int x = 15;
            int y = 35;
            foreach (var name in screenNames) {
                ScreenData screen = GuiGlobal.allWindows [name];
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
            GuiGlobal.ChangeScreens (b.text);
        }
    }
}

