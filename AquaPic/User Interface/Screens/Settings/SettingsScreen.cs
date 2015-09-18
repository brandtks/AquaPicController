using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class SettingsWindow : WindowBase
    {
        private AquaPicGUI tw;

        public SettingsWindow (params object[] options) : base () {
            if (options.Length > 0) {
                tw = options [0] as AquaPicGUI;
                if (tw != null) {
                    if (tw.IsTopLevel)
                        tw.ShowDecoration ();
                }
            }

            Show ();
        }

        public override void Dispose () {
            if (tw != null) {
                if (tw.IsTopLevel)
                    tw.HideDecoration ();
            }
            base.Dispose ();
        }
    }
}

