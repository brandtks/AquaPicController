using System;
using Gtk;
using MyWidgetLibrary;
using AquaPic.Runtime;
using AquaPic.Modules;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class WaterLevelWindow : WindowBase
    {
        uint timerId;
        TouchTextBox analogLevelTextBox;
        TouchTextBox switchStateTextBox;
        TouchLabel switchTypeLabel;
        TouchComboBox switchCombo;
        int switchId;

        TouchTextBox atoStateTextBox;

        public WaterLevelWindow (params object[] options) : base () {
            MyBox box1 = new MyBox (385, 395);
            Put (box1, 10, 30);
            box1.Show ();

            MyBox box2 = new MyBox (385, 193);
            Put (box2, 405, 30);
            box2.Show ();

            MyBox box3 = new MyBox (385, 192);
            Put (box3, 405, 233);
            box3.Show ();

            /**************************************************************************************************************/
            /* ATO                                                                                                        */
            /**************************************************************************************************************/
            var label = new TouchLabel ();
            label.text = "Auto Top Off";
            label.WidthRequest = 370;
            label.textColor = "pri";
            label.textSize = 12;
            Put (label, 15, 40);
            label.Show ();

            var stateLabel = new TouchLabel ();
            stateLabel.text = "ATO State";
            stateLabel.textColor = "grey4"; 
            stateLabel.WidthRequest = 170;
            Put (stateLabel, 15, 74);
            stateLabel.Show ();

            atoStateTextBox = new TouchTextBox ();
            atoStateTextBox.WidthRequest = 200;
            atoStateTextBox.text = WaterLevel.atoState.ToString ();
            Put (atoStateTextBox, 190, 70);
            atoStateTextBox.Show ();

            var atoSettingsBtn = new TouchButton ();
            atoSettingsBtn.text = "Settings";
            atoSettingsBtn.SetSizeRequest (100, 30);
            atoSettingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new AtoSettings ();
                s.Run ();
                s.Destroy ();
            };
            Put (atoSettingsBtn, 15, 390);
            atoSettingsBtn.Show ();

            /**************************************************************************************************************/
            /* Analog water sensor                                                                                        */
            /**************************************************************************************************************/
            label = new TouchLabel ();
            label.text = "Water Level Sensor";
            label.textColor = "pri";
            label.textSize = 12;
            Put (label, 413, 40);
            label.Show ();

            label = new TouchLabel ();
            label.text = "Water Level";
            label.textColor = "grey4"; 
            Put (label, 410, 74);
            label.Show ();

            analogLevelTextBox = new TouchTextBox ();
            float wl = WaterLevel.analogWaterLevel;
            if (wl < 0.0f)
                analogLevelTextBox.text = "Probe Disconnected";
            else
                analogLevelTextBox.text = wl.ToString ("F2");
            analogLevelTextBox.WidthRequest = 200;
            Put (analogLevelTextBox, 585, 70);

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Settings";
            settingsBtn.SetSizeRequest (100, 30);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new AnalogSensorSettings ();
                s.Run ();
                s.Destroy ();
            };
            Put (settingsBtn, 410, 188);
            settingsBtn.Show ();

            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 30);
            b.ButtonReleaseEvent += (o, args) => {
                var cal = new CalibrationDialog (
                    "Water Level Sensor", 
                    () => {
                        return AnalogInput.GetValue (WaterLevel.analogSensorChannel);
                    });

                cal.CalibrationCompleteEvent += (aa) => {
                    WaterLevel.SetCalibrationData (
                        (float)aa.zeroValue, 
                        (float)aa.fullScaleActual, 
                        (float)aa.fullScaleValue);
                };

                cal.Run ();
                cal.Destroy ();
            };
            Put (b, 515, 188);
            b.Show ();

            if (WaterLevel.floatSwitchCount == 0)
                switchId = -1;
            else
                switchId = 0;

            label = new TouchLabel ();
            label.text = "Probes";
            label.textColor = "pri";
            label.textSize = 12;
            Put (label, 413, 243);
            label.Show ();

            var sLabel = new TouchLabel ();
            sLabel.text = "Current State";
            sLabel.WidthRequest = 200;
            Put (sLabel, 410, 282);
            sLabel.Show ();

            switchStateTextBox = new TouchTextBox ();
            switchStateTextBox.WidthRequest = 200;
            Put (switchStateTextBox, 585, 278);
            switchStateTextBox.Show ();

            //Type
            switchTypeLabel = new TouchLabel ();
            switchTypeLabel.WidthRequest = 198;
            switchTypeLabel.textAlignment = MyAlignment.Right;
            Put (switchTypeLabel, 585, 308);
            switchTypeLabel.Show ();

            var switchSetupBtn = new TouchButton ();
            switchSetupBtn.text = "Probe Setup";
            switchSetupBtn.SetSizeRequest (100, 30);
            switchSetupBtn.ButtonReleaseEvent += (o, args) => {
                if (switchId != -1) {
                    string name = WaterLevel.GetFloatSwitchName (switchId);
                    var s = new SwitchSettings (name, switchId, true);
                    s.Run ();
                    s.Destroy ();

                    try {
                        WaterLevel.GetFloatSwitchIndex (name);
                    } catch (ArgumentException) {
                        switchCombo.List.Remove (name);
                        if (WaterLevel.floatSwitchCount != 0) {
                            switchId = 0;
                            switchCombo.Active = switchId;
                        } else {
                            switchId = -1;
                            switchCombo.Active = 0;
                        }

                        GetSwitchData ();
                    }
                } else {
                    int switchCount = WaterLevel.floatSwitchCount;

                    var s = new SwitchSettings ("New switch", -1, false);
                    s.Run ();
                    s.Destroy ();

                    if (WaterLevel.floatSwitchCount > switchCount) {
                        switchId = WaterLevel.floatSwitchCount - 1;
                        int listIdx = switchCombo.List.IndexOf ("New switch...");
                        switchCombo.List.Insert (listIdx, WaterLevel.GetFloatSwitchName (switchId));
                        switchCombo.Active = listIdx;
                        switchCombo.QueueDraw ();
                        GetSwitchData ();
                    } else {
                        if (switchId != -1)
                            switchCombo.Active = switchId;
                        else
                            switchCombo.Active = 0;
                    }
                }

                switchCombo.QueueDraw ();
            };
            Put (switchSetupBtn, 410, 390);
            switchSetupBtn.Show ();

            string[] sNames = WaterLevel.GetAllFloatSwitches ();
            switchCombo = new TouchComboBox (sNames);
            if (switchId != -1)
                switchCombo.Active = switchId;
            else
                switchCombo.Active = 0;
            switchCombo.WidthRequest = 235;
            switchCombo.List.Add ("New switch...");
            switchCombo.ChangedEvent += OnSwitchComboChanged;
            Put (switchCombo, 550, 238);
            switchCombo.Show ();

            GetSwitchData ();

            timerId = GLib.Timeout.Add (1000, OnUpdateTimer);

            ShowAll ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        public bool OnUpdateTimer () {
            if (WaterLevel.analogSensorEnabled) {
                float wl = WaterLevel.analogWaterLevel;
                if (wl < 0.0f)
                    analogLevelTextBox.text = "Probe Disconnected";
                else
                    analogLevelTextBox.text = wl.ToString ("F2");
                analogLevelTextBox.QueueDraw ();
            }

            if (switchId != -1) {
                bool state = DigitalInput.GetState (WaterLevel.GetFloatSwitchIndividualControl (switchId));
                if (state) {
                    switchStateTextBox.text = "Closed";
                    switchStateTextBox.bkgndColor = "pri";
                } else {
                    switchStateTextBox.text = "Open";
                    switchStateTextBox.bkgndColor = "seca";
                }
            }

            return true;
        }

        protected void OnSwitchComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New switch...") {
                int switchCount = WaterLevel.floatSwitchCount;

                var s = new SwitchSettings ("New switch", -1, false);
                s.Run ();
                s.Destroy ();

                if (WaterLevel.floatSwitchCount > switchCount) {
                    switchId = WaterLevel.floatSwitchCount - 1;
                    int listIdx = switchCombo.List.IndexOf ("New switch...");
                    switchCombo.List.Insert (listIdx, WaterLevel.GetFloatSwitchName (switchId));
                    switchCombo.Active = listIdx;
                    switchCombo.QueueDraw ();
                    GetSwitchData ();
                } else {
                    if (switchId != -1)
                        switchCombo.Active = switchId;
                    else
                        switchCombo.Active = 0;
                }
            } else {
                try {
                    int id = WaterLevel.GetFloatSwitchIndex (e.ActiveText);
                    switchId = id;
                    GetSwitchData ();
                } catch {
                    ;
                }
            }
        }

        protected void GetSwitchData () {
            if (switchId != -1) {
                bool state = DigitalInput.GetState (WaterLevel.GetFloatSwitchIndividualControl (switchId));

                if (state) {
                    switchStateTextBox.text = "Closed";
                    switchStateTextBox.bkgndColor = "pri";
                } else {
                    switchStateTextBox.text = "Open";
                    switchStateTextBox.bkgndColor = "seca";
                }

                SwitchType type = WaterLevel.GetFloatSwitchType (switchId);
                switchTypeLabel.text = Utilites.Utils.GetDescription (type);
            } else {
                switchTypeLabel.Visible = false;
                switchStateTextBox.text = string.Empty;
                switchStateTextBox.bkgndColor = "grey4";
            }

            switchTypeLabel.QueueDraw ();
            switchStateTextBox.QueueDraw ();
        }

        protected double GetCalibrationValue () {
            return AnalogInput.GetValue (WaterLevel.analogSensorChannel);
        }
    }
}

