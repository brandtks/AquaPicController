using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public partial class MainWindow : MyBackgroundWidget
    {
        public MainWindow (ButtonReleaseEventHandler OnTouchButtonRelease) : base ("Main", OnTouchButtonRelease) {
            Show ();
        }
    }
}

