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
using AquaPic.Gadgets.Sensor;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class AnalogSensorWidget : SensorWidget
    {
        public GenericAnalogInputBase analogInputDriver { get; protected set; }

        public AnalogSensorWidget (
            string sensorTypeLabel, 
            GenericAnalogSensorCollection sensorCollection,
            GenericAnalogInputBase analogInputDriver,
            Type settingsType
        ) : base (sensorTypeLabel, sensorCollection, settingsType) {
            this.analogInputDriver = analogInputDriver;
            if (!settingsType.TypeIs (typeof (GenericAnalogSensorSettings))) {
                throw new ArgumentException ("The settings type must derive GenericAnalogSensorSettings", nameof (settingsType));
            }

            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 60);
            b.ButtonReleaseEvent += OnCalibrationButtonReleaseEvent;
            Put (b, 0, 118);
            b.Show (); 

            GetSensorData ();

            Show ();
        }

        protected override void CallSensorSettingsDialog (bool forceNew = false) {
            GenericAnalogSensorSettings settings;
            if (sensorName.IsNotEmpty () && !forceNew) {
                settings = sensorCollection.GetGadgetSettings (sensorName) as GenericAnalogSensorSettings;
            } else {
                settings = Activator.CreateInstance (settingsType) as GenericAnalogSensorSettings;
            }

            var analogSensorCollection = (GenericAnalogSensorCollection)sensorCollection;
            var s = new AnalogSensorSettingsDialog (settings, analogSensorCollection, analogInputDriver, Toplevel as Window);
            s.Run ();
            var newProbeName = s.sensorName;
            var outcome = s.outcome;

            if ((outcome == TouchSettingsOutcome.Modified) && (newProbeName != sensorName)) {
                var index = sensorCombo.comboList.IndexOf (sensorName);
                sensorCombo.comboList[index] = newProbeName;
                sensorName = newProbeName;
            } else if (outcome == TouchSettingsOutcome.Added) {
                sensorCombo.comboList.Insert (sensorCombo.comboList.Count - 1, newProbeName);
                sensorCombo.activeText = newProbeName;
                sensorName = newProbeName;
            } else if (outcome == TouchSettingsOutcome.Deleted) {
                sensorCombo.comboList.Remove (sensorName);
                var allSensors = sensorCollection.GetAllGadgetNames ();
                if (allSensors.Length > 0) {
                    sensorName = allSensors[0];
                    sensorCombo.activeText = sensorName;
                } else {
                    sensorName = string.Empty;
                    sensorCombo.activeIndex = 0;
                }
            }

            sensorCombo.QueueDraw ();
            GetSensorData ();
        }

        protected void OnCalibrationButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            if (sensorName.IsNotEmpty ()) {
                var parent = Toplevel as Window;
                var cal = new CalibrationDialog (
                    sensorTypeLabel + " Calibration",
                    parent,
                    () => {
                        var channel = sensorCollection.GetGadget (sensorName).channel;
                        return analogInputDriver.GetChannelValue (channel);
                    });

                var analogSensorCollection = (GenericAnalogSensorCollection)sensorCollection;
                cal.CalibrationCompleteEvent += (a) => {
                    analogSensorCollection.SetCalibrationData (
                        sensorName,
                        (float)a.zeroActual,
                        (float)a.zeroValue,
                        (float)a.fullScaleActual,
                        (float)a.fullScaleValue);
                };

                var probe = (GenericAnalogSensor)sensorCollection.GetGadget (sensorName);
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
