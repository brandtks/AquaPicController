#region License

/*
 AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

 Copyright (c) 2018 Goodtime Development

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
using GoodtimeDevelopment.Utilites;
using GoodtimeDevelopment.TouchWidget;
using AquaPic.Sensors;
using AquaPic.Sensors.PhProbe;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class PhProbeWidget : Fixed
    {
        string probeName;
        TouchComboBox probeCombo;
        TouchLabel probeTextbox;
        TouchLabel probeLabel;

        public PhProbeWidget () {
            SetSizeRequest (370, 188);

            var label = new TouchLabel ();
            label.text = "Probes";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 0, 3);
            label.Show ();

            var probeSetupBtn = new TouchButton ();
            probeSetupBtn.text = Convert.ToChar (0x2699).ToString ();
            probeSetupBtn.SetSizeRequest (30, 30);
            probeSetupBtn.buttonColor = "pri";
            probeSetupBtn.ButtonReleaseEvent += OnProbeSettingsButtonReleaseEvent;
            Put (probeSetupBtn, 340, 0);
            probeSetupBtn.Show ();

            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 60);
            b.ButtonReleaseEvent += OnCalibrationButtonReleaseEvent;
            Put (b, 0, 128);
            b.Show ();

            probeLabel = new TouchLabel ();
            probeLabel.text = "pH";
            probeLabel.textAlignment = TouchAlignment.Center;
            probeLabel.textColor = "grey3";
            probeLabel.WidthRequest = 370;
            Put (probeLabel, 0, 78);
            probeLabel.Show ();

            probeTextbox = new TouchLabel ();
            probeTextbox.WidthRequest = 370;
            probeTextbox.textSize = 20;
            probeTextbox.textAlignment = TouchAlignment.Center;
            probeTextbox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
            Put (probeTextbox, 0, 43);
            probeTextbox.Show ();

            probeCombo = new TouchComboBox ();
            probeCombo.WidthRequest = 200;
            var probes = AquaPicSensors.PhProbes.GetAllSensorNames ();
            if (probes.Length > 0) {
                probeCombo.comboList.AddRange (probes);
                probeName = probes[0];
            } else {
                probeName = string.Empty;
            }
            probeCombo.comboList.Add ("New probe...");
            probeCombo.activeIndex = 0;
            probeCombo.ComboChangedEvent += OnProbeComboChanged;
            Put (probeCombo, 135, 0);
            probeCombo.Show ();

            GetProbeData ();

            Show ();
        }

        public void GetProbeData () {
            if (probeName.IsNotEmpty ()) {
                var probe = (PhProbe)AquaPicSensors.PhProbes.GetSensor (probeName);
                if (probe.connected) {
                    probeTextbox.text = probe.value.ToString ("F2");
                    probeLabel.Visible = true;
                } else {
                    probeTextbox.text = "Probe disconnected";
                    probeLabel.Visible = false;
                }
            } else {
                probeTextbox.text = "Probe not available";
                probeLabel.Visible = false;
            }

            probeTextbox.QueueDraw ();
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

        protected void OnProbeSettingsButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            CallTemperatureProbeSettingsDialog ();
        }

        protected void CallTemperatureProbeSettingsDialog (bool forceNew = false) {
            PhProbeSettings settings;
            if (probeName.IsNotEmpty () && !forceNew) {
                settings = (PhProbeSettings)AquaPicSensors.PhProbes.GetSensorSettings (probeName);
            } else {
                settings = new PhProbeSettings ();
            }
            var parent = Toplevel as Window;
            var s = new PhProbeSettingsDialog (settings, parent);
            s.Run ();
            var newProbeName = s.probeName;
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
                var probes = AquaPicSensors.PhProbes.GetAllSensorNames ();
                if (probes.Length > 0) {
                    probeName = probes[0];
                    probeCombo.activeText = probeName;
                } else {
                    probeName = string.Empty;
                    probeCombo.activeIndex = 0;
                }
            }

            probeCombo.QueueDraw ();
            GetProbeData ();
        }

        protected void OnCalibrationButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            if (probeName.IsNotEmpty ()) {
                var parent = Toplevel as Window;
                var cal = new CalibrationDialog (
                    "pH Probe",
                    parent,
                    () => {
                        var channel = AquaPicSensors.PhProbes.GetSensor (probeName).channel;
                        return AquaPicDrivers.PhOrp.GetChannelValue (channel);
                    });

                cal.CalibrationCompleteEvent += (a) => {
                    AquaPicSensors.TemperatureProbes.SetCalibrationData (
                        probeName,
                        (float)a.zeroActual,
                        (float)a.zeroValue,
                        (float)a.fullScaleActual,
                        (float)a.fullScaleValue);
                };

                var probe = (PhProbe)AquaPicSensors.PhProbes.GetSensor (probeName);
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

    }
}
