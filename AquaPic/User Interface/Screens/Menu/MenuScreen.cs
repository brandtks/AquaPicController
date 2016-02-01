﻿using System;
using System.Collections.Generic;
using Gtk;
using Cairo;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class MenuWindow : WindowBase
    {
        public MenuWindow (params object[] options) : base () {
            //var box = new TouchGraphicalBox (780, 395);
            //Put (box, 10, 30);

            screenTitle = "Menu";

            List<string> screenNames = new List<string> ();
            foreach (var screen in AquaPicGUI.allWindows.Keys)
                screenNames.Add (screen);

            screenNames.Sort ();

            int x = 60;
            int y = 80;
            foreach (var name in screenNames) {
                ScreenData screen = AquaPicGUI.allWindows [name];
                if (screen.showInMenu) {
                    var b = new TouchButton ();
                    b.SetSizeRequest (220, 50);
                    b.text = screen.name;
                    b.textColor = "black";
                    b.ButtonReleaseEvent += OnButtonClick;
                    Put (b, x, y);

                    x += 230;  
                    if (x >= 690) {
                        x = 60;
                        y += 60;
                    }
                }
            }

            ShowAll ();
        }

        protected void OnButtonClick (object sender, ButtonReleaseEventArgs args) {
            TouchButton b = sender as TouchButton;

            var tl = this.Toplevel;
            AquaPicGUI.ChangeScreens (b.text, tl, AquaPicGUI.currentScreen);
        }
    }
}

