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
    public class SensorWidget : Fixed
    {
        public string sensorTypeLabel { get; protected set; }
        public string sensorName { get; set; }
        public TouchComboBox sensorCombo { get; protected set; }
        public TouchLabel sensorStateTextbox { get; protected set; }
        public TouchLabel sensorLabel { get; protected set; }
        public GenericSensorCollection sensorCollection { get; protected set; }
        public Type settingsType { get; protected set; }

        public SensorWidget (
            string sensorTypeLabel,
            GenericSensorCollection sensorCollection,
            Type settingsType
        ) {
            this.sensorCollection = sensorCollection;
            if (!settingsType.TypeIs (typeof (GenericSensorSettings))) {
                throw new ArgumentException ("The settings type must derive GenericSensorSettings", nameof (settingsType));
            }
            this.settingsType = settingsType;

            SetSizeRequest (370, 188);

            var label = new TouchLabel ();
            label.text = sensorTypeLabel;
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 0, 3);
            label.Show ();

            var sensorSettingsButton = new TouchButton ();
            sensorSettingsButton.text = "\u2699";
            sensorSettingsButton.SetSizeRequest (30, 30);
            sensorSettingsButton.buttonColor = "pri";
            sensorSettingsButton.ButtonReleaseEvent += OnSensorSettingsButtonReleaseEvent;
            Put (sensorSettingsButton, 340, 0);
            sensorSettingsButton.Show ();

            sensorLabel = new TouchLabel ();
            sensorLabel.text = sensorTypeLabel;
            sensorLabel.textAlignment = TouchAlignment.Center;
            sensorLabel.textColor = "grey3";
            sensorLabel.WidthRequest = 370;
            Put (sensorLabel, 0, 78);
            sensorLabel.Show ();

            sensorStateTextbox = new TouchLabel ();
            sensorStateTextbox.WidthRequest = 370;
            sensorStateTextbox.textSize = 20;
            sensorStateTextbox.textAlignment = TouchAlignment.Center;
            sensorStateTextbox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
            Put (sensorStateTextbox, 0, 43);
            sensorStateTextbox.Show ();

            sensorCombo = new TouchComboBox ();
            sensorCombo.WidthRequest = 200;
            var allSensors = sensorCollection.GetAllGadgetNames ();
            if (allSensors.Length > 0) {
                sensorCombo.comboList.AddRange (allSensors);
                sensorName = allSensors[0];
            } else {
                sensorName = string.Empty;
            }
            sensorCombo.comboList.Add ("New sensor...");
            sensorCombo.activeIndex = 0;
            sensorCombo.ComboChangedEvent += OnSensorComboChanged;
            Put (sensorCombo, 135, 0);
            sensorCombo.Show ();

            GetSensorData ();

            Show ();
        }

        public virtual void GetSensorData () => throw new NotImplementedException ();

        protected void OnSensorComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New sensor...") {
                CallSensorSettingsDialog (true);
            } else {
                sensorName = e.activeText;
            }

            sensorCombo.QueueDraw ();
            GetSensorData ();
        }

        protected void OnSensorSettingsButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            CallSensorSettingsDialog ();
        }

        protected void CallSensorSettingsDialog (bool forceNew = false) {
            GenericAnalogSensorSettings settings;
            if (sensorName.IsNotEmpty () && !forceNew) {
                settings = sensorCollection.GetGadgetSettings (sensorName) as GenericAnalogSensorSettings;
            } else {
                settings = Activator.CreateInstance (settingsType) as GenericAnalogSensorSettings;
            }

            var s = new SensorSettingsDialog (settings, sensorCollection, Toplevel as Window);
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
    }
}
