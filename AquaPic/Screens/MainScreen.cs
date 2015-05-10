using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class MainWindow : MyBackgroundWidget
    {
        public MainWindow (MenuReleaseHandler OnMenuRelease) : base (0, OnMenuRelease) {
            TouchButton b = new TouchButton ();
            b.ButtonReleaseEvent += (o, args) => {
                TouchValueInput input = new TouchValueInput ();
                input.Show ();
            };
            Put (b, 100, 100);
            b.Show ();

            Show ();
        }
    }
}

