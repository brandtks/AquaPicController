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
        TouchLabel analogSensorLevelTextBox;
        TouchLabel analogSensorLevelLabel;
        TouchComboBox analogSensorCombo;

        string switchName;
        TouchLabel switchStateTextBox;
        TouchLabel switchTypeLabel;
        TouchLabel switchStateLabel;
        TouchComboBox switchCombo;

        public WaterLevelWindow (params object[] options) {
            sceneTitle = "Water Level";

            ExposeEvent += OnExpose;

            /******************************************************************************************************/
            /* Water Level Groups                                                                                 */
            /******************************************************************************************************/
            groupName = WaterLevel.firstWaterLevelGroup;

            if (options.Length >= 3) {
                var requestedGroup = options[2] as string;
                if (requestedGroup != null) {
                    if (WaterLevel.WaterLevelGroupNameExists (requestedGroup)) {
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
            Put (label, 37, 80);
            label.Show ();

            label = new TouchLabel ();
            label.WidthRequest = 329;
            label.text = "Level";
            label.textColor = "grey3";
            label.textAlignment = TouchAlignment.Center;
            Put (label, 60, 155);
            label.Show ();

            levelLabel = new TouchLabel ();
            levelLabel.WidthRequest = 329;
            levelLabel.textSize = 20;
            levelLabel.textAlignment = TouchAlignment.Center;
            levelLabel.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
            Put (levelLabel, 60, 120);
            levelLabel.Show ();

            var globalSettingsBtn = new TouchButton ();
            globalSettingsBtn.text = "\u2699";
            globalSettingsBtn.SetSizeRequest (30, 30);
            globalSettingsBtn.buttonColor = "pri";
            globalSettingsBtn.ButtonReleaseEvent += OnGroupSettingsButtonReleaseEvent;
            Put (globalSettingsBtn, 358, 77);
            globalSettingsBtn.Show ();

            /******************************************************************************************************/
            /* ATO Groups                                                                                         */
            /******************************************************************************************************/
            atoGroupName = AutoTopOff.firstAtoGroup;

            label = new TouchLabel ();
            label.text = "ATO";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 37, 280);
            label.Show ();

            atoStateTextBox = new TouchLabel ();
            atoStateTextBox.WidthRequest = 329;
            atoStateTextBox.textSize = 20;
            atoStateTextBox.textAlignment = TouchAlignment.Center;
            Put (atoStateTextBox, 60, 320);
            atoStateTextBox.Show ();

            var atoSettingsBtn = new TouchButton ();
            atoSettingsBtn.text = "\u2699";
            atoSettingsBtn.SetSizeRequest (30, 30);
            atoSettingsBtn.buttonColor = "pri";
            atoSettingsBtn.ButtonReleaseEvent += OnAtoSettingsButtonReleaseEvent;
            Put (atoSettingsBtn, 358, 277);
            atoSettingsBtn.Show ();

            atoClearFailBtn = new TouchButton ();
            atoClearFailBtn.SetSizeRequest (100, 60);
            atoClearFailBtn.text = "Reset ATO";
            atoClearFailBtn.buttonColor = "compl";
            atoClearFailBtn.ButtonReleaseEvent += (o, args) => {
                if (atoGroupName.IsNotEmpty ()) {
                    if (AutoTopOff.GetAtoGroupState (atoGroupName) == AutoTopOffState.Cooldown) {
                        AutoTopOff.ResetCooldownTime (atoGroupName);
                    } else {
                        if (!AutoTopOff.ClearAtoAlarm (atoGroupName)) {
                            MessageBox.Show ("Please acknowledge alarms first");
                        }
                    }
                }
            };
            Put (atoClearFailBtn, 70, 405);

            /**************************************************************************************************************/
            /* Analog water sensor                                                                                        */
            /**************************************************************************************************************/
            label = new TouchLabel ();
            label.text = "Analog Sensor";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 415, 80);
            label.Show ();

            analogSensorLevelLabel = new TouchLabel ();
            analogSensorLevelLabel.WidthRequest = 370;
            analogSensorLevelLabel.text = "Water Level";
            analogSensorLevelLabel.textColor = "grey3";
            analogSensorLevelLabel.textAlignment = TouchAlignment.Center;
            Put (analogSensorLevelLabel, 415, 155);
            analogSensorLevelLabel.Show ();

            analogSensorLevelTextBox = new TouchLabel ();
            analogSensorLevelTextBox.WidthRequest = 370;
            analogSensorLevelTextBox.textSize = 20;
            analogSensorLevelTextBox.textAlignment = TouchAlignment.Center;
            Put (analogSensorLevelTextBox, 415, 120);

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "\u2699";
            settingsBtn.SetSizeRequest (30, 30);
            settingsBtn.buttonColor = "pri";
            settingsBtn.ButtonReleaseEvent += OnWaterLevelSensorSettingsButtonReleaseEvent;
            Put (settingsBtn, 755, 77);
            settingsBtn.Show ();

            var calibrationButton = new TouchButton ();
            calibrationButton.text = "Calibrate";
            calibrationButton.SetSizeRequest (100, 60);
            calibrationButton.ButtonReleaseEvent += OnWaterLevelSensorCalibrationButtonReleaseEvent;
            Put (calibrationButton, 415, 195);
            calibrationButton.Show ();

            /**************************************************************************************************************/
            /* Float Switches                                                                                             */
            /**************************************************************************************************************/
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
            switchSetupBtn.text = "\u2699";
            switchSetupBtn.SetSizeRequest (30, 30);
            switchSetupBtn.buttonColor = "pri";
            switchSetupBtn.ButtonReleaseEvent += OnFloatSwitchSettingsButtonReleaseEvent;
            Put (switchSetupBtn, 755, 277);
            switchSetupBtn.Show ();

            groupCombo = new TouchComboBox (WaterLevel.GetAllWaterLevelGroupNames ());
            if (groupName.IsNotEmpty ()) {
                groupCombo.activeText = groupName;
            } else {
                groupCombo.activeIndex = 0;
            }
            groupCombo.WidthRequest = 200;
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
            atoGroupCombo.WidthRequest = 200;
            atoGroupCombo.comboList.Add ("New ATO...");
            atoGroupCombo.ComboChangedEvent += OnAtoGroupComboChanged;
            Put (atoGroupCombo, 153, 277);
            atoGroupCombo.Show ();

            analogSensorCombo = new TouchComboBox ();
            analogSensorCombo.WidthRequest = 200;
            if (groupName.IsNotEmpty ()) {
                var groupsWaterLevelSensors = WaterLevel.GetAllWaterLevelSensorsForWaterLevelGroup (groupName);
                analogSensorCombo.comboList.AddRange (groupsWaterLevelSensors);
                if (groupsWaterLevelSensors.Length > 0) {
                    analogSensorName = groupsWaterLevelSensors[0];
                    analogSensorCombo.activeText = analogSensorName;
                } else {
                    analogSensorName = string.Empty;
                }
            }
            analogSensorCombo.comboList.Add ("New level sensor...");
            analogSensorCombo.ComboChangedEvent += OnAnalogSensorComboChanged;
            Put (analogSensorCombo, 550, 77);
            analogSensorCombo.Show ();

            switchCombo = new TouchComboBox ();
            switchCombo.WidthRequest = 200;
            if (groupName.IsNotEmpty ()) {
                var groupsFloatSwitches = WaterLevel.GetAllFloatSwitchesForWaterLevelGroup (groupName);
                switchCombo.comboList.AddRange (groupsFloatSwitches);
                if (groupsFloatSwitches.Length > 0) {
                    switchName = groupsFloatSwitches[0];
                    switchCombo.activeText = switchName;
                } else {
                    switchName = string.Empty;
                }
            }
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

        protected void GetGroupData () {
            if (groupName.IsNotEmpty ()) {
                levelLabel.text = WaterLevel.GetWaterLevelGroupLevel (groupName).ToString ("F1");
            } else {
                levelLabel.text = "--";
            }

            levelLabel.QueueDraw ();
        }

        protected void GetAtoGroupData () {
            if (atoGroupName.IsNotEmpty ()) {
                if (AutoTopOff.GetAtoGroupEnable (atoGroupName)) {
                    atoStateTextBox.text = string.Format ("{0} : {1}",
                        AutoTopOff.GetAtoGroupState (atoGroupName),
                        AutoTopOff.GetAtoGroupAtoTime (atoGroupName).SecondsToString ());

                    if (Alarm.CheckAlarming (AutoTopOff.GetAtoGroupFailAlarmIndex (atoGroupName)) ||
                        AutoTopOff.GetAtoGroupState (atoGroupName) == AutoTopOffState.Cooldown) {
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
                float level = (float)AquaPicSensors.WaterLevelSensors.GetSensorValue (analogSensorName);
                if (level < 0f) {
                    analogSensorLevelTextBox.text = "Probe Disconnected";
                    analogSensorLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                    analogSensorLevelLabel.Visible = true;
                } else {
                    analogSensorLevelTextBox.text = level.ToString ("F2");
                    analogSensorLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
                    analogSensorLevelLabel.Visible = false;
                }
            } else {
                analogSensorLevelTextBox.text = "Probe not available";
                analogSensorLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                analogSensorLevelLabel.Visible = false;
            }

            analogSensorCombo.Visible = false;
            analogSensorCombo.Visible = true;

            analogSensorCombo.QueueDraw ();
            analogSensorLevelTextBox.QueueDraw ();
        }

        protected void GetSwitchData () {
            if (switchName.IsNotEmpty ()) {
                var floatSwitch = (FloatSwitch)AquaPicSensors.FloatSwitches.GetSensor (switchName);
                bool state = floatSwitch.activated;

                if (state) {
                    switchStateTextBox.text = "Activated";
                    switchStateTextBox.textColor = "pri";
                } else {
                    switchStateTextBox.text = "Normal";
                    switchStateTextBox.textColor = "seca";
                }

                switchStateLabel.Visible = true;
                switchTypeLabel.Visible = true;

                SwitchType type = floatSwitch.switchType;
                switchTypeLabel.text = Utils.GetDescription (type);
            } else {
                switchStateLabel.Visible = false;
                switchTypeLabel.Visible = false;
                switchStateTextBox.text = "Switch not available";
                switchStateTextBox.textColor = "white";
            }

            switchCombo.Visible = false;
            switchCombo.Visible = true;

            switchCombo.QueueDraw ();
            switchTypeLabel.QueueDraw ();
            switchStateTextBox.QueueDraw ();
        }

        protected void OnGroupSettingsButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            var parent = Toplevel as Window;
            var s = new WaterGroupSettings (WaterLevel.GetWaterLevelGroupSettings (groupName), parent);
            s.Run ();
            var newGroupName = s.groupName;
            var outcome = s.outcome;

            if ((outcome == TouchSettingsOutcome.Modified) && (newGroupName != groupName)) {
                var index = groupCombo.comboList.IndexOf (groupName);
                groupCombo.comboList[index] = newGroupName;
                groupCombo.activeText = newGroupName;
            } else if (outcome == TouchSettingsOutcome.Added) {
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
        }

        protected void OnGroupComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New group...") {
                var parent = Toplevel as Window;
                var s = new WaterGroupSettings (new WaterLevelGroupSettings (), parent);
                s.Run ();
                var newGroupName = s.groupName;
                var outcome = s.outcome;

                if (outcome == TouchSettingsOutcome.Added) {
                    groupCombo.comboList.Insert (groupCombo.comboList.Count - 1, newGroupName);
                    groupCombo.activeText = newGroupName;
                    groupName = newGroupName;
                } else {
                    groupCombo.activeText = groupName;
                }
            } else {
                groupName = e.activeText;

                analogSensorCombo.comboList.Clear ();
                if (groupName.IsNotEmpty ()) {
                    var groupsWaterLevelSensors = WaterLevel.GetAllWaterLevelSensorsForWaterLevelGroup (groupName);
                    analogSensorCombo.comboList.AddRange (groupsWaterLevelSensors);
                    if (groupsWaterLevelSensors.Length > 0) {
                        analogSensorName = groupsWaterLevelSensors[0];
                    } else {
                        analogSensorName = string.Empty;
                    }
                }
                analogSensorCombo.comboList.Add ("New level sensor...");
                analogSensorCombo.activeIndex = 0;
                analogSensorCombo.QueueDraw ();

                switchCombo.comboList.Clear ();
                if (groupName.IsNotEmpty ()) {
                    var groupsFloatSwitches = WaterLevel.GetAllFloatSwitchesForWaterLevelGroup (groupName);
                    switchCombo.comboList.AddRange (groupsFloatSwitches);
                    if (groupsFloatSwitches.Length > 0) {
                        switchName = groupsFloatSwitches[0];
                    } else {
                        switchName = string.Empty;
                    }
                }
                switchCombo.comboList.Add ("New switch...");
                switchCombo.activeIndex = 0;
                switchCombo.QueueDraw ();
            }

            GetGroupData ();
            GetAnalogSensorData ();
            GetSwitchData ();
        }

        protected void OnAtoSettingsButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            var parent = Toplevel as Window;
            var s = new AtoSettings (AutoTopOff.GetAutoTopOffGroupSettings (atoGroupName), parent);
            s.Run ();
            var newAtoGroupName = s.groupName;
            var outcome = s.outcome;

            if ((outcome == TouchSettingsOutcome.Modified) && (newAtoGroupName != groupName)) {
                var index = atoGroupCombo.comboList.IndexOf (groupName);
                atoGroupCombo.comboList[index] = newAtoGroupName;
                atoGroupName = newAtoGroupName;
            } else if (outcome == TouchSettingsOutcome.Added) {
                atoGroupCombo.comboList.Insert (atoGroupCombo.comboList.Count - 1, newAtoGroupName);
                atoGroupCombo.activeText = newAtoGroupName;
                atoGroupName = newAtoGroupName;
            } else if (outcome == TouchSettingsOutcome.Deleted) {
                atoGroupCombo.comboList.Remove (atoGroupName);
                atoGroupName = AutoTopOff.firstAtoGroup;
                if (atoGroupName.IsNotEmpty ()) {
                    atoGroupCombo.activeText = atoGroupName;
                } else {
                    atoGroupCombo.activeIndex = -1;
                }
            }

            atoGroupCombo.QueueDraw ();
            GetAtoGroupData ();
        }

        protected void OnAtoGroupComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New ATO...") {
                var parent = Toplevel as Window;
                var s = new AtoSettings (new AutoTopOffGroupSettings (), parent);
                s.Run ();
                var newGroupName = s.groupName;
                var outcome = s.outcome;

                if (outcome == TouchSettingsOutcome.Added) {
                    atoGroupCombo.comboList.Insert (atoGroupCombo.comboList.Count - 1, newGroupName);
                    atoGroupCombo.activeText = newGroupName;
                    atoGroupName = newGroupName;
                } else {
                    atoGroupCombo.activeText = atoGroupName;
                }
            } else {
                atoGroupName = e.activeText;
            }
            GetAtoGroupData ();
        }

        protected void OnWaterLevelSensorSettingsButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            CallWaterLevelSensorSettingsDialog ();
        }

        protected void OnAnalogSensorComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New level sensor...") {
                CallWaterLevelSensorSettingsDialog (true);
            } else {
                analogSensorName = e.activeText;
            }

            GetAnalogSensorData ();
        }

        protected void CallWaterLevelSensorSettingsDialog (bool forceNew = false) {
            WaterLevelSensorSettings settings;
            if (analogSensorName.IsNotEmpty () && !forceNew) {
                settings = (WaterLevelSensorSettings)AquaPicSensors.WaterLevelSensors.GetSensorSettings (analogSensorName);
            } else {
                settings = new WaterLevelSensorSettings ();
            }
            var parent = Toplevel as Window;
            var s = new WaterLevelSensorSettingsDialog (settings, parent);
            s.Run ();
            var newAnalogSensorName = s.waterLevelSensorName;
            var outcome = s.outcome;

            if ((outcome == TouchSettingsOutcome.Modified) && (newAnalogSensorName != analogSensorName)) {
                var index = analogSensorCombo.comboList.IndexOf (analogSensorName);
                analogSensorCombo.comboList[index] = newAnalogSensorName;
                analogSensorName = newAnalogSensorName;
            } else if (outcome == TouchSettingsOutcome.Added) {
                analogSensorCombo.comboList.Insert (analogSensorCombo.comboList.Count - 1, newAnalogSensorName);
                analogSensorCombo.activeText = newAnalogSensorName;
                analogSensorName = newAnalogSensorName;
                WaterLevel.AddWaterLevelSensorToWaterLevelGroup (groupName, analogSensorName);
            } else if (outcome == TouchSettingsOutcome.Deleted) {
                analogSensorCombo.comboList.Remove (analogSensorName);
                var groupsWaterLevelSensors = WaterLevel.GetAllWaterLevelSensorsForWaterLevelGroup (groupName);
                if (groupsWaterLevelSensors.Length > 0) {
                    analogSensorName = groupsWaterLevelSensors[0];
                    analogSensorCombo.activeText = analogSensorName;
                } else {
                    analogSensorName = string.Empty;
                    analogSensorCombo.activeIndex = 0;
                }
            }

            analogSensorCombo.QueueDraw ();
            GetAnalogSensorData ();
        }

        protected void OnWaterLevelSensorCalibrationButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            if (analogSensorName.IsNotEmpty ()) {
                var parent = Toplevel as Window;
                var cal = new CalibrationDialog (
                    "Water Level Sensor",
                    parent,
                    () => {
                        var channel = AquaPicSensors.WaterLevelSensors.GetSensor (analogSensorName).channel;
                        return AquaPicDrivers.AnalogInput.GetChannelValue (channel);
                    });

                cal.CalibrationCompleteEvent += (aa) => {
                    AquaPicSensors.WaterLevelSensors.SetCalibrationData (
                        analogSensorName,
                        (float)aa.zeroValue,
                        (float)aa.fullScaleActual,
                        (float)aa.fullScaleValue);
                };

                var sensor = (WaterLevelSensor)AquaPicSensors.WaterLevelSensors.GetSensor (analogSensorName);
                cal.calArgs.zeroValue = sensor.zeroScaleValue;
                cal.calArgs.fullScaleActual = sensor.fullScaleActual;
                cal.calArgs.fullScaleValue = sensor.fullScaleValue;

                cal.Run ();
            } else {
                MessageBox.Show ("Can't calibrate a none existent sensor");
            }
        }

        protected void OnFloatSwitchSettingsButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            CallFloatSwitchSettingsDialog ();
        }

        protected void OnSwitchComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New switch...") {
                CallFloatSwitchSettingsDialog (true);
            } else {
                switchName = e.activeText;
            }
            GetSwitchData ();
        }

        protected void CallFloatSwitchSettingsDialog (bool forceNew = false) {
            FloatSwitchSettings settings;
            if (switchName.IsNotEmpty () && !forceNew) {
                settings = (FloatSwitchSettings)AquaPicSensors.FloatSwitches.GetSensorSettings (switchName);
            } else {
                settings = new FloatSwitchSettings ();
            }
            var parent = Toplevel as Window;
            var s = new FloatSwitchSettingsDialog (settings, parent);
            s.Run ();
            var newSwitchName = s.floatSwitchName;
            var outcome = s.outcome;

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
                var groupsFloatSwitches = WaterLevel.GetAllFloatSwitchesForWaterLevelGroup (groupName);
                if (groupsFloatSwitches.Length > 0) {
                    switchName = groupsFloatSwitches[0];
                    switchCombo.activeText = switchName;
                } else {
                    switchName = string.Empty;
                    switchCombo.activeIndex = 0;
                }
            }

            switchCombo.QueueDraw ();
            GetSwitchData ();
        }
    }
}

