using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public partial class MainWindow : MyBackgroundWidget
    {
        public MainWindow (ButtonReleaseEventHandler OnTouchButtonRelease) : base ("Main", OnTouchButtonRelease) {
            TouchProgressBar bar = new TouchProgressBar ();
            bar.CurrentProgress = 0.50f;
            bar.EnableTouch = true;
            Put (bar, 200, 200);
            bar.Show ();

            Show ();
        }
    }
}

