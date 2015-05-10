using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class ConditionWindow : MyBackgroundWidget
    {
        public ConditionWindow (MenuReleaseHandler OnMenuRelease) : base (4, OnMenuRelease) {
            Show ();
        }
    }
}

