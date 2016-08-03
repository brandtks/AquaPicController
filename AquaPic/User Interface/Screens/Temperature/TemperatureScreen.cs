using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Drivers;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class TemperatureWindow : WindowBase
    {
        string groupName;
        TouchLabel tempTextBox;
        TouchLabel tempSetpoint;
        TouchLabel tempDeadband;
        TouchComboBox groupCombo;
        
        string heaterName;
        TouchLabel heaterLabel;
        TouchComboBox heaterCombo;

        string probeName;
        TouchLabel probeTempTextbox;
        TouchComboBox probeCombo;
        
        uint timerId;

        public TemperatureWindow (params object[] options) 
            : base () 
        {
            screenTitle = "Temperature";

            ExposeEvent += OnExpose;

            /******************************************************************************************************/
            /* Temperature Groups                                                                                 */
            /******************************************************************************************************/
            groupName = Temperature.defaultTemperatureGroup;

            var label = new TouchLabel ();
            label.text = "Groups";
            label.WidthRequest = 118;
            label.textColor = "seca";
            label.textSize = 12;
            label.textAlignment = TouchAlignment.Left;
            Put (label, 30, 80);
            label.Show ();

            var tempLabel = new TouchLabel ();
            tempLabel.WidthRequest = 329;
            tempLabel.text = "Temperature";
            tempLabel.textColor = "grey3";
            tempLabel.textAlignment = TouchAlignment.Center;
            Put (tempLabel, 60, 185);
            tempLabel.Show ();

            tempTextBox = new TouchLabel ();
            tempTextBox.SetSizeRequest (329, 50);
            tempTextBox.textSize = 36;
            tempTextBox.textAlignment = TouchAlignment.Center;
            tempTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Degrees;
            Put (tempTextBox, 60, 130);
            tempTextBox.Show ();

            var setpointlabel = new TouchLabel ();
            setpointlabel.WidthRequest = 116;
            setpointlabel.text = "Setpoint";
            setpointlabel.textColor = "grey3"; 
            setpointlabel.textAlignment = TouchAlignment.Center;
            Put (setpointlabel, 108, 260);
            setpointlabel.Show ();

            tempSetpoint = new TouchLabel ();
            tempSetpoint.SetSizeRequest (116, 30);
            tempSetpoint.textSize = 20;
            tempSetpoint.textAlignment = TouchAlignment.Center;
            tempSetpoint.textRender.unitOfMeasurement = UnitsOfMeasurement.Degrees;
            Put (tempSetpoint, 108, 225);
            tempSetpoint.Show ();

            var tempDeadbandLabel = new TouchLabel ();
            tempDeadbandLabel.WidthRequest = 116;
            tempDeadbandLabel.text = "Deadband";
            tempDeadbandLabel.textColor = "grey3";
            tempDeadbandLabel.textAlignment = TouchAlignment.Center;
            Put (tempDeadbandLabel, 224, 260);
            tempDeadbandLabel.Show ();

            tempDeadband = new TouchLabel ();
            tempDeadband.SetSizeRequest (116, 30);
            tempDeadband.textSize = 20;
            tempDeadband.textAlignment = TouchAlignment.Center;
            tempDeadband.textRender.unitOfMeasurement = UnitsOfMeasurement.Degrees;
            Put (tempDeadband, 224, 225);
            tempDeadband.Show ();

            var globalSettingsBtn = new TouchButton ();
            globalSettingsBtn.text = "Settings";
            globalSettingsBtn.SetSizeRequest (100, 60);
            globalSettingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new TemperatureGroupSettings (groupName, !groupName.IsEmpty ());
                s.Run ();
                var newGroupName = s.temperatureGroupName;
                var outcome = s.outcome;
                s.Destroy ();

                if (outcome == TouchSettingsOutcome.Added) {
                    groupName = newGroupName;
                    groupCombo.comboList.Insert (groupCombo.comboList.Count - 1, groupName);
                    groupCombo.activeText = groupName;
                } else if (outcome == TouchSettingsOutcome.Deleted) {
                    groupCombo.comboList.Remove (groupName);
                    groupName = Temperature.defaultTemperatureGroup;
                    groupCombo.activeText = groupName;
                }

                groupCombo.QueueDraw ();
                GetGroupData ();
            };
            Put (globalSettingsBtn, 290, 405);
            globalSettingsBtn.Show ();

            /******************************************************************************************************/
            /* Heaters                                                                                            */
            /******************************************************************************************************/
            heaterName = Temperature.defaultHeater;

            label = new TouchLabel ();
            label.text = "Heaters";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 415, 80);
            label.Show ();

            heaterLabel = new TouchLabel ();
            heaterLabel.textAlignment = TouchAlignment.Center;
            heaterLabel.WidthRequest = 370;
            heaterLabel.textColor = "secb";
            heaterLabel.textSize = 20;
            Put (heaterLabel, 415, 120);
            heaterLabel.Show ();

            var heaterSetupBtn = new TouchButton ();
            heaterSetupBtn.text = "Heater Setup";
            heaterSetupBtn.SetSizeRequest (100, 60);
            heaterSetupBtn.ButtonReleaseEvent += (o, args) => {
                var s = new HeaterSettings (heaterName, heaterName.IsNotEmpty ());
                s.Run ();
                var newHeaterName = s.newOrUpdatedHeaterName;
                var outcome = s.outcome;
                s.Destroy ();

                if ((outcome == TouchSettingsOutcome.Modified) && (newHeaterName != heaterName)) {
                    var index = heaterCombo.comboList.IndexOf (heaterName);
                    heaterCombo.comboList[index] = newHeaterName;
                    heaterName = newHeaterName;
                } else if (outcome == TouchSettingsOutcome.Added) {
                    heaterCombo.comboList.Insert (heaterCombo.comboList.Count - 1, newHeaterName);
                    heaterCombo.activeText = newHeaterName;
                    heaterName = newHeaterName;
                } else if (outcome == TouchSettingsOutcome.Deleted) {
                    heaterCombo.comboList.Remove (heaterName);
                    heaterName = Temperature.defaultHeater;
                    heaterCombo.activeText = heaterName;
                }

                heaterCombo.QueueDraw ();
                GetHeaterData ();
            };
            Put (heaterSetupBtn, 415, 195);
            heaterSetupBtn.Show ();

            /******************************************************************************************************/
            /* Temperature Probes                                                                                 */
            /******************************************************************************************************/
            probeName = Temperature.defaultTemperatureProbe;

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
                var s = new ProbeSettings (probeName, probeName.IsNotEmpty ());
                s.Run ();
                var newProbeName = s.newOrUpdatedProbeName;
                var outcome = s.outcome;
                s.Destroy ();

                if ((outcome == TouchSettingsOutcome.Modified) && (newProbeName != probeName)) {
                    var index = probeCombo.comboList.IndexOf (probeName);
                    probeCombo.comboList[index] = newProbeName;
                    probeName = newProbeName;
                } else if (outcome == TouchSettingsOutcome.Added) {
                    probeCombo.comboList.Insert (probeCombo.comboList.Count - 1, newProbeName);
                    probeCombo.activeText = newProbeName;
                    probeName = newProbeName;
                } else if (outcome == TouchSettingsOutcome.Deleted) {
                    probeCombo.comboList.Remove (probeName);
                    probeName = Temperature.defaultTemperatureProbe;
                    probeCombo.activeText = probeName;
                }

                probeCombo.QueueDraw ();
                GetProbeData ();
            };
            Put (probeSetupBtn, 415, 405);
            probeSetupBtn.Show ();

            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 60);
            b.ButtonReleaseEvent += (o, args) => {
                if (probeName.IsNotEmpty ()) {
                    var cal = new CalibrationDialog (
                        probeName + " Probe",
                        () => {
                            return AquaPicDrivers.AnalogInput.GetChannelValue (Temperature.GetTemperatureProbeIndividualControl (probeName));
                        },
                        CalibrationState.ZeroActual);

                    cal.CalibrationCompleteEvent += (aa) => {
                        bool success = Temperature.SetTemperatureProbeCalibrationData (
                            probeName,
                            (float)aa.zeroActual,
                            (float)aa.zeroValue,
                            (float)aa.fullScaleActual,
                            (float)aa.fullScaleValue);

                        if (!success) {
                            MessageBox.Show ("Something went wrong");
                        }
                    };

                    cal.calArgs.zeroActual = Temperature.GetTemperatureProbeZeroActual (probeName);
                    cal.calArgs.zeroValue = Temperature.GetTemperatureProbeZeroValue (probeName);
                    cal.calArgs.fullScaleActual = Temperature.GetTemperatureProbeFullScaleActual (probeName);
                    cal.calArgs.fullScaleValue = Temperature.GetTemperatureProbeFullScaleValue (probeName);

                    cal.Run ();
                    cal.Destroy ();
                } else {
                    MessageBox.Show ("No probe selected\n" +
                                    "Can't perfom a calibration");
                }
            };
            Put (b, 525, 405);

            var tLabel = new TouchLabel ();
            tLabel.text = "Temperature";
            tLabel.textAlignment = TouchAlignment.Center;
            tLabel.textColor = "grey3";
            tLabel.WidthRequest = 370;
            Put (tLabel, 415, 355);
            tLabel.Show ();

            probeTempTextbox = new TouchLabel ();
            probeTempTextbox.WidthRequest = 370;
            probeTempTextbox.textSize = 20;
            probeTempTextbox.textAlignment = TouchAlignment.Center;
            Put (probeTempTextbox, 415, 320);
            probeTempTextbox.Show ();

            heaterCombo = new TouchComboBox (Temperature.GetAllHeaterNames ()); 
            heaterCombo.WidthRequest = 235;
            heaterCombo.comboList.Add ("New heater...");
            heaterCombo.ChangedEvent += OnHeaterComboChanged;
            Put (heaterCombo, 550, 77);
            heaterCombo.Show ();

            if (heaterName.IsNotEmpty ())
                heaterCombo.activeText = heaterName;
            else
                heaterCombo.active = 0;

            probeCombo = new TouchComboBox (Temperature.GetAllTemperatureProbeNames ());
            probeCombo.WidthRequest = 235;
            probeCombo.comboList.Add ("New probe...");
            probeCombo.ChangedEvent += OnProbeComboChanged;
            Put (probeCombo, 550, 277);
            probeCombo.Show ();

            if (probeName.IsNotEmpty ())
                probeCombo.activeText = probeName;
            else
                probeCombo.active = 0;

            groupCombo = new TouchComboBox (Temperature.GetAllTemperatureGroupNames ());
            groupCombo.WidthRequest = 235;
            groupCombo.comboList.Add ("New group...");
            groupCombo.ChangedEvent += OnGroupComboChanged;
            Put (groupCombo, 153, 77);
            groupCombo.Show ();

            if (groupName.IsNotEmpty ()) {
                groupCombo.activeText = groupName;
            } else {
                groupCombo.active = 0;
            }

            GetHeaterData ();
            GetProbeData ();
            GetGroupData ();

            timerId = GLib.Timeout.Add (1000, OnTimer);

            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
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
        }

        protected void OnHeaterComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New heater...") {
                int heaterCount = Temperature.heaterCount;

                var s = new HeaterSettings (string.Empty, false);
                s.Run ();
                var newHeaterName = s.newOrUpdatedHeaterName;
                var outcome = s.outcome;
                s.Destroy ();

                if (outcome == TouchSettingsOutcome.Added) { 
                    heaterCombo.comboList.Insert (heaterCombo.comboList.Count - 1, newHeaterName);
                    heaterCombo.activeText = newHeaterName;
                    heaterName = newHeaterName;
                } else {
                    heaterCombo.activeText = heaterName;
                }
            } else {
                heaterName = e.ActiveText;
            }

            heaterCombo.QueueDraw ();
            GetHeaterData ();
        }

        protected void OnProbeComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New probe...") {
                var s = new ProbeSettings (string.Empty, false);
                s.Run ();
                var newProbeName = s.newOrUpdatedProbeName;
                var outcome = s.outcome;
                s.Destroy ();

                if (outcome == TouchSettingsOutcome.Added) {
                    probeCombo.comboList.Insert (probeCombo.comboList.Count - 1, newProbeName);
                    probeCombo.activeText = newProbeName;
                    probeName = newProbeName;
                } else {
                    probeCombo.activeText = probeName;
                }
            } else {
                probeName = e.ActiveText;
            }

            probeCombo.QueueDraw ();
            GetProbeData ();
        }

        protected void OnGroupComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New group...") {
                var s = new TemperatureGroupSettings (string.Empty, false);
                s.Run ();
                string newGroupName = s.temperatureGroupName;
                var outcome = s.outcome;
                s.Destroy ();

                if (outcome == TouchSettingsOutcome.Added) {
                    groupCombo.comboList.Insert (groupCombo.comboList.Count - 1, newGroupName);
                    groupCombo.activeText = newGroupName;
                    groupName = newGroupName;
                } else {
                    groupCombo.activeText = groupName;
                }
            } else {
                groupName = e.ActiveText;
            }

            groupCombo.QueueDraw ();
            GetGroupData ();
        }

        protected void GetHeaterData () {
            if (heaterName.IsNotEmpty ()) {
                if (Power.GetOutletState (Temperature.GetHeaterIndividualControl (heaterName)) == MyState.On) {
                    heaterLabel.text = "Heater On";
                    heaterLabel.textColor = "secb";
                } else {
                    heaterLabel.text = "Heater Off";
                    heaterLabel.textColor = "white";
                }
            } else {
                heaterLabel.text = "No Heaters Connected";
                heaterLabel.textColor = "white";
            }

            heaterLabel.QueueDraw ();
        }

        protected void GetProbeData () {
            if (probeName.IsNotEmpty ()) {
                if (Temperature.IsTemperatureProbeConnected (probeName)) {
                    probeTempTextbox.text = Temperature.GetTemperatureProbeTemperature (probeName).ToString ("F2");
                    probeTempTextbox.textRender.unitOfMeasurement = UnitsOfMeasurement.Degrees;
                } else {
                    probeTempTextbox.text = "Probe disconnected";
                    probeTempTextbox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                }
            } else {
                probeTempTextbox.text = "Probe not available";
                probeTempTextbox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
            }

            probeTempTextbox.QueueDraw ();
        }

        protected void GetGroupData () {
            if (!groupName.IsEmpty ()) {
                tempTextBox.text = Temperature.GetTemperatureGroupTemperature (groupName).ToString ("F1");
                tempSetpoint.text = Temperature.GetTemperatureGroupTemperatureSetpoint (groupName).ToString ("F1");
                tempDeadband.text = Temperature.GetTemperatureGroupTemperatureDeadband (groupName).ToString ("F1");
            } else {
                tempTextBox.text = "--";
                tempSetpoint.text = "--";
                tempDeadband.text = "--";
            }

            tempTextBox.QueueDraw ();
            tempSetpoint.QueueDraw ();
            tempDeadband.QueueDraw ();
        }

        protected bool OnTimer () {
            GetGroupData ();
            GetProbeData ();
            GetHeaterData ();
            return true;
        }
    }
}

