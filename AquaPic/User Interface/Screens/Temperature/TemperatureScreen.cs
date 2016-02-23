using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
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
            //TouchGraphicalBox box1 = new TouchGraphicalBox (385, 395);
            //Put (box1, 10, 30);
            //box1.Show ();

            //TouchGraphicalBox box2 = new TouchGraphicalBox (385, 193);
            //Put (box2, 405, 30);
            //box2.Show ();

            //TouchGraphicalBox box3 = new TouchGraphicalBox (385, 192);
            //Put (box3, 405, 233);
            //box3.Show ();

            screenTitle = "Temperature";

            ExposeEvent += (o, args) => {
                using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                    TouchColor.SetSource (cr, "grey3", 0.75);
                    cr.LineWidth = 3;

                    cr.MoveTo (402.5, 70);
                    cr.LineTo (402.5, 460);
                    cr.ClosePath ();
                    cr.Stroke ();

                    cr.MoveTo (417.5, 267.5);
                    cr.LineTo (780, 267.5);
                    cr.ClosePath ();
                    cr.Stroke ();
                }
            };

            TouchLabel genInfoLabel = new TouchLabel ();
            genInfoLabel.text = "General Temperature Information";
            genInfoLabel.textAlignment = TouchAlignment.Center;
            genInfoLabel.WidthRequest = 342;
            genInfoLabel.textColor = "seca";
            genInfoLabel.textSize = 12;
            Put (genInfoLabel, 60, 80);
            genInfoLabel.Show ();

            var tempLabel = new TouchLabel ();
            tempLabel.text = "Temperature";
            tempLabel.textColor = "grey4"; 
            Put (tempLabel, 60, 124);
            tempLabel.Show ();

            tempTextBox = new TouchTextBox ();
            tempTextBox.WidthRequest = 155;
            tempTextBox.text = Temperature.WaterTemperature.ToString ("F1");
            Put (tempTextBox, 235, 120);
            tempTextBox.Show ();

            var setpointlabel = new TouchLabel ();
            setpointlabel.text = "Setpoint";
            setpointlabel.textColor = "grey4"; 
            Put (setpointlabel, 60, 159);
            setpointlabel.Show ();

            var tempSetpoint = new TouchTextBox ();
            tempSetpoint.WidthRequest = 155;
            tempSetpoint.text = Temperature.temperatureSetpoint.ToString ("F1");
            Put (tempSetpoint, 235, 155);
            tempSetpoint.Show ();

            var tempDeadbandLabel = new TouchLabel ();
            tempDeadbandLabel.text = "Deadband";
            tempDeadbandLabel.textColor = "grey4";
            Put (tempDeadbandLabel, 60, 194);
            tempDeadbandLabel.Show ();

            var tempDeadband = new TouchTextBox ();
            tempDeadband.WidthRequest = 155;
            tempDeadband.text = (Temperature.temperatureDeadband * 2).ToString ("F1");
            Put (tempDeadband, 235, 190);
            tempDeadband.Show ();

            if (Temperature.heaterCount == 0)
                heaterId = -1;
            else
                heaterId = 0;

            var label = new TouchLabel ();
            label.text = "Heaters";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 415, 80);
            label.Show ();

            heaterLabel = new TouchLabel ();
            heaterLabel.textAlignment = TouchAlignment.Center;
            heaterLabel.WidthRequest = 375;
            heaterLabel.textColor = "secb";
            Put (heaterLabel, 410, 120);
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
            globalSettingsBtn.SetSizeRequest (100, 60);
            globalSettingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new TemperatureSettings ();
                s.Run ();
                s.Destroy ();

                tempSetpoint.text = Temperature.temperatureSetpoint.ToString ("F1");
                tempSetpoint.QueueDraw ();
                tempDeadband.text = (Temperature.temperatureDeadband * 2).ToString ("F1");
                tempDeadband.QueueDraw ();
            };
            Put (globalSettingsBtn, 290, 405);
            globalSettingsBtn.Show ();

            var heaterSetupBtn = new TouchButton ();
            heaterSetupBtn.text = "Heater Setup";
            heaterSetupBtn.SetSizeRequest (100, 60);
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
            Put (heaterSetupBtn, 415, 195);
            heaterSetupBtn.Show ();

            if (Temperature.temperatureProbeCount == 0)
                probeId = -1;
            else
                probeId = 0;

            label = new TouchLabel ();
            label.text = "Probes";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 415, 280);
            label.Show ();

            var probeSetupBtn = new TouchButton ();
            probeSetupBtn.text = "Probe Setup";
            probeSetupBtn.SetSizeRequest (100, 60);
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
            Put (probeSetupBtn, 415, 405);
            probeSetupBtn.Show ();

            var tLabel = new TouchLabel ();
            tLabel.text = "Temperature";
            tLabel.textAlignment = TouchAlignment.Right;
            tLabel.WidthRequest = 95;
            Put (tLabel, 415, 324);
            tLabel.Show ();

            probeTempTextbox = new TouchTextBox ();
            probeTempTextbox.WidthRequest = 215;
            Put (probeTempTextbox, 560, 320);
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
            Put (heaterCombo, 550, 77);
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
            Put (probeCombo, 550, 277);
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
                probeTempTextbox.text = "Probe not available";
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

