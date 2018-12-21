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
using AquaPic.Modules;
using AquaPic.Drivers;
using AquaPic.Sensors;

namespace AquaPic.UserInterface
{
    public class SwitchSettings : TouchSettingsDialog
    {
        public string switchName { get; private set; }
        string waterLevelGroupName;

        public SwitchSettings (string waterLevelGroupName, FloatSwitchSettings settings, Window parent)
            : base (settings.name, settings.name.IsNotEmpty (), parent) 
        {
            this.waterLevelGroupName = waterLevelGroupName;
            switchName = settings.name;

            var t = new SettingsTextBox ("Name");
            t.textBox.text = switchName.IsNotEmpty () ? switchName : "Enter name";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (args.text.IsEmpty ())
                    args.keepText = false;
                else if (AquaPicSensors.FloatSwitches.SensorNameExists (args.text)) {
                    MessageBox.Show ("Switch name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            var c = new SettingsComboBox ("Input");
            if (switchName.IsNotEmpty ()) {
                var ic = settings.channel;
                c.combo.comboList.Add (string.Format ("Current: {0}.i{1}", ic.Group, ic.Individual));
                c.combo.activeIndex = 0;
            }
            c.combo.nonActiveMessage = "Please select channel";
            c.combo.comboList.AddRange (AquaPicDrivers.DigitalInput.GetAllAvaiableChannels ());
            AddSetting (c);

            t = new SettingsTextBox ("Physical Level");
            t.textBox.text = switchName.IsNotEmpty () ? settings.physicalLevel.ToString () : "0";
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    var physicalLevel = Convert.ToSingle (args.text);

                    if (physicalLevel <= 0f) {
                        MessageBox.Show ("Physical level can not be less than or equal to 0");
                        args.keepText = false;
                    }

                } catch {
                    MessageBox.Show ("Improper number format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            c = new SettingsComboBox ("Type");
            var types = Enum.GetNames (typeof (SwitchType));
            c.combo.comboList.AddRange (types);
            c.combo.nonActiveMessage = "Please select type";
            if (switchName.IsNotEmpty ()) {
                c.combo.activeText = settings.switchType.ToString ();
            }
            AddSetting (c);

            c = new SettingsComboBox ("Function");
            var functions = Enum.GetNames (typeof (SwitchFunction));
            c.combo.comboList.AddRange (functions);
            c.combo.comboList.Remove ("None");
            c.combo.nonActiveMessage = "Please select function";
            if (switchName.IsNotEmpty ()) {
                c.combo.activeText = settings.switchFuntion.ToString ();
            }
            AddSetting (c);

            t = new SettingsTextBox ("Time Offset");
            t.textBox.text = switchName.IsNotEmpty () ? 
                string.Format ("{0} secs", settings.timeOffset / 1000) :
                "Enter time";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (args.text.IsEmpty ())
                    args.keepText = false;

                var time = ParseTime (args.text);
                if (time >= 0) {
                    args.text = string.Format ("{0} secs", time);
                } else {
                    MessageBox.Show ("Improper format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            DrawSettings ();
        }

        protected int ParseTime (string input) {
            int time;

            try {
                int idx = input.IndexOf ("secs", StringComparison.InvariantCultureIgnoreCase);
                if (idx == -1)
                    time = Convert.ToInt32 (input);
                else {
                    var timeString = input.Substring (0, idx);
                    time = Convert.ToInt32 (timeString);
                }
            } catch {
                time = -1;
            }

            return time;
        }

        protected override bool OnSave (object sender) {
            var switchSettings = new FloatSwitchSettings ();

            switchSettings.name = (string)settings["Name"].setting;
            if (switchSettings.name == "Enter name") {
                MessageBox.Show ("Invalid probe name");
                return false;
            }

            string channelString = (string)settings["Input"].setting;
            if (channelString.IsEmpty ()) {
                MessageBox.Show ("Please select an channel");
                return false;
            }
            switchSettings.channel = ParseIndividualControl (channelString);

            switchSettings.physicalLevel = Convert.ToSingle (settings["Physical Level"].setting);

            string typeString = (string)settings["Type"].setting;
            if (typeString.IsNotEmpty ()) {
                switchSettings.switchType = (SwitchType)Enum.Parse (typeof (SwitchType), typeString);
            } else {
                MessageBox.Show ("Please select switch type");
                return false;
            }

            string functionString = (string)settings["Function"].setting;
            if (functionString.IsNotEmpty ()) {
                switchSettings.switchFuntion = (SwitchFunction)Enum.Parse (typeof (SwitchFunction), functionString);
            } else {
                MessageBox.Show ("Please select switch function");
                return false;
            }

            if (!CheckFunctionAgainstType (switchSettings.switchFuntion, switchSettings.switchType)) {
                return false;
            }

            var timeOffsetString = (string)settings["Time Offset"].setting;
            if (timeOffsetString == "Enter time") {
                MessageBox.Show ("Please enter delay time for float switch");
                return false;
            }
            switchSettings.timeOffset = (uint)ParseTime (timeOffsetString);

            WaterLevel.UpdateFloatSwitchInWaterLevelGroup (waterLevelGroupName, switchName, switchSettings);
            switchName = switchSettings.name;

            return true;
        }

        protected override bool OnDelete (object sender) {
            WaterLevel.RemoveFloatSwitchFromWaterLevelGroup (waterLevelGroupName, switchName);
            return true;
        }

        protected bool CheckFunctionAgainstType (SwitchFunction function, SwitchType type) {
            var message = string.Empty;
            var confirmed = true;

            if ((function == SwitchFunction.HighLevel) && (type != SwitchType.NormallyClosed)) {
                message = "High level switch should be normally closed";
            } else if ((function == SwitchFunction.LowLevel) && (type != SwitchType.NormallyClosed)) {
                message = "Low level switch should be normally closed";
            } else if ((function == SwitchFunction.ATO) && (type != SwitchType.NormallyOpened)) {
                message = "ATO switch should be normally opened";
            }

            if (message.IsNotEmpty ()) {
                var parent = Toplevel as Window;
                var ms = new TouchDialog (message + "\n" +
                    "Are you sure you want to use this configuration", parent);

                ms.Response += (o, a) => {
                    if (a.ResponseId == ResponseType.Yes) {
                        confirmed = true;
                    }
                };

                ms.Run ();
                ms.Destroy ();
            }

            return confirmed;
        }
    }
}

