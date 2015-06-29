using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class SettingsWindow : WindowBase
    {
        public SettingsWindow (params object[] options) : base () {
            TouchCurvedProgressBar c = new TouchCurvedProgressBar ();
            c.currentProgress = 0.85f;
            Put (c, 300, 150);
            c.Show ();

            ShowAll ();
        }
    }
}

