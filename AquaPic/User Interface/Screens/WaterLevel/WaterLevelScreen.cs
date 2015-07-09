using System;
using Gtk;
using MyWidgetLibrary;
using AquaPic.Runtime;
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    public class WaterLevelWindow : WindowBase
    {
        uint timerId;
        TouchTextBox tb;

        public WaterLevelWindow (params object[] options) : base () {
            MyBox box1 = new MyBox (385, 395);
            Put (box1, 10, 30);
            box1.Show ();

            MyBox box2 = new MyBox (385, 195);
            Put (box2, 405, 30);
            box2.Show ();

            MyBox box3 = new MyBox (385, 195);
            Put (box3, 405, 230);
            box3.Show ();

            var label = new TouchLabel ();
            label.text = "Auto Top Off";
            label.WidthRequest = 370;
            label.textColor = "pri";
            label.textSize = 12;
            Put (label, 15, 40);
            label.Show ();

            label = new TouchLabel ();
            label.text = "Water Level Sensor";
            label.textColor = "pri";
            label.textSize = 12;
            Put (label, 413, 40);
            label.Show ();

            label = new TouchLabel ();
            label.text = "Water Level";
            label.textColor = "grey4"; 
            Put (label, 410, 74);
            label.Show ();

            tb = new TouchTextBox ();
            tb.text = WaterLevel.analogWaterLevel.ToString ("F2");
            tb.WidthRequest = 200;
            Put (tb, 585, 70);

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Settings";
            settingsBtn.SetSizeRequest (100, 30);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new LevelSettings ();
                s.Run ();
                s.Destroy ();
            };
            Put (settingsBtn, 410, 190);
            settingsBtn.Show ();

            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 30);
            Put (b, 515, 190);

            timerId = GLib.Timeout.Add (1000, OnUpdateTimer);

            ShowAll ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);

            base.Dispose ();
        }

        public bool OnUpdateTimer () {
            tb.text = WaterLevel.analogWaterLevel.ToString ("F2");
            tb.QueueDraw ();

            return true;
        }
    }
}

