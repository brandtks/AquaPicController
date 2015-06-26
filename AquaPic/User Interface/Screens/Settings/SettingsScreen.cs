using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class SettingsWindow : WindowBase
    {
        public SettingsWindow (params object[] options) : base () {
            MyAmpMeter am = new MyAmpMeter ();
            am.currentAmps = 5.54;
            Put (am, 300, 150);
            am.Show ();

            ShowAll ();
        }
    }
}

