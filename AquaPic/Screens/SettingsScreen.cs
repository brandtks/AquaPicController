using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public partial class SettingsWindow : MyBackgroundWidget
    {
        public SettingsWindow (MenuReleaseHandler OnMenuRelease) : base (5, OnMenuRelease) {
            Show ();
        }
    }
}

