using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    public class TemperatureWindow : WindowBase
    {
        TouchComboBox heaterCombo;
        TouchComboBox probeCombo;
        TouchLabel heaterLabel;
        TouchTextBox probeTempTextbox;
        TouchTextBox tempTextBox;
        int heaterId;
        int probeId;
        uint timerId;

//        SettingTextBox setpoint;
//        SettingTextBox deadband;

        public TemperatureWindow (params object[] options) : base () {
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
            label.text = "General Temperature Information";
            label.WidthRequest = 370;
            label.textColor = "pri";
            label.textSize = 12;
            Put (label, 15, 40);
            label.Show ();

            var tempLabel = new TouchLabel ();
            tempLabel.text = "Temperature";
            tempLabel.textColor = "grey4"; 
            tempLabel.textSize = 14;
            tempLabel.WidthRequest = 170;
            tempLabel.render.alignment = MyAlignment.Right;
            Put (tempLabel, 15, 75);
            tempLabel.Show ();

            tempTextBox = new TouchTextBox ();
            tempTextBox.WidthRequest = 200;
            tempTextBox.HeightRequest = 35;
            tempTextBox.textSize = 14;
            tempTextBox.text = Temperature.WaterTemperature.ToString ("F1");
            Put (tempTextBox, 190, 70);
            tempTextBox.Show ();

            var setpointlabel = new TouchLabel ();
            setpointlabel.text = "Setpoint";
            setpointlabel.textColor = "grey4"; 
            setpointlabel.WidthRequest = 170;
            setpointlabel.render.alignment = MyAlignment.Right;
            Put (setpointlabel, 15, 114);
            setpointlabel.Show ();

            var tempSetpoint = new TouchTextBox ();
            tempSetpoint.WidthRequest = 200;
            tempSetpoint.text = Temperature.temperatureSetpoint.ToString ("F1");
            Put (tempSetpoint, 190, 110);
            tempSetpoint.Show ();

            var tempDeadbandLabel = new TouchLabel ();
            tempDeadbandLabel.text = "Deadband";
            tempDeadbandLabel.textColor = "grey4";
            tempDeadbandLabel.WidthRequest = 170;
            tempDeadbandLabel.render.alignment = MyAlignment.Right;
            Put (tempDeadbandLabel, 15, 149);
            tempDeadbandLabel.Show ();

            var tempDeadband = new TouchTextBox ();
            tempDeadband.WidthRequest = 200;
            tempDeadband.text = (Temperature.temperatureDeadband * 2).ToString ("F1");
            Put (tempDeadband, 190, 145);
            tempDeadband.Show ();

            heaterId = 0;

            heaterLabel = new TouchLabel ();
            heaterLabel.textAlignment = MyAlignment.Center;
            heaterLabel.WidthRequest = 375;
            heaterLabel.textColor = "secb";
            Put (heaterLabel, 410, 77);
            heaterLabel.Show ();

//            setpoint = new SettingTextBox ();
//            setpoint.label.text = "Setpoint";
//            setpoint.textBox.enableTouch = false;
//            Put (setpoint, 410, 100);
//
//            deadband = new SettingTextBox ();
//            deadband.label.text = "Deadband";
//            deadband.textBox.enableTouch = false;
//            Put (deadband, 410, 135);

            var heaterSetupBtn = new TouchButton ();
            heaterSetupBtn.text = "Heater Setup";
            heaterSetupBtn.SetSizeRequest (100, 30);
            heaterSetupBtn.ButtonReleaseEvent += (o, args) => {
                string name = Temperature.GetHeaterName (heaterId);
                var s = new HeaterSettings (name, heaterId, true);
                s.Run ();
                s.Destroy ();

                try {
                    Temperature.GetHeaterIndex (name);
                } catch (ArgumentException) {
                    heaterCombo.List.Remove (name);
                    heaterId = 0;
                    heaterCombo.Active = heaterId;
                    GetHeaterData ();
                }
            };
            Put (heaterSetupBtn, 410, 190);
            heaterSetupBtn.Show ();

            var globalSettingsBtn = new TouchButton ();
            globalSettingsBtn.text = "Settings";
            globalSettingsBtn.SetSizeRequest (100, 30);
            globalSettingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new TemperatureSettings ();
                s.Run ();
                s.Destroy ();
                tempSetpoint.text = Temperature.temperatureSetpoint.ToString ("F1");
                tempDeadband.text = (Temperature.temperatureDeadband * 2).ToString ("F1");
            };
            Put (globalSettingsBtn, 15, 390);
            globalSettingsBtn.Show ();

            var probeSetupBtn = new TouchButton ();
            probeSetupBtn.text = "Probe Setup";
            probeSetupBtn.SetSizeRequest (100, 30);
            probeSetupBtn.ButtonReleaseEvent += (o, args) => {
                string name = Temperature.GetTemperatureProbeName (probeId);
                var s = new ProbeSettings (name, probeId, true);
                s.Run ();
                s.Destroy ();

                try {
                    Temperature.GetTemperatureProbeIndex (name);
                } catch (ArgumentException) {
                    probeCombo.List.Remove (name);
                    probeId = 0;
                    probeCombo.Active = heaterId;
                    GetProbeData ();
                }
            };
            Put (probeSetupBtn, 410, 390);
            probeSetupBtn.Show ();

            var tLabel = new TouchLabel ();
            tLabel.text = "Temperature";
            tLabel.textAlignment = MyAlignment.Right;
            tLabel.WidthRequest = 100;
            Put (tLabel, 410, 304);
            tLabel.Show ();

            probeTempTextbox = new TouchTextBox ();
            probeTempTextbox.WidthRequest = 200;
            Put (probeTempTextbox, 515, 300);
            probeTempTextbox.Show ();

            string[] hNames = Temperature.GetAllHeaterNames ();
            heaterCombo = new TouchComboBox (hNames);
            heaterCombo.Active = heaterId;
            heaterCombo.WidthRequest = 235;
            heaterCombo.List.Add ("New heater...");
            heaterCombo.ChangedEvent += OnHeaterComboChanged;
            Put (heaterCombo, 550, 35);
            heaterCombo.Show ();

            string[] pNames = Temperature.GetAllTemperatureProbeNames ();
            probeCombo = new TouchComboBox (pNames);
            probeCombo.Active = probeId;
            probeCombo.WidthRequest = 235;
            probeCombo.List.Add ("New probe...");
            probeCombo.ChangedEvent += OnProbeComboChanged;
            Put (probeCombo, 550, 235);
            probeCombo.Show ();

            GetHeaterData ();
            GetProbeData ();

            timerId = GLib.Timeout.Add (1000, OnTimer);

            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);

            base.Dispose ();
        }

        protected void OnHeaterComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New heater...") {
                var s = new HeaterSettings ("New Heater", -1, false);
                s.Run ();
                s.Destroy ();

                heaterId = Temperature.GetHeaterCount () - 1;
                int listIdx = heaterCombo.List.IndexOf ("New heater...");
                heaterCombo.List.Insert (listIdx, Temperature.GetHeaterName (heaterId));
                heaterCombo.Active = listIdx;
                heaterCombo.QueueDraw ();
                GetHeaterData ();
            } else {
                try {
                    int id = Temperature.GetHeaterIndex (e.ActiveText);
                    heaterId = id;
                    GetHeaterData ();
                } catch {
                    ;
                }
            }
        }

        protected void OnProbeComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New probe...") {
                var s = new ProbeSettings ("New probe", -1, false);
                s.Run ();
                s.Destroy ();

                probeId = Temperature.GetTemperatureProbeCount () - 1;
                int listIdx = probeCombo.List.IndexOf ("New probe...");
                probeCombo.List.Insert (listIdx, Temperature.GetTemperatureProbeName (probeId));
                probeCombo.Active = listIdx;
                probeCombo.QueueDraw ();
                GetProbeData ();
            } else {
                try {
                    int id = Temperature.GetTemperatureProbeIndex (e.ActiveText);
                    probeId = id;
                    GetProbeData ();
                } catch {
                    ;
                }
            }
        }

        protected void GetHeaterData () {
//            if (Temperature.ControlsTemperature (heaterId)) {
                heaterLabel.text = string.Format ("{0} control based upon global setpoints", Temperature.GetHeaterName (heaterId));
//                setpoint.Visible = false;
//                deadband.Visible = false;
//            } else {
//                heaterLabel.text = "Heater Control Setpoints";
//                setpoint.Visible = true;
//                deadband.Visible = true;
//
//                setpoint.textBox.text = Temperature.GetHeaterSetpoint (heaterId).ToString ("F1");
//                deadband.textBox.text = Temperature.GetHeaterDeadband (heaterId).ToString ("F1");
//            }

            heaterLabel.QueueDraw ();
//            setpoint.QueueDraw ();
//            deadband.QueueDraw ();
        }

        protected void GetProbeData () {
            probeTempTextbox.text = Temperature.GetTemperatureProbeTemperature (probeId).ToString ("F2");
            probeTempTextbox.QueueDraw ();
        }

        protected bool OnTimer () {
            GetProbeData ();
            tempTextBox.text = Temperature.WaterTemperature.ToString ("F1");
            tempTextBox.QueueDraw ();

            return true;
        }
    }
}

