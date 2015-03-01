using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public partial class MainWindow : MyBackgroundWidget
    {
        public MainWindow (ButtonReleaseEventHandler OnTouchButtonRelease) : base ("Main", OnTouchButtonRelease) {
            SelectorSwitch ss = new SelectorSwitch (0, 3, 1, MyOrientation.Vertical);
            ss.AddSelectedColorOption (0, 0.50, 0.25, 0.25);
            ss.AddSelectedColorOption (1, 0.25, 0.25, 0.50);
            ss.AddSelectedColorOption (2, 0.40, 0.40, 0.20);
            Put (ss, 50, 50);
            ss.Show ();

            MyLabel l1 = new MyLabel ();
            l1.text = "Auto Auto";
            l1.color = "#FF4F4F";
            Put (l1, 75, 50);
            l1.Show ();

            MyLabel l2 = new MyLabel ();
            l2.text = "Auto";
            l2.color = "#5F5FFF";
            Put (l2, 75, 80);
            l2.Show ();

            MyLabel l3 = new MyLabel ();
            l3.text = "Manual";
            l3.color = "#FFFF0A";
            Put (l3, 75, 110);
            l3.Show ();

            Show ();
        }
    }
}

