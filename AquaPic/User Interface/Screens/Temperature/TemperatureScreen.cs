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
        int heaterId;
        int probeId;

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

            var temp = new TouchTextBox ();
            temp.WidthRequest = 200;
            temp.HeightRequest = 35;
            temp.textSize = 14;
            temp.text = Temperature.WaterTemperature.ToString ("F1");
            Put (temp, 190, 70);
            temp.Show ();

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

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Heater Setup";
            settingsBtn.SetSizeRequest (100, 30);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
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
                    GetProbeData ();
                }
            };
            Put (settingsBtn, 410, 190);
            settingsBtn.Show ();

            var settingsBtn2 = new TouchButton ();
            settingsBtn2.text = "Settings";
            settingsBtn2.SetSizeRequest (100, 30);
            settingsBtn2.ButtonReleaseEvent += (o, args) => {
                var s = new TemperatureSettings ();
                s.Run ();
                s.Destroy ();
                tempSetpoint.text = Temperature.temperatureSetpoint.ToString ("F1");
                tempDeadband.text = (Temperature.temperatureDeadband * 2).ToString ("F1");
            };
            Put (settingsBtn2, 15, 390);
            settingsBtn2.Show ();

            var settingsBtn3 = new TouchButton ();
            settingsBtn3.text = "Probe Setup";
            settingsBtn3.SetSizeRequest (100, 30);
            settingsBtn3.ButtonReleaseEvent += (o, args) => {
                var s = new ProbeSettings (probeId);
                s.Run ();
                s.Destroy ();
            };
            Put (settingsBtn3, 410, 390);
            settingsBtn3.Show ();

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
            probeCombo.ChangedEvent += OnHeaterComboChanged;
            Put (probeCombo, 550, 235);
            probeCombo.Show ();

            GetHeaterData ();

            Show ();
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
                GetHeaterData ();
            } else {
                int id = Temperature.GetHeaterIndex (e.ActiveText);
                if (id != -1) {
                    heaterId = id;
                    GetHeaterData ();
                }
            }
        }

        protected void OnProbeComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int id = Temperature.GetTemperatureProbeIndex (e.ActiveText);

            if (id != -1) {
                probeId = id;
                GetProbeData ();
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

        }
    }
}

