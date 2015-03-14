using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.LightingModule;

namespace AquaPic
{
    public partial class LightingWindow : MyBackgroundWidget
    {
        public LightingWindow (ButtonReleaseEventHandler OnTouchButtonRelease) : base ("Lighting", OnTouchButtonRelease) {
            TouchSelectorSwitch ss = new TouchSelectorSwitch (0, 3, 1, MyOrientation.Vertical);
            ss.AddSelectedColorOption (0, 0.50, 0.25, 0.25);
            ss.AddSelectedColorOption (1, 0.25, 0.25, 0.50);
            ss.AddSelectedColorOption (2, 0.40, 0.40, 0.20);
            Put (ss, 50, 50);
            ss.Show ();

            TouchLabel l1 = new TouchLabel ();
            l1.Text = "Auto Auto";
            l1.FontColor = new MyColor (1.0, 0.75, 0.75);
            Put (l1, 75, 50);
            l1.Show ();

            TouchLabel l2 = new TouchLabel ();
            l2.Text = "Auto";
            l2.FontColor = new MyColor (0.75, 0.75, 1.0);
            Put (l2, 75, 80);
            l2.Show ();

            TouchLabel l3 = new TouchLabel ();
            l3.Text = "Manual";
            l3.FontColor = new MyColor (1.0, 1.0, 0.50);
            Put (l3, 75, 110);
            l3.Show ();

            TouchTextBox sunRise = new TouchTextBox ();
            sunRise.WidthRequest = 200;
            sunRise.Text = Lighting.SunRiseToday.ToString ();
            Put (sunRise, 300, 50);
            sunRise.Show ();

            TouchTextBox sunSet = new TouchTextBox ();
            sunSet.WidthRequest = 200;
            sunSet.Text = Lighting.SunSetToday.ToString ();
            Put (sunSet, 300, 95);
            sunSet.Show ();

            Show ();
        }
    }
}

