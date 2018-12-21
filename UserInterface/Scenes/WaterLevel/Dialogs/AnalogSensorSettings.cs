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
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Sensors;
using AquaPic.Modules;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class AnalogSensorSettings : TouchSettingsDialog
    {
        public string analogSensorName { get; private set; }
        string waterLevelGroupName;

        public AnalogSensorSettings (string waterLevelGroupName, WaterLevelSensorSettings settings, Window parent)
            : base (settings.name, settings.name.IsNotEmpty (), parent) 
        {
            analogSensorName = settings.name;
            var analogSensorNameNotEmpty = analogSensorName.IsNotEmpty ();
            this.waterLevelGroupName = waterLevelGroupName;

            var t = new SettingsTextBox ("Name");
            t.textBox.text = analogSensorName.IsNotEmpty () ? analogSensorName : "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (args.text.IsEmpty ())
                    args.keepText = false;
                else if (AquaPicSensors.WaterLevelSensors.SensorNameExists (args.text)) {
                    MessageBox.Show ("Switch name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Input Channel");
            if (analogSensorName.IsNotEmpty ()) {
                var ic = settings.channel;
                c.combo.comboList.Add (string.Format ("Current: {0}.i{1}", ic.Group, ic.Individual));
                c.combo.activeIndex = 0;
            }
            c.combo.nonActiveMessage = "Select outlet";
            c.combo.comboList.AddRange (AquaPicDrivers.AnalogInput.GetAllAvaiableChannels ());
            AddSetting (c);

            DrawSettings ();
        }

        protected override bool OnSave (object sender) {
            var sensorSettings = new WaterLevelSensorSettings ();

            sensorSettings.name = (string)settings["Name"].setting;
            if (sensorSettings.name == "Enter name") {
                MessageBox.Show ("Invalid probe name");
                return false;
            }

            string channelString = (string)settings["Input Channel"].setting;
            if (channelString.IsEmpty ()) {
                MessageBox.Show ("Please select an channel");
                return false;
            }
            sensorSettings.channel = ParseIndividualControl (channelString);

            WaterLevel.UpdateWaterLevelSensorInWaterLevelGroup (waterLevelGroupName, analogSensorName, sensorSettings);
            analogSensorName = sensorSettings.name;

            return true;
        }

        protected override bool OnDelete (object sender) {
            WaterLevel.RemoveWaterLevelSensorFromWaterLevelGroup (waterLevelGroupName, analogSensorName);
            return true;
        }
    }
}

