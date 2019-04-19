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
using AquaPic.Gadgets;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class WaterLevelSensorSettingsDialog : TouchSettingsDialog
    {
        public string waterLevelSensorName { get; private set; }

        public WaterLevelSensorSettingsDialog (WaterLevelSensorSettings settings, Window parent)
            : base (settings.name, settings.name.IsNotEmpty (), parent) 
        {
            waterLevelSensorName = settings.name;
            var analogSensorNameNotEmpty = waterLevelSensorName.IsNotEmpty ();

            var t = new SettingsTextBox ("Name");
            t.textBox.text = waterLevelSensorName.IsNotEmpty () ? waterLevelSensorName : "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (args.text.IsEmpty ()) {
                    args.keepText = false;
                } else if (Sensors.WaterLevelSensors.GadgetNameExists (args.text)) {
                    MessageBox.Show ("Water level sensor name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Input Channel");
            if (waterLevelSensorName.IsNotEmpty ()) {
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

            Sensors.WaterLevelSensors.UpdateSensor (waterLevelSensorName, sensorSettings);
            waterLevelSensorName = sensorSettings.name;

            return true;
        }

        protected override bool OnDelete (object sender) {
            Sensors.WaterLevelSensors.RemoveSensor (waterLevelSensorName);
            return true;
        }
    }
}

