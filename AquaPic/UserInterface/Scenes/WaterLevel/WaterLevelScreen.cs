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
using AquaPic.Modules;
using AquaPic.Drivers;
using AquaPic.Sensors;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class WaterLevelWindow : SceneBase
    {
        string groupName;
        TouchLabel levelLabel;
        TouchComboBox groupCombo;

		string atoGroupName;
		TouchLabel atoStateTextBox;
        TouchButton atoClearFailBtn;
		TouchComboBox atoGroupCombo;

        string analogSensorName;
        TouchLabel analogLevelTextBox;
        TouchComboBox analogCombo;

        string switchName;
        TouchLabel switchStateTextBox;
        TouchLabel switchTypeLabel;
        TouchLabel switchStateLabel;
        TouchComboBox switchCombo;

        public WaterLevelWindow (params object[] options) : base () {
            sceneTitle = "Water Level";

            ExposeEvent += OnExpose;

            /******************************************************************************************************/
            /* Water Level Groups                                                                                 */
            /******************************************************************************************************/
            groupName = WaterLevel.firstWaterLevelGroup;

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
                    groupName = WaterLevel.firstWaterLevelGroup;
                    groupCombo.activeText = groupName;
                }

                groupCombo.QueueDraw ();
                GetGroupData ();
            };
			Put (globalSettingsBtn, 290, 195);
            globalSettingsBtn.Show ();

			/******************************************************************************************************/
            /* ATO Groups                                                                                         */
            /******************************************************************************************************/
			atoGroupName = AutoTopOff.firstAtoGroup;

			label = new TouchLabel ();
            label.text = "ATO";
            label.textColor = "seca";
            label.textSize = 12;
			Put (label, 30, 280);
            label.Show ();

            var stateLabel = new TouchLabel ();
            stateLabel.text = "ATO State";
            stateLabel.textColor = "grey3";
            stateLabel.WidthRequest = 329;
            stateLabel.textAlignment = TouchAlignment.Center;
            Put (stateLabel, 60, 355);
            stateLabel.Show ();

            atoStateTextBox = new TouchLabel ();
            atoStateTextBox.WidthRequest = 329;
            atoStateTextBox.textSize = 20;
            atoStateTextBox.textAlignment = TouchAlignment.Center;
			Put (atoStateTextBox, 60, 320);
            atoStateTextBox.Show ();

            var atoSettingsBtn = new TouchButton ();
            atoSettingsBtn.text = "Settings";
            atoSettingsBtn.SetSizeRequest (100, 60);
            atoSettingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new AtoSettings (atoGroupName, atoGroupName.IsNotEmpty ());
                s.Run ();
                var newGroupName = s.atoGroupName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if (outcome == TouchSettingsOutcome.Added) {
                    atoGroupName = newGroupName;
                    atoGroupCombo.comboList.Insert (atoGroupCombo.comboList.Count - 1, atoGroupName);
                    atoGroupCombo.activeText = atoGroupName;
                } else if (outcome == TouchSettingsOutcome.Deleted) {
                    atoGroupCombo.comboList.Remove (atoGroupName);
                    atoGroupName = WaterLevel.firstWaterLevelGroup;
                    atoGroupCombo.activeText = atoGroupName;
                }

                atoGroupCombo.QueueDraw ();
                GetAtoGroupData ();
            };
            Put (atoSettingsBtn, 290, 405);
            atoSettingsBtn.Show ();

            atoClearFailBtn = new TouchButton ();
            atoClearFailBtn.SetSizeRequest (100, 60);
            atoClearFailBtn.text = "Reset ATO";
            atoClearFailBtn.buttonColor = "compl";
            atoClearFailBtn.ButtonReleaseEvent += (o, args) => {
                if (atoGroupName.IsNotEmpty ()) {
                    if (!AutoTopOff.ClearAtoAlarm (atoGroupName))
                        MessageBox.Show ("Please acknowledge alarms first");
                }
            };
            Put (atoClearFailBtn, 70, 405);

            /**************************************************************************************************************/
            /* Analog water sensor                                                                                        */
            /**************************************************************************************************************/
            analogSensorName = WaterLevel.firstAnalogLevelSensor;

            label = new TouchLabel ();
            label.text = "Analog Sensor";
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
                    analogSensorName = WaterLevel.firstAnalogLevelSensor;
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
                    MessageBox.Show ("Can't calibrate a none existent sensor");
                }
            };
            Put (b, 525, 195);
            b.Show ();

            /**************************************************************************************************************/
            /* Float Switches                                                                                             */
            /**************************************************************************************************************/
            switchName = WaterLevel.firstFloatSwitch;

            label = new TouchLabel ();
            label.text = "Probes";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 415, 280);
            label.Show ();

            switchStateLabel = new TouchLabel ();
            switchStateLabel.text = "Current Switch State";
            switchStateLabel.textAlignment = TouchAlignment.Center;
            switchStateLabel.textColor = "grey3";
            switchStateLabel.WidthRequest = 370;
            Put (switchStateLabel, 415, 355);

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
                    switchName = WaterLevel.firstFloatSwitch;
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

			atoGroupCombo = new TouchComboBox (AutoTopOff.GetAllAtoGroupNames ());
            if (atoGroupName.IsNotEmpty ()) {
                atoGroupCombo.activeText = atoGroupName;
            } else {
                atoGroupCombo.activeIndex = 0;
            }
            atoGroupCombo.WidthRequest = 235;
            atoGroupCombo.comboList.Add ("New group...");
            atoGroupCombo.ComboChangedEvent += OnGroupComboChanged;
			Put (atoGroupCombo, 153, 277);
            atoGroupCombo.Show ();

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
			GetAtoGroupData ();
            GetAnalogSensorData ();
            GetSwitchData ();

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

				cr.MoveTo (40, 267.5);
                cr.LineTo (387.5, 267.5);
                cr.ClosePath ();
                cr.Stroke ();

                cr.MoveTo (417.5, 267.5);
                cr.LineTo (780, 267.5);
                cr.ClosePath ();
                cr.Stroke ();
            }
        }

        protected override bool OnUpdateTimer () {
			GetGroupData ();
			GetAtoGroupData ();
            GetAnalogSensorData ();
            GetSwitchData ();
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
                    Console.WriteLine ("Moving combo back to {0}", groupName);
                    groupCombo.activeText = groupName;
                }

                groupCombo.QueueDraw ();
            } else {
                groupName = e.activeText;
            }
            GetGroupData ();
        }

		protected void OnAtoGroupComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New group...") {
                var s = new AtoSettings (string.Empty, false);
                s.Run ();
                var newGroupName = s.atoGroupName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if (outcome == TouchSettingsOutcome.Added) {
                    atoGroupCombo.comboList.Insert (atoGroupCombo.comboList.Count - 1, newGroupName);
                    atoGroupCombo.activeText = newGroupName;
                    atoGroupName = newGroupName;
                } else {
                    Console.WriteLine ("Moving combo back to {0}", atoGroupName);
                    atoGroupCombo.activeText = atoGroupName;
                }

                atoGroupCombo.QueueDraw ();
            } else {
                atoGroupName = e.activeText;
            }
            GetAtoGroupData ();
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
            if (groupName.IsNotEmpty ()) {
                levelLabel.text = WaterLevel.GetWaterLevelGroupLevel (groupName).ToString ("F1");
                levelLabel.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
            } else {
                levelLabel.text = "--";
                levelLabel.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
            }

            levelLabel.QueueDraw ();
        }

		protected void GetAtoGroupData () {
            if (atoGroupName.IsNotEmpty ()) {
                if (AutoTopOff.GetAtoGroupEnable (atoGroupName)) {
                    atoStateTextBox.text = string.Format ("{0} : {1}",
                        AutoTopOff.GetAtoGroupState (atoGroupName),
                        AutoTopOff.GetAtoGroupAtoTime (atoGroupName).SecondsToString ());

                    if (Alarm.CheckAlarming (AutoTopOff.GetAtoGroupFailAlarmIndex (atoGroupName))) {
                        atoClearFailBtn.Visible = true;
                        atoClearFailBtn.Show ();
                    } else {
                        atoClearFailBtn.Visible = false;
                    }

                } else {
                    atoStateTextBox.text = "ATO Disabled";
                    atoClearFailBtn.Visible = false;
                }
            } else {
                atoStateTextBox.text = "ATO Disabled";
                atoClearFailBtn.Visible = false;
            }

            atoStateTextBox.QueueDraw ();
        }

        protected void GetAnalogSensorData () {
            if (analogSensorName.IsNotEmpty ()) {
                float wl = WaterLevel.GetAnalogLevelSensorLevel (analogSensorName);
                if (wl < 0.0f) {
                    analogLevelTextBox.text = "Probe Disconnected";
                    analogLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                } else {
                    analogLevelTextBox.text = wl.ToString ("F2");
                    analogLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
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

                switchStateLabel.Visible = true;
                switchTypeLabel.Visible = true;

                SwitchType type = WaterLevel.GetFloatSwitchType (switchName);
                switchTypeLabel.text = Utils.GetDescription (type);
            } else {
                switchStateLabel.Visible = false;
                switchTypeLabel.Visible = false;
                switchStateTextBox.text = "Switch not available";
                switchStateTextBox.textColor = "white";
            }

            switchTypeLabel.QueueDraw ();
            switchStateTextBox.QueueDraw ();
        }
    }
}

