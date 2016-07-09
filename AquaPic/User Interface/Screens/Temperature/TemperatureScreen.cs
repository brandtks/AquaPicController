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
        
        int heaterId;
        TouchLabel heaterLabel;
        TouchComboBox heaterCombo;

        int probeId;
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
            label.text = "Temperature Groups";
            label.WidthRequest = 118;
            label.textColor = "seca";
            label.textSize = 12;
            label.textAlignment = TouchAlignment.Right;
            Put (label, 30, 70);
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
                s.Destroy ();

                // A new group was added
                if (Temperature.CheckTemperatureGroupKeyNoThrow (newGroupName)) {
                    groupName = newGroupName;
                    if (!groupCombo.comboList.Contains (groupName)) {
                        groupCombo.comboList.Insert (groupCombo.comboList.Count - 1, groupName);
                        groupCombo.activeText = groupName;
                    }
                }

                // The group was deleted
                if (!Temperature.CheckTemperatureGroupKeyNoThrow (groupName)) {
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
            if (Temperature.heaterCount == 0)
                heaterId = -1;
            else
                heaterId = 0;

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
                if (heaterId != -1) {
                    string name = Temperature.GetHeaterName (heaterId);
                    var s = new HeaterSettings (name, heaterId, true);
                    s.Run ();
                    s.Destroy ();

                    try {
                        Temperature.GetHeaterIndex (name);
                    } catch (ArgumentException) {
                        heaterCombo.comboList.Remove (name);
                        if (Temperature.heaterCount != 0) {
                            heaterId = 0;
                            heaterCombo.active = heaterId;
                        } else {
                            heaterId = -1;
                            heaterCombo.active = 0;
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
                        int listIdx = heaterCombo.comboList.IndexOf ("New heater...");
                        heaterCombo.comboList.Insert (listIdx, Temperature.GetHeaterName (heaterId));
                        heaterCombo.active = listIdx;
                        heaterCombo.QueueDraw ();
                        GetHeaterData ();
                    } else {
                        if (heaterId != -1)
                            heaterCombo.active = heaterId;
                        else
                            heaterCombo.active = 0;
                    }
                }

                heaterCombo.QueueDraw ();
            };
            Put (heaterSetupBtn, 415, 195);
            heaterSetupBtn.Show ();

            /******************************************************************************************************/
            /* Temperature Probes                                                                                 */
            /******************************************************************************************************/
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
                        probeCombo.comboList.Remove (name);
                        if (Temperature.temperatureProbeCount != 0) {
                            probeId = 0;
                            probeCombo.active = probeId;
                        } else {
                            probeId = -1;
                            probeCombo.active = 0;
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
                        int listIdx = probeCombo.comboList.IndexOf ("New probe...");
                        probeCombo.comboList.Insert (listIdx, Temperature.GetTemperatureProbeName (probeId));
                        probeCombo.active = listIdx;
                        probeCombo.QueueDraw ();
                        GetProbeData ();
                    } else {
                        if (probeId != -1)
                            probeCombo.active = probeId;
                        else
                            probeCombo.active = 0;
                    }
                }

                probeCombo.QueueDraw ();
            };
            Put (probeSetupBtn, 415, 405);
            probeSetupBtn.Show ();

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

            if (heaterId != -1)
                heaterCombo.active = heaterId;
            else
                heaterCombo.active = 0;

            probeCombo = new TouchComboBox (Temperature.GetAllTemperatureProbeNames ());
            probeCombo.WidthRequest = 235;
            probeCombo.comboList.Add ("New probe...");
            probeCombo.ChangedEvent += OnProbeComboChanged;
            Put (probeCombo, 550, 277);
            probeCombo.Show ();

            if (probeId != -1)
                probeCombo.active = probeId;
            else
                probeCombo.active = 0;

            groupCombo = new TouchComboBox (Temperature.GetAllTemperatureGroupNames ());
            groupCombo.WidthRequest = 235;
            groupCombo.comboList.Add ("New group...");
            groupCombo.ChangedEvent += OnGroupComboChanged;
            Put (groupCombo, 153, 77);
            groupCombo.Show ();

            if (!groupName.IsEmpty ()) {
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

                var s = new HeaterSettings ("New Heater", -1, false);
                s.Run ();
                s.Destroy ();

                if (Temperature.heaterCount > heaterCount) { // a heater was added
                    heaterId = Temperature.heaterCount - 1;
                    int listIdx = heaterCombo.comboList.IndexOf ("New heater...");
                    heaterCombo.comboList.Insert (listIdx, Temperature.GetHeaterName (heaterId));
                    heaterCombo.active = listIdx;
                    heaterCombo.QueueDraw ();
                    GetHeaterData ();
                } else {
                    if (heaterId != -1)
                        heaterCombo.active = heaterId;
                    else
                        heaterCombo.active = 0;
                }
            } else {
                heaterId = Temperature.GetHeaterIndex (e.ActiveText);
            }

            GetHeaterData ();
        }

        protected void OnProbeComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New probe...") {
                int probeCount = Temperature.temperatureProbeCount;

                var s = new ProbeSettings ("New probe", -1, false);
                s.Run ();
                s.Destroy ();

                if (Temperature.temperatureProbeCount > probeCount) {
                    probeId = Temperature.temperatureProbeCount - 1;
                    int listIdx = probeCombo.comboList.IndexOf ("New probe...");
                    probeCombo.comboList.Insert (listIdx, Temperature.GetTemperatureProbeName (probeId));
                    probeCombo.active = listIdx;
                    probeCombo.QueueDraw ();
                    GetProbeData ();
                } else {
                    if (probeId != -1)
                        probeCombo.active = probeId;
                    else
                        probeCombo.active = 0;
                }
            } else {
                probeId = Temperature.GetTemperatureProbeIndex (e.ActiveText);
            }

            GetProbeData ();
        }

        protected void OnGroupComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New group...") {
                var s = new TemperatureGroupSettings (string.Empty, false);
                s.Run ();
                string newGroupName = s.temperatureGroupName;
                s.Destroy ();

                if (Temperature.CheckTemperatureGroupKeyNoThrow (newGroupName)) {
                    groupName = newGroupName;
                    groupCombo.comboList.Insert (groupCombo.comboList.Count - 1, groupName);
                    groupCombo.activeText = groupName;
                }
            } else {
                groupName = e.ActiveText;
            }

            groupCombo.QueueDraw ();
            GetGroupData ();
        }

        protected void GetHeaterData () {
            if (heaterId != -1) {
                if (Power.GetOutletState (Temperature.GetHeaterIndividualControl (heaterId)) == MyState.On) {
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
            if (probeId != -1) {
                if (Temperature.IsTemperatureProbeConnected (probeId)) {
                    probeTempTextbox.text = Temperature.GetTemperatureProbeTemperature (probeId).ToString ("F2");
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

