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
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class AtoSettings : TouchSettingsDialog
    {
		string groupName;
        public string atoGroupName {
            get {
                return groupName;
            }
        }

		public AtoSettings (string name, bool includeDelete)
            : base (name + " Auto Top Off", includeDelete) 
		{
			groupName = name;
              
			var t = new SettingsTextBox ("Name");
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = groupName; 
            } else {
                t.textBox.text = "Enter name";
            }
			t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                else if (!AutoTopOff.AtoGroupNameOk (args.text)) {
                    MessageBox.Show ("ATO group name already exists");
                    args.keepText = false;
                }
            };
            AddSetting (t);

			var s = new SettingsSelectorSwitch ("Enable");
			if (groupName.IsNotEmpty ()) {
				if (AutoTopOff.GetAtoGroupEnable (groupName)) {
					s.selectorSwitch.currentSelected = 0;
					showOptional = true;
				} else {
					s.selectorSwitch.currentSelected = 1;
					showOptional = false;
				}
			} 
            AddSetting (s);

			var c = new SettingsComboBox ("Water Group");
			c.combo.nonActiveMessage = "Select outlet";
			var availableGroups = WaterLevel.GetAllWaterLevelGroupNames ();
			c.combo.comboList.AddRange (availableGroups);
			if (groupName.IsNotEmpty ()) {
				var index = Array.IndexOf (availableGroups, AutoTopOff.GetAtoGroupWaterLevelGroupName (groupName));
				c.combo.activeIndex = index;                     
            }          
            AddSetting (c);

			t = new SettingsTextBox ("Request Bit Name");
            if (groupName.IsNotEmpty ()) {
				t.textBox.text = AutoTopOff.GetAtoGroupRequestBitName (groupName);
            } else {
                t.textBox.text = "Enter name";
            }
			t.textBox.TextChangedEvent += (sender, args) => {
				if (args.text.IsEmpty ()) {
					args.keepText = false;
				}
            };
            AddSetting (t);

			s = new SettingsSelectorSwitch ("Use Analog");
			if (groupName.IsNotEmpty ()) {
				if (AutoTopOff.GetAtoGroupUseAnalogSensor (groupName))
					s.selectorSwitch.currentSelected = 0;
				else
					s.selectorSwitch.currentSelected = 1;
			} else {
				s.selectorSwitch.currentSelected = 1;
			}
			AddSetting (s);

			t = new SettingsTextBox ("Analog Off");
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = AutoTopOff.GetAtoGroupAnalogOffSetpoint (groupName).ToString ();
            } else {
                t.textBox.text = "0";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float offStpnt = Convert.ToSingle (args.text);

                    if (offStpnt < 0.0f) {
                        MessageBox.Show ("Analog on setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }

                    float onStpnt = Convert.ToSingle (((SettingsTextBox)settings["Analog On"]).textBox.text);

                    if (onStpnt >= offStpnt) {
                        MessageBox.Show ("Analog on setpoint can't be greater than or equal to off setpoint");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper analog off setpoint format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

			t = new SettingsTextBox ("Analog On");
			if (groupName.IsNotEmpty ()) {
				t.textBox.text = AutoTopOff.GetAtoGroupAnalogOnSetpoint (groupName).ToString ();
			} else {
				t.textBox.text = "0";
			}
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float onStpnt = Convert.ToSingle (args.text);

                    if (onStpnt < 0.0f) {
                        MessageBox.Show ("Analog on setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }
                        
                    float offStpnt = Convert.ToSingle (((SettingsTextBox)settings ["Analog Off"]).textBox.text);

                    if (onStpnt >= offStpnt) {
                        MessageBox.Show ("Analog on setpoint can't be greater than or equal to off setpoint");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper analog on setpoint format");
                    args.keepText = false;
                }
            };
			AddSetting (t);         

			s = new SettingsSelectorSwitch ("Use Float Switch");
			if (groupName.IsNotEmpty ()) {
				if (AutoTopOff.GetAtoGroupUseFloatSwitches (groupName))
					s.selectorSwitch.currentSelected = 0;
				else
					s.selectorSwitch.currentSelected = 1;
			} else {
                s.selectorSwitch.currentSelected = 1;
            }
			AddSetting (s);

			t = new SettingsTextBox ("Max Runtime");
			if (groupName.IsNotEmpty ()) {
				t.textBox.text = string.Format ("{0} mins", AutoTopOff.GetAtoGroupMaximumRuntime (groupName));
			} else {
				t.textBox.text = "1 min";
			}
            t.textBox.TextChangedEvent += (sender, args) => {
				if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;

                var time = ParseTime (args.text);
                if (time >= 0) {
                    args.text = string.Format ("{0} mins", time);
                } else {
                    MessageBox.Show ("Improper format");
                    args.keepText = false;
                }
            };
			AddSetting (t);

			t = new SettingsTextBox ("Cooldown");
			if (groupName.IsNotEmpty ()) {
				t.textBox.text = string.Format ("{0} mins", AutoTopOff.GetAtoGroupMinimumCooldown (groupName));
			} else {
				t.textBox.text = "10 mins";
			}
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;

				var time = ParseTime (args.text);
				if (time >= 0) {
					args.text = string.Format ("{0} mins", time);
				} else {
					MessageBox.Show ("Improper format");
					args.keepText = false;
				}
            };
			AddSetting (t);

            DrawSettings ();
        }

		protected int ParseTime (string input) {
			int time = -1;

			int idx = input.IndexOf ("mins", StringComparison.InvariantCultureIgnoreCase);
			try {
				if (idx == -1)
					time = Convert.ToInt32 (input);
				else {
					string timeString = input.Substring (0, idx);
					time = Convert.ToInt32 (timeString);
				}
			} catch {
				time = -1;
			}

			return time;
		}

		protected override bool OnSave (object sender) {
			var name = (string)settings["Name"].setting;
			if (name == "Enter name") {
                MessageBox.Show ("Invalid ATO name");
                return false;
            }

            var enable = (int)settings["Enable"].setting == 0;
			var waterLevelGroup = (string)settings["Water Group"].setting;
			var requestBitName = (string)settings["Request Bit Name"].setting;
			if (requestBitName == "Enter name") {
                MessageBox.Show ("Invalid request bit name");
                return false;
            }

			var useAnalogSensors = (int)settings["Use Analog"].setting == 0;
			var analogOnSetpoint = Convert.ToSingle (settings["Analog On"].setting);
			var analogOffSetpoint = Convert.ToSingle (settings["Analog Off"].setting);

			var useFloatSwitches = (int)settings["Use Float Switch"].setting == 0;

			var maximumRuntime = (uint)ParseTime ((string)settings["Max Runtime"].setting);
			var minimumCooldown = (uint)ParseTime ((string)settings["Cooldown"].setting);
            
			var jo = SettingsHelper.OpenSettingsFile ("autoTopOffProperties") as JObject;
			var ja = jo["atoGroups"] as JArray;

			if (groupName.IsEmpty ()) {          
				AutoTopOff.AddAtoGroup (
					name,
					enable,
					requestBitName,
					waterLevelGroup,
					maximumRuntime,
					minimumCooldown,
					useAnalogSensors,
					analogOnSetpoint,
					analogOffSetpoint,
					useFloatSwitches);

				var jobj = new JObject {
					new JProperty ("name", name),
					new JProperty ("enable", enable.ToString ()),
					new JProperty ("waterLevelGroupName", waterLevelGroup),
					new JProperty ("requestBitName", requestBitName),
					new JProperty ("useAnalogSensors", useAnalogSensors.ToString ()),
					new JProperty ("analogOnSetpoint", analogOnSetpoint.ToString ()),
					new JProperty ("analogOffSetpoint", analogOffSetpoint.ToString ()),
					new JProperty ("useFloatSwitches", useFloatSwitches.ToString ()),
					new JProperty ("maximumRuntime", maximumRuntime.ToString ()),
					new JProperty ("minimumCooldown", minimumCooldown.ToString ())
				};

				ja.Add (jobj);
				groupName = name;
			} else {
				if (groupName != name) {
					AutoTopOff.SetAtoGroupName (groupName, name);
					groupName = name;
				}
				AutoTopOff.SetAtoGroupEnable (groupName, enable);
				AutoTopOff.SetAtoGroupWaterLevelGroupName (groupName, waterLevelGroup);
				AutoTopOff.SetAtoGroupRequestBitName (groupName, requestBitName);
				AutoTopOff.SetAtoGroupUseAnalogSensor (groupName, useAnalogSensors);
				AutoTopOff.SetAtoGroupAnalogOnSetpoint (groupName, analogOnSetpoint);
				AutoTopOff.SetAtoGroupAnalogOffSetpoint (groupName, analogOffSetpoint);
				AutoTopOff.SetAtoGroupUseFloatSwitches (groupName, useFloatSwitches);
				AutoTopOff.SetAtoGroupMaximumRuntime (groupName, maximumRuntime);
				AutoTopOff.SetAtoGroupMinimumCooldown (groupName, minimumCooldown);

				int arrIdx = SettingsHelper.FindSettingsInArray (ja, groupName);
                if (arrIdx == -1) {
                    MessageBox.Show ("Something went wrong");
                    return false;
                }

				ja[arrIdx]["enable"] = enable.ToString ();
				ja[arrIdx]["waterLevelGroupName"] = waterLevelGroup;
				ja[arrIdx]["requestBitName"] = requestBitName;
				ja[arrIdx]["useAnalogSensors"] = useAnalogSensors.ToString ();
				ja[arrIdx]["analogOnSetpoint"] = analogOnSetpoint.ToString ();
				ja[arrIdx]["analogOffSetpoint"] = analogOffSetpoint.ToString ();
				ja[arrIdx]["useFloatSwitches"] = useFloatSwitches.ToString ();
				ja[arrIdx]["maximumRuntime"] = maximumRuntime.ToString ();
				ja[arrIdx]["minimumCooldown"] = minimumCooldown.ToString ();            
			}

			SettingsHelper.SaveSettingsFile ("autoTopOffProperties", jo);
            return true;
        }
    }
}

