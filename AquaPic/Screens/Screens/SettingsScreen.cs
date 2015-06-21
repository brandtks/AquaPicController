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

            TouchLabel l = new TouchLabel ();
            l.text = "Testing";
            l.WidthRequest = 50;
            l.textRender.textWrap = MyTextWrap.Shrink;
            l.textRender.orientation = MyOrientation.Vertical;
            Put (l, 100, 100);
            l.Show ();
        }
    }
}

