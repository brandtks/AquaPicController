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

            MyBox box2 = new MyBox (385, 193);
            Put (box2, 405, 30);
            box2.Show ();

            MyBox box3 = new MyBox (385, 192);
            Put (box3, 405, 233);
            box3.Show ();

            var label = new TouchLabel ();
            label.text = "General Temperature Information";
            label.WidthRequest = 370;
            label.textSize = 12;
            label.textColor = "pri";
            Put (label, 15, 40);
            label.Show ();

            var tempLabel = new TouchLabel ();
            tempLabel.text = "Temperature";
            tempLabel.textColor = "grey4"; 
            tempLabel.WidthRequest = 170;
            Put (tempLabel, 15, 74);
            tempLabel.Show ();

            tempTextBox = new TouchTextBox ();
            tempTextBox.WidthRequest = 200;
            tempTextBox.text = Temperature.WaterTemperature.ToString ("F1");
            Put (tempTextBox, 190, 70);
            tempTextBox.Show ();

            var setpointlabel = new TouchLabel ();
            setpointlabel.text = "Setpoint";
            setpointlabel.textColor = "grey4"; 
            setpointlabel.WidthRequest = 170;
            Put (setpointlabel, 15, 109);
            setpointlabel.Show ();

            var tempSetpoint = new TouchTextBox ();
            tempSetpoint.WidthRequest = 200;
            tempSetpoint.text = Temperature.temperatureSetpoint.ToString ("F1");
            Put (tempSetpoint, 190, 105);
            tempSetpoint.Show ();

            var tempDeadbandLabel = new TouchLabel ();
            tempDeadbandLabel.text = "Deadband";
            tempDeadbandLabel.textColor = "grey4";
            tempDeadbandLabel.WidthRequest = 170;
            Put (tempDeadbandLabel, 15, 144);
            tempDeadbandLabel.Show ();

            var tempDeadband = new TouchTextBox ();
            tempDeadband.WidthRequest = 200;
            tempDeadband.text = (Temperature.temperatureDeadband * 2).ToString ("F1");
            Put (tempDeadband, 190, 140);
            tempDeadband.Show ();

            if (Temperature.heaterCount == 0)
                heaterId = -1;
            else
                heaterId = 0;

            label = new TouchLabel ();
            label.text = "Heaters";
            label.textColor = "pri";
            label.textSize = 12;
            Put (label, 413, 40);
            label.Show ();

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

            var globalSettingsBtn = new TouchButton ();
            globalSettingsBtn.text = "Settings";
            globalSettingsBtn.SetSizeRequest (100, 30);
            globalSettingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new TemperatureSettings ();
                s.Run ();
                s.Destroy ();

                tempSetpoint.text = Temperature.temperatureSetpoint.ToString ("F1");
                tempSetpoint.QueueDraw ();
                tempDeadband.text = (Temperature.temperatureDeadband * 2).ToString ("F1");
                tempDeadband.QueueDraw ();
            };
            Put (globalSettingsBtn, 15, 390);
            globalSettingsBtn.Show ();

            var heaterSetupBtn = new TouchButton ();
            heaterSetupBtn.text = "Heater Setup";
            heaterSetupBtn.SetSizeRequest (100, 30);
            heaterSetupBtn.ButtonReleaseEvent += (o, args) => {
                if (heaterId != -1) {
                    string name = Temperature.GetHeaterName (heaterId);
                    var s = new HeaterSettings (name, heaterId, true);
                    s.Run ();
                    s.Destroy ();

                    try {
                        Temperature.GetHeaterIndex (name);
                    } catch (ArgumentException) {
                        heaterCombo.List.Remove (name);
                        if (Temperature.heaterCount != 0) {
                            heaterId = 0;
                            heaterCombo.Active = heaterId;
                        } else {
                            heaterId = -1;
                            heaterCombo.Active = 0;
                        }

                        GetHeaterData ();
                    }
                } else {
                    int heaterCount = Temperature.heaterCount;

                    var s = new HeaterSettings ("New Heater", -1, false);
                    s.Run ();
                    s.Destroy ();

                    if (Temperature.heaterCount > heaterCount) { // a heater was added
                        heaterId = Temperature.heaterCount - 1;
                        int listIdx = heaterCombo.List.IndexOf ("New heater...");
                        heaterCombo.List.Insert (listIdx, Temperature.GetHeaterName (heaterId));
                        heaterCombo.Active = listIdx;
                        heaterCombo.QueueDraw ();
                        GetHeaterData ();
                    } else {
                        if (heaterId != -1)
                            heaterCombo.Active = heaterId;
                        else
                            heaterCombo.Active = 0;
                    }
                }

                heaterCombo.QueueDraw ();
            };
            Put (heaterSetupBtn, 410, 188);
            heaterSetupBtn.Show ();

            if (Temperature.temperatureProbeCount == 0)
                probeId = -1;
            else
                probeId = 0;

            label = new TouchLabel ();
            label.text = "Probes";
            label.textColor = "pri";
            label.textSize = 12;
            Put (label, 413, 243);
            label.Show ();

            var probeSetupBtn = new TouchButton ();
            probeSetupBtn.text = "Probe Setup";
            probeSetupBtn.SetSizeRequest (100, 30);
            probeSetupBtn.ButtonReleaseEvent += (o, args) => {
                if (probeId != -1) {
                    string name = Temperature.GetTemperatureProbeName (probeId);
                    var s = new ProbeSettings (name, probeId, true);
                    s.Run ();
                    s.Destroy ();

                    try {
                        Temperature.GetTemperatureProbeIndex (name);
                    } catch (ArgumentException) {
                        probeCombo.List.Remove (name);
                        if (Temperature.temperatureProbeCount != 0) {
                            probeId = 0;
                            probeCombo.Active = probeId;
                        } else {
                            probeId = -1;
                            probeCombo.Active = 0;
                        }

                        GetProbeData ();
                    }
                } else {
                    int probeCount = Temperature.temperatureProbeCount;

                    var s = new ProbeSettings ("New probe", -1, false);
                    s.Run ();
                    s.Destroy ();

                    if (Temperature.temperatureProbeCount > probeCount) {
                        probeId = Temperature.temperatureProbeCount - 1;
                        int listIdx = probeCombo.List.IndexOf ("New probe...");
                        probeCombo.List.Insert (listIdx, Temperature.GetTemperatureProbeName (probeId));
                        probeCombo.Active = listIdx;
                        probeCombo.QueueDraw ();
                        GetProbeData ();
                    } else {
                        if (probeId != -1)
                            probeCombo.Active = probeId;
                        else
                            probeCombo.Active = 0;
                    }
                }

                probeCombo.QueueDraw ();
            };
            Put (probeSetupBtn, 410, 390);
            probeSetupBtn.Show ();

            var tLabel = new TouchLabel ();
            tLabel.text = "Temperature";
            tLabel.textAlignment = MyAlignment.Right;
            tLabel.WidthRequest = 100;
            Put (tLabel, 410, 282);
            tLabel.Show ();

            probeTempTextbox = new TouchTextBox ();
            probeTempTextbox.WidthRequest = 200;
            Put (probeTempTextbox, 585, 278);
            probeTempTextbox.Show ();

            string[] hNames = Temperature.GetAllHeaterNames ();
            heaterCombo = new TouchComboBox (hNames);
            if (heaterId != -1)
                heaterCombo.Active = heaterId;
            else
                heaterCombo.Active = 0;
            heaterCombo.WidthRequest = 235;
            heaterCombo.List.Add ("New heater...");
            heaterCombo.ChangedEvent += OnHeaterComboChanged;
            Put (heaterCombo, 550, 35);
            heaterCombo.Show ();

            string[] pNames = Temperature.GetAllTemperatureProbeNames ();
            probeCombo = new TouchComboBox (pNames);
            if (probeId != -1)
                probeCombo.Active = probeId;
            else
                probeCombo.Active = 0;
            probeCombo.WidthRequest = 235;
            probeCombo.List.Add ("New probe...");
            probeCombo.ChangedEvent += OnProbeComboChanged;
            Put (probeCombo, 550, 238);
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
                int heaterCount = Temperature.heaterCount;

                var s = new HeaterSettings ("New Heater", -1, false);
                s.Run ();
                s.Destroy ();

                if (Temperature.heaterCount > heaterCount) { // a heater was added
                    heaterId = Temperature.heaterCount - 1;
                    int listIdx = heaterCombo.List.IndexOf ("New heater...");
                    heaterCombo.List.Insert (listIdx, Temperature.GetHeaterName (heaterId));
                    heaterCombo.Active = listIdx;
                    heaterCombo.QueueDraw ();
                    GetHeaterData ();
                } else {
                    if (heaterId != -1)
                        heaterCombo.Active = heaterId;
                    else
                        heaterCombo.Active = 0;
                }
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
                int probeCount = Temperature.temperatureProbeCount;

                var s = new ProbeSettings ("New probe", -1, false);
                s.Run ();
                s.Destroy ();

                if (Temperature.temperatureProbeCount > probeCount) {
                    probeId = Temperature.temperatureProbeCount - 1;
                    int listIdx = probeCombo.List.IndexOf ("New probe...");
                    probeCombo.List.Insert (listIdx, Temperature.GetTemperatureProbeName (probeId));
                    probeCombo.Active = listIdx;
                    probeCombo.QueueDraw ();
                    GetProbeData ();
                } else {
                    if (probeId != -1)
                        probeCombo.Active = probeId;
                    else
                        probeCombo.Active = 0;
                }
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
            if (heaterId != -1) {
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


//            setpoint.QueueDraw ();
//            deadband.QueueDraw ();
            } else {
                heaterLabel.text = string.Empty;
            }

            heaterLabel.QueueDraw ();
        }

        protected void GetProbeData () {
            if (probeId != -1) {
                probeTempTextbox.text = Temperature.GetTemperatureProbeTemperature (probeId).ToString ("F2");
            } else {
                probeTempTextbox.text = "0.0";
            }

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

