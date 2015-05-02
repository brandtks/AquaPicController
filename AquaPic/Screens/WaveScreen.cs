using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public partial class WaveWindow : MyBackgroundWidget
    {
        public WaveWindow (MenuReleaseHandler OnMenuRelease) : base (3, OnMenuRelease) {
            Show ();
        }
    }
}

