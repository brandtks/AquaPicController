#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Runtime;
using AquaPic.Modules;
using AquaPic.Drivers;
using AquaPic.Utilites;
using AquaPic.Sensors;

namespace AquaPic.UserInterface
{
    public class WaterLevelWindow : SceneBase
    {
        uint timerId;

        string groupName;
        TouchLabel levelLabel;
        TouchComboBox groupCombo;

        string analogSensorName;
        TouchLabel analogLevelTextBox;
        TouchComboBox analogCombo;

        string switchName;
        TouchLabel switchStateTextBox;
        TouchLabel switchTypeLabel;
        TouchComboBox switchCombo;

        public WaterLevelWindow (params object[] options) : base () {
            sceneTitle = "Water Level";

            ExposeEvent += OnExpose;

            /******************************************************************************************************/
            /* Water Level Groups                                                                                 */
            /******************************************************************************************************/
            groupName = WaterLevel.defaultWaterLevelGroup;

            var label = new TouchLabel ();
            label.text = "Groups";
            label.WidthRequest = 118;
            label.textColor = "seca";
            label.textSize = 12;
            label.textAlignment = TouchAlignment.Left;
            Put (label, 30, 80);
            label.Show ();

            label = new TouchLabel ();
            label.WidthRequest = 329;
            label.text = "Level";
            label.textColor = "grey3";
            label.textAlignment = TouchAlignment.Center;
            Put (label, 60, 185);
            label.Show ();

            levelLabel = new TouchLabel ();
            levelLabel.SetSizeRequest (329, 50);
            levelLabel.textSize = 36;
            levelLabel.textAlignment = TouchAlignment.Center;
            levelLabel.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
            Put (levelLabel, 60, 130);
            levelLabel.Show ();

            var globalSettingsBtn = new TouchButton ();
            globalSettingsBtn.text = "Settings";
            globalSettingsBtn.SetSizeRequest (100, 60);
            globalSettingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new WaterGroupSettings (groupName, groupName.IsNotEmpty ());
                s.Run ();
                var newGroupName = s.waterLevelGroupName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if (outcome == TouchSettingsOutcome.Added) {
                    groupName = newGroupName;
                    groupCombo.comboList.Insert (groupCombo.comboList.Count - 1, groupName);
                    groupCombo.activeText = groupName;
                } else if (outcome == TouchSettingsOutcome.Deleted) {
                    groupCombo.comboList.Remove (groupName);
                    groupName = WaterLevel.defaultWaterLevelGroup;
                    groupCombo.activeText = groupName;
                }

                groupCombo.QueueDraw ();
                GetGroupData ();
            };
            Put (globalSettingsBtn, 290, 405);
            globalSettingsBtn.Show ();

            /**************************************************************************************************************/
            /* Analog water sensor                                                                                        */
            /**************************************************************************************************************/
            analogSensorName = WaterLevel.defaultAnalogLevelSensor;

            label = new TouchLabel ();
            label.text = "Water Level Sensor";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 415, 80);
            label.Show ();

            label = new TouchLabel ();
            label.WidthRequest = 370;
            label.text = "Water Level";
            label.textColor = "grey3";
            label.textAlignment = TouchAlignment.Center;
            Put (label, 415, 155);
            label.Show ();

            analogLevelTextBox = new TouchLabel ();
            analogLevelTextBox.WidthRequest = 370;
            analogLevelTextBox.textSize = 20;
            analogLevelTextBox.textAlignment = TouchAlignment.Center;
            Put (analogLevelTextBox, 415, 120);

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Settings";
            settingsBtn.SetSizeRequest (100, 60);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new AnalogSensorSettings (analogSensorName, analogSensorName.IsNotEmpty ());
                s.Run ();
                var newAnalogSensorName = s.newOrUpdatedAnalogSensorName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if ((outcome == TouchSettingsOutcome.Modified) && (newAnalogSensorName != analogSensorName)) {
                    var index = analogCombo.comboList.IndexOf (analogSensorName);
                    analogCombo.comboList[index] = newAnalogSensorName;
                    analogSensorName = newAnalogSensorName;
                } else if (outcome == TouchSettingsOutcome.Added) {
                    analogCombo.comboList.Insert (analogCombo.comboList.Count - 1, newAnalogSensorName);
                    analogCombo.activeText = newAnalogSensorName;
                    analogSensorName = newAnalogSensorName;
                } else if (outcome == TouchSettingsOutcome.Deleted) {
                    analogCombo.comboList.Remove (analogSensorName);
                    analogSensorName = WaterLevel.defaultAnalogLevelSensor;
                    analogCombo.activeText = analogSensorName;
                }

                analogCombo.QueueDraw ();
                GetAnalogSensorData ();
            };
            Put (settingsBtn, 415, 195);
            settingsBtn.Show ();

            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 60);
            b.ButtonReleaseEvent += (o, args) => {
                if (analogSensorName.IsNotEmpty ()) {
                    if (WaterLevel.GetAnalogLevelSensorEnable (analogSensorName)) {
                        var cal = new CalibrationDialog (
                            "Water Level Sensor",
                            () => {
                                return AquaPicDrivers.AnalogInput.GetChannelValue (
                                    WaterLevel.GetAnalogLevelSensorIndividualControl (analogSensorName)
                                );
                            });

                        cal.CalibrationCompleteEvent += (aa) => {
                            WaterLevel.SetCalibrationData (
                                analogSensorName, 
                                (float)aa.zeroValue,
                                (float)aa.fullScaleActual,
                                (float)aa.fullScaleValue);
                        };

                        cal.calArgs.zeroValue = WaterLevel.GetAnalogLevelSensorZeroScaleValue (analogSensorName);
                        cal.calArgs.fullScaleActual = WaterLevel.GetAnalogLevelSensorFullScaleActual (analogSensorName);
                        cal.calArgs.fullScaleValue =WaterLevel.GetAnalogLevelSensorFullScaleValue (analogSensorName);

                        cal.Run ();
                        cal.Destroy ();
                        cal.Dispose ();
                    } else {
                        MessageBox.Show ("Analog water level sensor is disabled\n" +
                                        "Can't perfom a calibration");
                    }
                } else {
                    MessageBox.Show ("Can't calibrate a none existent sensor");
                }
            };
            Put (b, 525, 195);
            b.Show ();

            /**************************************************************************************************************/
            /* Float Switches                                                                                             */
            /**************************************************************************************************************/
            switchName = WaterLevel.defaultFloatSwitch;

            label = new TouchLabel ();
            label.text = "Probes";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 415, 280);
            label.Show ();

            var sLabel = new TouchLabel ();
            sLabel.text = "Current Switch State";
            sLabel.textAlignment = TouchAlignment.Center;
            sLabel.textColor = "grey3";
            sLabel.WidthRequest = 370;
            Put (sLabel, 415, 355);
            sLabel.Show ();

            switchStateTextBox = new TouchLabel ();
            switchStateTextBox.WidthRequest = 370;
            switchStateTextBox.textSize = 20;
            switchStateTextBox.textAlignment = TouchAlignment.Center;
            Put (switchStateTextBox, 415, 320);
            switchStateTextBox.Show ();

            //Type
            switchTypeLabel = new TouchLabel ();
            switchTypeLabel.WidthRequest = 370;
            switchTypeLabel.textAlignment = TouchAlignment.Center;
            switchTypeLabel.textColor = "grey3";
            Put (switchTypeLabel, 415, 370);
            switchTypeLabel.Show ();

            var switchSetupBtn = new TouchButton ();
            switchSetupBtn.text = "Probe Setup";
            switchSetupBtn.SetSizeRequest (100, 60);
            switchSetupBtn.ButtonReleaseEvent += (o, args) => {
                var s = new SwitchSettings (switchName, switchName.IsNotEmpty ());
                s.Run ();
                var newSwitchName = s.newOrUpdatedFloatSwitchName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if ((outcome == TouchSettingsOutcome.Modified) && (newSwitchName != switchName)) {
                    var index = switchCombo.comboList.IndexOf (switchName);
                    switchCombo.comboList[index] = newSwitchName;
                    switchName = newSwitchName;
                } else if (outcome == TouchSettingsOutcome.Added) {
                    switchCombo.comboList.Insert (switchCombo.comboList.Count - 1, newSwitchName);
                    switchCombo.activeText = newSwitchName;
                    switchName = newSwitchName;
                } else if (outcome == TouchSettingsOutcome.Deleted) {
                    switchCombo.comboList.Remove (switchName);
                    switchName = WaterLevel.defaultFloatSwitch;
                    switchCombo.activeText = switchName;
                }

                switchCombo.QueueDraw ();
                GetSwitchData ();
            };
            Put (switchSetupBtn, 415, 405);
            switchSetupBtn.Show ();

            groupCombo = new TouchComboBox (WaterLevel.GetAllWaterLevelGroupNames ());
            if (groupName.IsNotEmpty ()) {
                groupCombo.activeText = groupName;
            } else {
                groupCombo.activeIndex = 0;
            }
            groupCombo.WidthRequest = 235;
            groupCombo.comboList.Add ("New group...");
            groupCombo.ComboChangedEvent += OnGroupComboChanged;
            Put (groupCombo, 153, 77);
            groupCombo.Show ();

            analogCombo = new TouchComboBox (WaterLevel.GetAllAnalogLevelSensors ());
            if (analogSensorName.IsNotEmpty ()) {
                analogCombo.activeText = analogSensorName;
            } else {
                analogCombo.activeIndex = 0;
            }
            analogCombo.WidthRequest = 235;
            analogCombo.comboList.Add ("New level sensor...");
            analogCombo.ComboChangedEvent += OnAnalogSensorComboChanged;
            Put (analogCombo, 550, 77);
            analogCombo.Show ();

            switchCombo = new TouchComboBox (WaterLevel.GetAllFloatSwitches ());
            if (switchName.IsNotEmpty ()) {
                switchCombo.activeText = switchName;
            } else {
                switchCombo.activeIndex = 0;
            }
            switchCombo.WidthRequest = 235;
            switchCombo.comboList.Add ("New switch...");
            switchCombo.ComboChangedEvent += OnSwitchComboChanged;
            Put (switchCombo, 550, 277);
            switchCombo.Show ();

            GetGroupData ();
            GetAnalogSensorData ();
            GetSwitchData ();

            timerId = GLib.Timeout.Add (1000, OnUpdateTimer);

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

        public bool OnUpdateTimer () {
            GetAnalogSensorData ();
            GetSwitchData ();
            GetGroupData ();

            return true;
        }

        protected void OnGroupComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New group...") {
                var s = new WaterGroupSettings (string.Empty, false);
                s.Run ();
                var newGroupName = s.waterLevelGroupName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if (outcome == TouchSettingsOutcome.Added) {
                    groupCombo.comboList.Insert (groupCombo.comboList.Count - 1, newGroupName);
                    groupCombo.activeText = newGroupName;
                    groupName = newGroupName;
                } else {
                    groupCombo.activeText = groupName;
                }

                groupCombo.QueueDraw ();
            } else {
                groupName = e.activeText;
            }
            GetGroupData ();
        }

        protected void OnAnalogSensorComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New level sensor...") {
                var s = new AnalogSensorSettings (string.Empty, false);
                s.Run ();
                var newAnalogSensorName = s.newOrUpdatedAnalogSensorName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if (outcome == TouchSettingsOutcome.Added) {
                    analogCombo.comboList.Insert (analogCombo.comboList.Count - 1, newAnalogSensorName);
                    analogCombo.activeText = newAnalogSensorName;
                    analogSensorName = newAnalogSensorName;
                } else {
                    analogCombo.activeText = analogSensorName;
                }
                analogCombo.QueueDraw ();
            } else {
                analogSensorName = e.activeText;
            }
            GetAnalogSensorData ();
        }

        protected void OnSwitchComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New switch...") {
                var s = new SwitchSettings (string.Empty, false);
                s.Run ();
                var newSwitchName = s.newOrUpdatedFloatSwitchName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if (outcome == TouchSettingsOutcome.Added) {
                    switchCombo.comboList.Insert (switchCombo.comboList.Count - 1, newSwitchName);
                    switchCombo.activeText = newSwitchName;
                    switchName = newSwitchName;
                } else {
                    switchCombo.activeText = switchName;
                }
                switchCombo.QueueDraw ();
            } else {
                switchName = e.activeText;
            }
            GetSwitchData ();
        }

        protected void GetGroupData () {
            if (!groupName.IsEmpty ()) {
                levelLabel.text = WaterLevel.GetWaterLevelGroupLevel (groupName).ToString ("F1");
            } else {
                levelLabel.text = "--";
            }

            levelLabel.QueueDraw ();
        }

        protected void GetAnalogSensorData () {
            if (analogSensorName.IsNotEmpty ()) {
                if (WaterLevel.GetAnalogLevelSensorEnable (analogSensorName)) {
                    float wl = WaterLevel.GetAnalogLevelSensorLevel (analogSensorName);
                    if (wl < 0.0f) {
                        analogLevelTextBox.text = "Probe Disconnected";
                        analogLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                    } else {
                        analogLevelTextBox.text = wl.ToString ("F2");
                        analogLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
                    }
                } else {
                    analogLevelTextBox.text = "Sensor disabled";
                }
            } else {
                analogLevelTextBox.text = "--";
                analogLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
            }

            analogLevelTextBox.QueueDraw ();
        }

        protected void GetSwitchData () {
            if (switchName.IsNotEmpty ()) {
                bool state = WaterLevel.GetFloatSwitchState (switchName);

                if (state) {
                    switchStateTextBox.text = "Activated";
                    switchStateTextBox.textColor = "pri";
                } else {
                    switchStateTextBox.text = "Normal";
                    switchStateTextBox.textColor = "seca";
                }

                SwitchType type = WaterLevel.GetFloatSwitchType (switchName);
                switchTypeLabel.text = Utils.GetDescription (type);
            } else {
                switchTypeLabel.Visible = false;
                switchStateTextBox.text = "Switch not available";
                switchStateTextBox.textColor = "white";
            }

            switchTypeLabel.QueueDraw ();
            switchStateTextBox.QueueDraw ();
        }
    }
}

