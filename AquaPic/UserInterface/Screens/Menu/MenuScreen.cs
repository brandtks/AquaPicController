using System;
using System.Collections.Generic;
using Gtk;
using Cairo;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class MenuWindow : SceneBase
    {
        public MenuWindow (params object[] options) : base () {
            sceneTitle = "Menu";

            List<string> screenNames = new List<string> ();
            foreach (var screen in AquaPicGui.AquaPicUserInterface.scenes.Keys)
                screenNames.Add (screen);

            screenNames.Sort ();

            int x = 60;
            int y = 80;
            foreach (var name in screenNames) {
                SceneData screen = AquaPicGui.AquaPicUserInterface.scenes [name];
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
            AquaPicGui.AquaPicUserInterface.ChangeScreens (b.text, tl, AquaPicGui.AquaPicUserInterface.currentScene);
        }
    }
}

