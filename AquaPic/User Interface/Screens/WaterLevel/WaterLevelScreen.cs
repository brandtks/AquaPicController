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
        TouchComboBox switchCombo;
        int switchId;

        public WaterLevelWindow (params object[] options) : base () {
            MyBox box1 = new MyBox (385, 395);
            Put (box1, 10, 30);
            box1.Show ();

            MyBox box2 = new MyBox (385, 193);
            Put (box2, 405, 30);
            box2.Show ();

            MyBox box3 = new MyBox (385, 192);
            Put (box3, 405, 233);
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
            Put (settingsBtn, 410, 188);
            settingsBtn.Show ();

            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 30);
            Put (b, 515, 188);

            switchId = 0;

            label = new TouchLabel ();
            label.text = "Probes";
            label.textColor = "pri";
            label.textSize = 12;
            Put (label, 413, 243);
            label.Show ();

            var switchSetupBtn = new TouchButton ();
            switchSetupBtn.text = "Probe Setup";
            switchSetupBtn.SetSizeRequest (100, 30);
            switchSetupBtn.ButtonReleaseEvent += (o, args) => {
                string name = WaterLevel.GetFloatSwitchName (switchId);
                var s = new SwitchSettings (name, switchId, true);
                s.Run ();
                s.Destroy ();

                try {
                    WaterLevel.GetFloatSwitchIndex (name);
                } catch (ArgumentException) {
                    switchCombo.List.Remove (name);
                    switchId = 0;
                    switchCombo.Active = switchId;
                }

                switchCombo.QueueDraw ();
            };
            Put (switchSetupBtn, 410, 390);
            switchSetupBtn.Show ();

            string[] sNames = WaterLevel.GetAllFloatSwitches ();
            switchCombo = new TouchComboBox (sNames);
            switchCombo.Active = switchId;
            switchCombo.WidthRequest = 235;
            switchCombo.List.Add ("New switch...");
            switchCombo.ChangedEvent += OnSwitchComboChanged;
            Put (switchCombo, 550, 238);
            switchCombo.Show ();

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

        public void OnSwitchComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New switch...") {
                int switchCount = WaterLevel.GetFloatSwitchCount ();

                var s = new SwitchSettings ("New switch", -1, false);
                s.Run ();
                s.Destroy ();

                if (WaterLevel.GetFloatSwitchCount () > switchCount) {
                    switchId = WaterLevel.GetFloatSwitchCount () - 1;
                    int listIdx = switchCombo.List.IndexOf ("New switch...");
                    switchCombo.List.Insert (listIdx, WaterLevel.GetFloatSwitchName (switchId));
                    switchCombo.Active = listIdx;
                    switchCombo.QueueDraw ();
                } else
                    switchCombo.Active = switchId;
            } else {
                try {
                    int id = WaterLevel.GetFloatSwitchIndex (e.ActiveText);
                    switchId = id;
                } catch {
                    ;
                }
            }
        }
    }
}

