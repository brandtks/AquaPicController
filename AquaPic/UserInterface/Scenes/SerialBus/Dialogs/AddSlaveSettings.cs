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
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Gtk;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public class AddSlaveSettings : TouchSettingsDialog
    {
        public AddSlaveSettings ()
            : base ("Add Slave Module", false, 400)
        {
            SaveEvent += OnSave;

            var c = new SettingsComboBox ();
            c.text = "Type";
            c.combo.comboList.AddRange (new string[] {
                "Power",
                "Analog Input",
                "Analog Output",
                "Digital Input",
                "pH/ORP"
            });
            c.combo.nonActiveMessage = "Select slave type";
            AddSetting (c);

            var t = new SettingsTextBox ();
            t.text = "Address";
            t.textBox.text = "Enter AquaPicBus Address";
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text)) {
                    args.keepText = false;
                }  else {
                    try {
                        int address;
                        if (args.text.StartsWith ("x", StringComparison.InvariantCultureIgnoreCase)) {
                            var parseString = args.text.Substring (args.text.IndexOf ("x", StringComparison.InvariantCultureIgnoreCase) + 1); 
                            address = int.Parse (parseString, NumberStyles.HexNumber);
                        } else {
                            address = Convert.ToInt32 (args.text);
                        }

                        args.text = string.Format ("0x{0:X}, {1}", address, address);
                    } catch {
                        MessageBox.Show ("Improper address");
                        args.keepText = false;
                    }
                }
            };
            AddSetting (t);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            var typeNumber = (settings["Type"] as SettingsComboBox).combo.activeIndex;
            if (typeNumber == -1) {
                MessageBox.Show ("Please select a slave type");
                return false;
            }

            int address;
            try {
                var addressString = (settings["Address"] as SettingsTextBox).textBox.text;
                var index = addressString.IndexOf (',');
                addressString = addressString.Substring (index + 1);
                address = Convert.ToInt32 (addressString);
            } catch {
                MessageBox.Show ("Improper slave address");
                return false;
            }

            if (!SerialBus.AquaPicBus.IsAddressOk (address)) {
                MessageBox.Show ("Address already in use");
                return false;
            }

            var type = string.Empty;
            var name = string.Empty;
            var optionTokens = new List<string> ();

            switch (typeNumber) {
            case 0:
                type = "power";
                name = string.Format ("PS{0}", Power.powerStripCount + 1);
                bool alarmOnPowerLoss = false;

                var parent = Toplevel as Window;
                if (parent != null) {
                    if (!parent.IsTopLevel)
                        parent = null;
                }

                var dialog = new TouchDialog ("Alarm on power loss", parent);
                dialog.Response += (o, a) => {
                    if (a.ResponseId == ResponseType.Yes) {
                        alarmOnPowerLoss = true;
                    } else {
                        alarmOnPowerLoss = false;
                    }
                };

                dialog.Run ();
                dialog.Destroy ();

                optionTokens.Add (alarmOnPowerLoss.ToString ());

                Power.AddPowerStrip (address, name, alarmOnPowerLoss);
                break;
            case 1:
                type = "analogInput";
                name = string.Format ("AI{0}", AquaPicDrivers.AnalogInput.cardCount + 1);
                AquaPicDrivers.AnalogInput.AddCard (address, name);
                break;
            case 2:
                type = "analogOutput";
                name = string.Format ("AQ{0}", AquaPicDrivers.AnalogOutput.cardCount + 1);
                AquaPicDrivers.AnalogOutput.AddCard (address, name);
                break;
            case 3:
                type = "digitalInput";
                name = string.Format ("DI{0}", AquaPicDrivers.DigitalInput.cardCount + 1);
                AquaPicDrivers.DigitalInput.AddCard (address, name);
                break;
            case 4:
                type = "phOrp";
                name = string.Format ("PH{0}", AquaPicDrivers.DigitalInput.cardCount + 1);
                AquaPicDrivers.PhOrp.AddCard (address, name);
                break;
            }

            var jo = new JObject ();
            jo.Add (new JProperty ("type", type));

            var jao = new JArray ();
            jao.Add (string.Format ("0x{0:X}", address));
            jao.Add (name);

            foreach (var jt in optionTokens) {
                jao.Add (jt);
            }

            jo.Add (new JProperty ("options", jao));

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "equipment.json");

            string jstring = File.ReadAllText (path);
            var ja = (JArray)JToken.Parse (jstring);

            ja.Add (jo);

            File.WriteAllText (path, ja.ToString ());

            return true;
        }
    }
}

