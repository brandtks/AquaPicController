using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class MainWindow : MyBackgroundWidget
    {
        public MainWindow (MenuReleaseHandler OnMenuRelease) : base (0, OnMenuRelease) {
            Show ();
        }
    }
}

