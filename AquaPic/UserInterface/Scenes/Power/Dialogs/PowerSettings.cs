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
using System.Globalization;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;
using AquaPic.Drivers;
using AquaPic.SerialBus;

namespace AquaPic.UserInterface
{
    public class PowerSettings : TouchSettingsDialog
	{
		string powerStripName;

		public PowerSettings (string powerStripName, bool includeDelete = false) 
			: base ("New Power Strip", includeDelete) 
		{
			this.powerStripName = powerStripName;

			if (powerStripName.IsEmpty ()) {            
				var t = new SettingsTextBox ("Address");
				t.textBox.text = "Enter Address";
				t.textBox.TextChangedEvent += (sender, args) => {
					if (string.IsNullOrWhiteSpace (args.text)) {
						args.keepText = false;
					} else {
						try {
							int address;
							if ((args.text.StartsWith ("x", StringComparison.InvariantCultureIgnoreCase)) ||
								(args.text.StartsWith ("0x", StringComparison.InvariantCultureIgnoreCase))) {
								var parseString = args.text.Substring (args.text.IndexOf ("x", StringComparison.InvariantCultureIgnoreCase) + 1);
								address = int.Parse (parseString, NumberStyles.HexNumber);
							} else {
								address = Convert.ToInt32 (args.text);
							}

							if (!AquaPicBus.SlaveAddressOk (address)) {
								MessageBox.Show ("Address already exists");
								args.keepText = false;
							} else {
								args.text = string.Format ("0x{0:X}, {1}", address, address);
							}
						} catch {
							MessageBox.Show ("Improper address");
							args.keepText = false;
						}
					}
				};
				AddSetting (t);
			} else {
				Title = string.Format ("{0} Settings", powerStripName);
			}

			var s = new SettingsSelectorSwitch ("Power Loss Alarm", "Yes", "No");
            AddSetting (s);

            DrawSettings ();
        }      
        
		protected override bool OnSave (object sender) {
			var alarmOnPowerLoss = (int)settings["Power Loss Alarm"].setting == 0;

			var ja = SettingsHelper.OpenSettingsFile ("equipment") as JArray;

			if (powerStripName.IsEmpty ()) {            
				var addressString = (string)settings["Address"].setting;
				if (addressString == "Enter AquaPicBus Address") {
					MessageBox.Show ("Invalid address");
					return false;
				}
				var address = Convert.ToInt32 (addressString.Substring (addressString.IndexOf (",", StringComparison.InvariantCultureIgnoreCase) + 2));
				var name = string.Format ("PS{0}", Power.powerStripCount + 1);

				Power.AddPowerStrip (name, address, alarmOnPowerLoss);        
                            
				var jo = new JObject {
					new JProperty ("type", "power"),
					new JProperty ("address", string.Format ("0x{0:X}", address)),
                    new JProperty ("name ", name)
                };
				var jao = new JArray ();
                jao.Add (alarmOnPowerLoss.ToString ());
				jo.Add (new JProperty ("options", jao));

				ja.Add (jo);
			} else {
				Power.SetPowerStripAlarmOnPowerLoss (powerStripName, alarmOnPowerLoss);
				var index = SettingsHelper.FindSettingsInArray (ja, powerStripName);
				if (index == -1) {
					MessageBox.Show ("Something went wrong");
                    return false;
                }
				var jao = (JArray)ja[index]["options"];
				jao[0] = alarmOnPowerLoss.ToString ();
			}

			SettingsHelper.SaveSettingsFile ("equipment", ja);
            return true;
        }

		protected override bool OnDelete (object sender) {
			var ja = SettingsHelper.OpenSettingsFile ("equipment") as JArray;
			var index = SettingsHelper.FindSettingsInArray (ja, powerStripName);
            if (index == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }
			ja.RemoveAt (index);
			SettingsHelper.SaveSettingsFile ("equipment", ja);
			Power.RemovePowerStrip (powerStripName);
			return true;
		}
	}
}

