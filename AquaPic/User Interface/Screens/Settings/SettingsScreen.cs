using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class SettingsWindow : MyBackgroundWidget
    {
        public SettingsWindow (params object[] options) : base () {
            TouchProgressBar pb = new TouchProgressBar (MyOrientation.Horizontal);
            pb.enableTouch = true;
            Put (pb, 100, 150);
            pb.Show ();

            TouchLabel l = new TouchLabel ();
            l.text = "Testing";
            l.WidthRequest = 50;
            l.render.textWrap = MyTextWrap.Shrink;
            l.render.orientation = MyOrientation.Vertical;
            Put (l, 100, 100);
            l.Show ();

            ShowAll ();
        }
    }
}

