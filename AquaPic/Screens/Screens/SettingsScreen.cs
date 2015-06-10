using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class SettingsWindow : MyBackgroundWidget
    {
        public SettingsWindow (params object[] options) : base () {
            ShowAll ();
        }
    }
}

