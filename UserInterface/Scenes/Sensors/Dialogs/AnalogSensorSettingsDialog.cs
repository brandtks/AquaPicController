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
    public class AnalogSensorSettingsDialog : SensorSettingsDialog
    {
        public AnalogSensorSettingsDialog (
            GenericAnalogSensorSettings settings,
            GenericAnalogSensorCollection sensorCollection,
            GenericAnalogInputBase analogInputDriver, 
            Window parent)
            : base (settings, sensorCollection, analogInputDriver, parent) 
        {
            var t = new SettingsTextBox ("LPF Factor");
            t.textBox.text = sensorName.IsNotEmpty () ? settings.lowPassFilterFactor.ToString () : "5";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    var factor = Convert.ToInt32 (args.text);

                    if (factor < 0) {
                        MessageBox.Show ("Low passs filter factor can't be negative");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper Low passs filter factor format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            DrawSettings ();
        }

        protected override bool OnSave (object sender) {
            var sensorSettings = new GenericAnalogSensorSettings ();

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

            sensorSettings.lowPassFilterFactor = Convert.ToInt32 (settings["LPF Factor"].setting);

            sensorCollection.UpdateSensor (sensorName, sensorSettings);
            sensorName = sensorSettings.name;

            return true;
        }

        protected override bool OnDelete (object sender) {
            sensorCollection.RemoveSensor (sensorName);
            return true;
        }
    }
}
