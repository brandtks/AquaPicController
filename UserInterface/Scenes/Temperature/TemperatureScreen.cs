#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using Gtk;
using Cairo;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules.Temperature;
using AquaPic.Drivers;
using AquaPic.Globals;
using AquaPic.Sensors;
using AquaPic.Sensors.TemperatureProbe;

namespace AquaPic.UserInterface
{
    public class TemperatureWindow : SceneBase
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
        TouchLabel probeTempLabel;
        TouchComboBox probeCombo;

        public TemperatureWindow (params object[] options) {
            sceneTitle = "Temperature";

            ExposeEvent += OnExpose;

            /******************************************************************************************************/
            /* Temperature Groups                                                                                 */
            /******************************************************************************************************/
            groupName = Temperature.defaultTemperatureGroup;

            if (options.Length >= 3) {
                var requestedGroup = options[2] as string;
                if (requestedGroup != null) {
                    if (Temperature.TemperatureGroupNameExists (requestedGroup)) {
                        groupName = requestedGroup;
                    }
                }
            }

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
            globalSettingsBtn.text = Convert.ToChar (0x2699).ToString ();
            globalSettingsBtn.SetSizeRequest (30, 30);
            globalSettingsBtn.buttonColor = "pri";
            globalSettingsBtn.ButtonReleaseEvent += OnTemperatureGroupSettingsButtonReleaseEvent;
            Put (globalSettingsBtn, 358, 77);
            globalSettingsBtn.Show ();

            /******************************************************************************************************/
            /* Heaters                                                                                            */
            /******************************************************************************************************/
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
            heaterSetupBtn.text = Convert.ToChar (0x2699).ToString ();
            heaterSetupBtn.SetSizeRequest (30, 30);
            heaterSetupBtn.buttonColor = "pri";
            heaterSetupBtn.ButtonReleaseEvent += OnHeaterSettingsButtonReleaseEvent;
            Put (heaterSetupBtn, 755, 77);
            heaterSetupBtn.Show ();

            /******************************************************************************************************/
            /* Temperature Probes                                                                                 */
            /******************************************************************************************************/
            label = new TouchLabel ();
            label.text = "Probes";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 415, 280);
            label.Show ();

            var probeSetupBtn = new TouchButton ();
            probeSetupBtn.text = Convert.ToChar (0x2699).ToString ();
            probeSetupBtn.SetSizeRequest (30, 30);
            probeSetupBtn.buttonColor = "pri";
            probeSetupBtn.ButtonReleaseEvent += OnTemperatureProbeSettingsButtonReleaseEvent;
            Put (probeSetupBtn, 755, 277);
            probeSetupBtn.Show ();

            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 60);
            b.ButtonReleaseEvent += OnWaterLevelSensorCalibrationButtonReleaseEvent;
            Put (b, 415, 405);

            probeTempLabel = new TouchLabel ();
            probeTempLabel.text = "Temperature";
            probeTempLabel.textAlignment = TouchAlignment.Center;
            probeTempLabel.textColor = "grey3";
            probeTempLabel.WidthRequest = 370;
            Put (probeTempLabel, 415, 355);
            probeTempLabel.Show ();

            probeTempTextbox = new TouchLabel ();
            probeTempTextbox.WidthRequest = 370;
            probeTempTextbox.textSize = 20;
            probeTempTextbox.textAlignment = TouchAlignment.Center;
            Put (probeTempTextbox, 415, 320);
            probeTempTextbox.Show ();

            heaterCombo = new TouchComboBox ();
            if (groupName.IsNotEmpty ()) {
                var heaterNames = Temperature.GetAllHeaterNames (groupName);
                if (heaterNames.Length > 0) {
                    heaterCombo.comboList.AddRange (heaterNames);
                    heaterName = heaterNames[0];
                } else {
                    heaterName = string.Empty;
                }
            }
            heaterCombo.WidthRequest = 200;
            heaterCombo.comboList.Add ("New heater...");
            heaterCombo.ComboChangedEvent += OnHeaterComboChanged;
            Put (heaterCombo, 550, 77);
            heaterCombo.Show ();

            if (heaterName.IsNotEmpty ())
                heaterCombo.activeText = heaterName;
            else
                heaterCombo.activeIndex = 0;

            probeCombo = new TouchComboBox ();
            if (groupName.IsNotEmpty ()) {
                var groupsTemperatureProbes = Temperature.GetAllTemperatureProbesForTemperatureGroup (groupName);
                if (groupsTemperatureProbes.Length > 0) {
                    probeCombo.comboList.AddRange (groupsTemperatureProbes);
                    probeName = groupsTemperatureProbes[0];
                } else {
                    probeName = string.Empty;
                }
            }
            probeCombo.WidthRequest = 200;
            probeCombo.comboList.Add ("New probe...");
            probeCombo.ComboChangedEvent += OnProbeComboChanged;
            Put (probeCombo, 550, 277);
            probeCombo.Show ();

            if (probeName.IsNotEmpty ())
                probeCombo.activeText = probeName;
            else
                probeCombo.activeIndex = 0;

            groupCombo = new TouchComboBox (Temperature.GetAllTemperatureGroupNames ());
            groupCombo.WidthRequest = 200;
            groupCombo.comboList.Add ("New group...");
            groupCombo.ComboChangedEvent += OnGroupComboChanged;
            Put (groupCombo, 153, 77);
            groupCombo.Show ();

            if (groupName.IsNotEmpty ()) {
                groupCombo.activeText = groupName;
            } else {
                groupCombo.activeIndex = 0;
            }

            GetHeaterData ();
            GetProbeData ();
            GetGroupData ();

            Show ();
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
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

        protected void GetHeaterData () {
            if (heaterName.IsNotEmpty ()) {
                if (Power.GetOutletState (Temperature.GetHeaterIndividualControl (groupName, heaterName)) == MyState.On) {
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
                var probe = (TemperatureProbe)AquaPicSensors.TemperatureProbes.GetSensor (probeName);
                if (probe.connected) {
                    probeTempTextbox.text = probe.value.ToString ("F2");
                    probeTempTextbox.textRender.unitOfMeasurement = UnitsOfMeasurement.Degrees;
                    probeTempLabel.Visible = true;
                } else {
                    probeTempTextbox.text = "Probe disconnected";
                    probeTempTextbox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                    probeTempLabel.Visible = false;
                }
            } else {
                probeTempTextbox.text = "Probe not available";
                probeTempTextbox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                probeTempLabel.Visible = false;
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

        protected void OnGroupComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New group...") {
                CallTemperatureGroupSettingsDialog (true);
            } else {
                groupName = e.activeText;

                probeCombo.comboList.Clear ();
                if (groupName.IsNotEmpty ()) {
                    var groupsTemperatureProbes = Temperature.GetAllTemperatureProbesForTemperatureGroup (groupName);
                    probeCombo.comboList.AddRange (groupsTemperatureProbes);
                    if (groupsTemperatureProbes.Length > 0) {
                        probeName = groupsTemperatureProbes[0];
                    } else {
                        probeName = string.Empty;
                    }
                }
                probeCombo.comboList.Add ("New level sensor...");
                probeCombo.activeIndex = 0;
                probeCombo.QueueDraw ();

                heaterCombo.comboList.Clear ();
                if (groupName.IsNotEmpty ()) {
                    var heaterNames = Temperature.GetAllHeaterNames (groupName);
                    if (heaterNames.Length > 0) {
                        heaterCombo.comboList.AddRange (heaterNames);
                        heaterName = heaterNames[0];
                    } else {
                        heaterName = string.Empty;
                    }
                }
                heaterCombo.comboList.Add ("New heater...");
                heaterCombo.activeIndex = 0;
                heaterCombo.QueueDraw ();
            }

            groupCombo.QueueDraw ();
            GetGroupData ();
            GetProbeData ();
        }

        protected void OnTemperatureGroupSettingsButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            CallTemperatureGroupSettingsDialog ();
        }

        protected void CallTemperatureGroupSettingsDialog (bool forceNew = false) {
            TemperatureGroupSettings settings;
            if (groupName.IsNotEmpty () && !forceNew) {
                settings = Temperature.GetTemperatureGroupSettings (groupName);
            } else {
                settings = new TemperatureGroupSettings ();
            }
            var parent = Toplevel as Window;
            var s = new TemperatureGroupSettingsDialog (settings, parent);
            s.Run ();
            var newGroupName = s.groupName;
            var outcome = s.outcome;

            if ((outcome == TouchSettingsOutcome.Modified) && (newGroupName != groupName)) {
                var index = groupCombo.comboList.IndexOf (groupName);
                groupCombo.comboList[index] = newGroupName;
                groupName = newGroupName;
            } else if (outcome == TouchSettingsOutcome.Added) {
                groupCombo.comboList.Insert (groupCombo.comboList.Count - 1, newGroupName);
                groupCombo.activeText = newGroupName;
                groupName = newGroupName;

                probeCombo.comboList.Clear ();
                probeCombo.comboList.Add ("New level sensor...");
                probeCombo.activeIndex = 0;
                probeCombo.QueueDraw ();
                probeName = string.Empty;

                heaterCombo.comboList.Clear ();
                heaterCombo.comboList.Add ("New heater...");
                heaterCombo.activeIndex = 0;
                heaterCombo.QueueDraw ();
                heaterName = string.Empty;

            } else if (outcome == TouchSettingsOutcome.Deleted) {
                groupCombo.comboList.Remove (groupName);
                groupName = Temperature.defaultTemperatureGroup;
                if (groupName.IsNotEmpty ()) {
                    groupCombo.activeText = groupName;
                } else {
                    groupCombo.activeIndex = 0;
                }

                probeCombo.comboList.Clear ();
                if (groupName.IsNotEmpty ()) {
                    var groupsTemperatureProbes = Temperature.GetAllTemperatureProbesForTemperatureGroup (groupName);
                    if (groupsTemperatureProbes.Length > 0) {
                        probeCombo.comboList.AddRange (groupsTemperatureProbes);
                        probeName = groupsTemperatureProbes[0];
                    } else {
                        probeName = string.Empty;
                    }
                }
                probeCombo.comboList.Add ("New level sensor...");
                probeCombo.activeIndex = 0;
                probeCombo.QueueDraw ();

                heaterCombo.comboList.Clear ();
                if (groupName.IsNotEmpty ()) {
                    var heaterNames = Temperature.GetAllHeaterNames (groupName);
                    if (heaterNames.Length > 0) {
                        heaterCombo.comboList.AddRange (heaterNames);
                        heaterName = heaterNames[0];
                    } else {
                        heaterName = string.Empty;
                    }
                }
                heaterCombo.comboList.Add ("New heater...");
                heaterCombo.activeIndex = 0;
                heaterCombo.QueueDraw ();
            }

            groupCombo.QueueDraw ();
            GetGroupData ();
            GetProbeData ();
            GetHeaterData ();
        }

        protected void OnHeaterComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New heater...") {
                CallHeaterSettingsDialog (true);
            } else {
                heaterName = e.activeText;
            }

            heaterCombo.QueueDraw ();
            GetHeaterData ();
        }

        protected void OnHeaterSettingsButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            CallHeaterSettingsDialog ();
        }

        protected void CallHeaterSettingsDialog (bool forceNew = false) {
            if (groupName.IsNotEmpty ()) {
                HeaterSettings settings;
                if (heaterName.IsNotEmpty () && !forceNew) {
                    settings = Temperature.GetHeaterSettings (groupName, heaterName);
                } else {
                    settings = new HeaterSettings ();
                }
                var parent = Toplevel as Window;
                var s = new HeaterSettingsDialog (groupName, settings, parent);
                s.Run ();
                var newHeaterName = s.heaterName;
                var outcome = s.outcome;

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
                    var heaterNames = Temperature.GetAllHeaterNames (groupName);
                    if (heaterNames.Length > 0) {
                        heaterName = heaterNames[0];
                        heaterCombo.activeText = heaterName;
                    } else {
                        heaterName = string.Empty;
                        heaterCombo.activeIndex = 0;
                    }
                }

                heaterCombo.QueueDraw ();
                GetHeaterData ();
            }
        }

        protected void OnProbeComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New probe...") {
                CallTemperatureProbeSettingsDialog ();
            } else {
                probeName = e.activeText;
            }

            probeCombo.QueueDraw ();
            GetProbeData ();
        }

        protected void OnTemperatureProbeSettingsButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            CallTemperatureProbeSettingsDialog ();
        }

        protected void CallTemperatureProbeSettingsDialog (bool forceNew = false) {
            TemperatureProbeSettings settings;
            if (probeName.IsNotEmpty () && !forceNew) {
                settings = (TemperatureProbeSettings)AquaPicSensors.TemperatureProbes.GetSensorSettings (probeName);
            } else {
                settings = new TemperatureProbeSettings ();
            }
            var parent = Toplevel as Window;
            var s = new TemperatureProbeSettingsDialog (settings, parent);
            s.Run ();
            var newProbeName = s.temperatureProbeName;
            var outcome = s.outcome;

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
                var groupsTemperatureProbes = Temperature.GetAllTemperatureProbesForTemperatureGroup (groupName);
                if (groupsTemperatureProbes.Length > 0) {
                    probeName = groupsTemperatureProbes[0];
                    probeCombo.activeText = probeName;
                } else {
                    probeName = string.Empty;
                    probeCombo.activeIndex = 0;
                }
            }

            probeCombo.QueueDraw ();
            GetProbeData ();
        }

        protected void OnWaterLevelSensorCalibrationButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            if (probeName.IsNotEmpty ()) {
                var parent = Toplevel as Window;
                var cal = new CalibrationDialog (
                    "Temperature Probe",
                    parent,
                    () => {
                        var channel = AquaPicSensors.TemperatureProbes.GetSensor (probeName).channel;
                        return AquaPicDrivers.AnalogInput.GetChannelValue (channel);
                    });

                cal.CalibrationCompleteEvent += (a) => {
                    AquaPicSensors.TemperatureProbes.SetCalibrationData (
                        probeName,
                        (float)a.zeroActual,
                        (float)a.zeroValue,
                        (float)a.fullScaleActual,
                        (float)a.fullScaleValue);
                };

                var probe = (TemperatureProbe)AquaPicSensors.TemperatureProbes.GetSensor (probeName);
                cal.calArgs.zeroActual = probe.zeroScaleCalibrationActual;
                cal.calArgs.zeroValue = probe.zeroScaleCalibrationValue;
                cal.calArgs.fullScaleActual = probe.fullScaleCalibrationActual;
                cal.calArgs.fullScaleValue = probe.fullScaleCalibrationValue;

                cal.Run ();
            } else {
                MessageBox.Show ("No probe selected\n" +
                                "Can't perfom a calibration");
            }
        }

        protected override bool OnUpdateTimer () {
            GetGroupData ();
            GetProbeData ();
            GetHeaterData ();
            return true;
        }
    }
}

