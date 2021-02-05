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
using AquaPic.Gadgets.Sensor;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class SensorSettingsDialog : TouchSettingsDialog {
        public string sensorName { get; protected set; }
        protected GenericSensorCollection sensorCollection;

        public SensorSettingsDialog (
            GenericSensorSettings settings,
            GenericSensorCollection sensorCollection,
            Window parent
        ) : base (settings.name, settings.name.IsNotEmpty (), parent) {
            sensorName = settings.name;
            this.sensorCollection = sensorCollection;

            var t = new SettingsTextBox ("Name");
            t.textBox.text = sensorName.IsNotEmpty () ? sensorName : "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (args.text.IsEmpty ()) {
                    args.keepText = false;
                } else if (Sensors.PhProbes.GadgetNameExists (args.text)) {
                    MessageBox.Show ("Probe name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Input Channel");
            if (sensorName.IsNotEmpty ()) {
                var ic = settings.channel;
                c.combo.comboList.Add (string.Format ("Current: {0}.i{1}", ic.Group, ic.Individual));
                c.combo.activeIndex = 0;
            }
            c.combo.nonActiveMessage = "Please select channel";
            AddSetting (c);

            DrawSettings ();
        }

        protected virtual string[] GetAvailableChannels () => throw new NotImplementedException ();

        protected virtual void UpdateAvailableChannels () {
            var comboSetting = settings["Input Channel"] as SettingsComboBox;
            comboSetting.combo.comboList.AddRange (GetAvailableChannels ());
        }

        protected override bool OnSave (object sender) {
            var sensorSettings = new GenericSensorSettings ();

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
