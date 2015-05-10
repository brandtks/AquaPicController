using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class SettingsWindow : MyBackgroundWidget
    {
        public SettingsWindow (MenuReleaseHandler OnMenuRelease) : base (5, OnMenuRelease) {
            Show ();
        }
    }
}

