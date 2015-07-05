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
            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 40);
            Put (b, 15, 35);

            tb = new TouchTextBox ();
            tb.text = WaterLevel.waterLevel.ToString ("F2");
            tb.WidthRequest = 200;
            Put (tb, 15, 85);

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Settings";
            settingsBtn.SetSizeRequest (100, 30);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new LevelSettings ();
                s.Run ();
                s.Destroy ();
            };
            Put (settingsBtn, 15, 390);
            settingsBtn.Show ();

            timerId = GLib.Timeout.Add (1000, OnUpdateTimer);

            ShowAll ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);

            base.Dispose ();
        }

        public bool OnUpdateTimer () {
            tb.text = WaterLevel.waterLevel.ToString ("F2");
            tb.QueueDraw ();

            return true;
        }
    }
}

