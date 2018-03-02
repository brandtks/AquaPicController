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
    public class AtoScreen : SceneBase
    {
        uint timerId;
        TouchLabel atoStateTextBox;
        TouchLabel reservoirLevelTextBox;
        TouchButton atoClearFailBtn;

        public AtoScreen (params object[] options) : base () {
            /**************************************************************************************************************/
            /* ATO                                                                                                        */
            /**************************************************************************************************************/
            var label = new TouchLabel ();
            label.text = "Auto Top Off";
            label.WidthRequest = 329;
            label.textColor = "seca";
            label.textSize = 12;
            label.textAlignment = TouchAlignment.Right;
            Put (label, 60, 80);
            label.Show ();

            var stateLabel = new TouchLabel ();
            stateLabel.text = "ATO State";
            stateLabel.textColor = "grey3";
            stateLabel.WidthRequest = 329;
            stateLabel.textAlignment = TouchAlignment.Center;
            Put (stateLabel, 60, 155);
            stateLabel.Show ();

            atoStateTextBox = new TouchLabel ();
            atoStateTextBox.WidthRequest = 329;
            if (WaterLevel.atoEnabled) {
                atoStateTextBox.text = string.Format ("{0} : {1}",
                    WaterLevel.atoState,
                    WaterLevel.atoTime.SecondsToString ());
            } else {
                atoStateTextBox.text = "ATO Disabled";
            }
            atoStateTextBox.textSize = 20;
            atoStateTextBox.textAlignment = TouchAlignment.Center;
            Put (atoStateTextBox, 60, 120);
            atoStateTextBox.Show ();

            var reservoirLevelLabel = new TouchLabel ();
            reservoirLevelLabel.WidthRequest = 329;
            reservoirLevelLabel.text = "Reservoir Level";
            reservoirLevelLabel.textColor = "grey3";
            reservoirLevelLabel.textAlignment = TouchAlignment.Center;
            Put (reservoirLevelLabel, 60, 230);
            reservoirLevelLabel.Show ();

            reservoirLevelTextBox = new TouchLabel ();
            reservoirLevelTextBox.SetSizeRequest (329, 50);
            reservoirLevelTextBox.textSize = 20;
            reservoirLevelTextBox.textAlignment = TouchAlignment.Center;
            if (WaterLevel.atoReservoirLevelEnabled) {
                float wl = WaterLevel.atoReservoirLevel;
                if (wl < 0.0f) {
                    reservoirLevelTextBox.text = "Probe Disconnected";
                } else {
                    reservoirLevelTextBox.text = wl.ToString ("F2");
                    reservoirLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
                }
            } else {
                reservoirLevelTextBox.text = "Sensor disabled";
            }
            Put (reservoirLevelTextBox, 60, 195);
            reservoirLevelTextBox.Show ();

            var atoSettingsBtn = new TouchButton ();
            atoSettingsBtn.text = "Settings";
            atoSettingsBtn.SetSizeRequest (100, 60);
            atoSettingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new AtoSettings ();
                s.Run ();
                s.Destroy ();
                s.Dispose ();
            };
            Put (atoSettingsBtn, 290, 405);
            atoSettingsBtn.Show ();

            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 60);
            b.ButtonReleaseEvent += (o, args) => {
                if (WaterLevel.atoReservoirLevelEnabled) {
                    var cal = new CalibrationDialog (
                        "ATO Reservoir Level Sensor",
                        () => {
                            return AquaPicDrivers.AnalogInput.GetChannelValue (WaterLevel.atoReservoirLevelChannel);
                        });

                    cal.CalibrationCompleteEvent += (aa) => {
                        WaterLevel.SetAtoReservoirCalibrationData (
                            (float)aa.zeroValue,
                            (float)aa.fullScaleActual,
                            (float)aa.fullScaleValue);
                    };

                    cal.calArgs.zeroValue = WaterLevel.atoReservoirLevelSensorZeroCalibrationValue;
                    cal.calArgs.fullScaleActual = WaterLevel.atoReservoirLevelSensorFullScaleCalibrationActual;
                    cal.calArgs.fullScaleValue = WaterLevel.atoReservoirLevelSensorFullScaleCalibrationValue;

                    cal.Run ();
                    cal.Destroy ();
                    cal.Dispose ();
                } else {
                    MessageBox.Show ("ATO reservoir level sensor is disabled\n" +
                                    "Can't perfom a calibration");
                }
            };
            Put (b, 180, 405);
            b.Show ();

            atoClearFailBtn = new TouchButton ();
            atoClearFailBtn.SetSizeRequest (100, 60);
            atoClearFailBtn.text = "Reset ATO";
            atoClearFailBtn.buttonColor = "compl";
            atoClearFailBtn.ButtonReleaseEvent += (o, args) => {
                if (!WaterLevel.ClearAtoAlarm ())
                    MessageBox.Show ("Please acknowledge alarms first");
            };
            Put (atoClearFailBtn, 70, 405);
            if (Alarm.CheckAlarming (WaterLevel.atoFailedAlarmIndex)) {
                atoClearFailBtn.Visible = true;
                atoClearFailBtn.Show ();
            } else {
                atoClearFailBtn.Visible = false;
            }

            Alarm.AddAlarmHandler (WaterLevel.atoFailedAlarmIndex, OnAtoFailedAlarmEvent);
        }

        public override void Dispose () {
            Alarm.RemoveAlarmHandler (WaterLevel.atoFailedAlarmIndex, OnAtoFailedAlarmEvent);
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        public bool OnUpdateTimer () {
            if (WaterLevel.atoEnabled) {
                atoStateTextBox.text = string.Format ("{0} : {1}",
                    WaterLevel.atoState,
                    WaterLevel.atoTime.SecondsToString ());

                if (WaterLevel.atoReservoirLevelEnabled) {
                    float wl = WaterLevel.atoReservoirLevel;
                    if (wl < 0.0f) {
                        reservoirLevelTextBox.text = "Probe Disconnected";
                        reservoirLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                    } else {
                        reservoirLevelTextBox.text = wl.ToString ("F2");
                        reservoirLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
                    }
                } else {
                    reservoirLevelTextBox.text = "Sensor disabled";
                    reservoirLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                }
                reservoirLevelTextBox.QueueDraw ();

            } else {
                atoStateTextBox.text = "ATO Disabled";
            }
            atoStateTextBox.QueueDraw ();

            return true;
        }
    }
}

