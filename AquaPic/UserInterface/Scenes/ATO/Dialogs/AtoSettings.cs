#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using System.IO;
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TouchWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Utilites;
using AquaPic.Drivers;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class AtoSettings : TouchSettingsDialog
    {
        public AtoSettings () : base ("Auto Top Off") {
            SaveEvent += OnSave;

            var s = new SettingSelectorSwitch ();
            s.text = "Enable";
            if (WaterLevel.atoEnabled) {
                s.selectorSwitch.currentSelected = 0;
                showOptional = true;
            } else {
                s.selectorSwitch.currentSelected = 1;
                showOptional = false;
            }
            s.selectorSwitch.SelectorChangedEvent += (sender, args) => {
                if (args.currentSelectedIndex == 0)
                    showOptional = true;
                else
                    showOptional = false;

                UpdateSettingsVisibility ();
            };
            AddSetting (s);

            s = new SettingSelectorSwitch ();
            s.text = "Use Analog";
            if (WaterLevel.atoUseAnalogSensor)
                s.selectorSwitch.currentSelected = 0;
            else
                s.selectorSwitch.currentSelected = 1;
            AddOptionalSetting (s);

            var t = new SettingsTextBox ();
            t.text = "Analog On";
            t.textBox.text = WaterLevel.atoAnalogOnSetpoint.ToString ();
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
            AddOptionalSetting (t);

            t = new SettingsTextBox ();
            t.text = "Analog Off";
            t.textBox.text = WaterLevel.atoAnalogOffSetpoint.ToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float offStpnt = Convert.ToSingle (args.text);

                    if (offStpnt < 0.0f) {
                        MessageBox.Show ("Analog on setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }

                    float onStpnt = Convert.ToSingle (((SettingsTextBox)settings ["Analog On"]).textBox.text);

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
            AddOptionalSetting (t);

            s = new SettingSelectorSwitch ();
            s.text = "Use Float Switch";
            if (WaterLevel.atoUseFloatSwitch)
                s.selectorSwitch.currentSelected = 0;
            else
                s.selectorSwitch.currentSelected = 1;
            AddOptionalSetting (s);

            var c = new SettingsComboBox ();
            c.text = "Pump Outlet";
            string[] availOutlets = Power.GetAllAvaiblableOutlets ();
            if (WaterLevel.atoPumpOutlet.IsNotEmpty ()) {
                IndividualControl ic = WaterLevel.atoPumpOutlet;
                string pwrName = Power.GetPowerStripName (ic.Group);
                pwrName = string.Format ("{0}.p{1}", pwrName, ic.Individual);
                c.combo.comboList.Add (string.Format ("Current: {0}", pwrName));
                c.combo.activeIndex = 0;
            } else
                c.combo.nonActiveMessage = "Select outlet";
            c.combo.comboList.AddRange (availOutlets); 
            AddOptionalSetting (c);

            t = new SettingsTextBox ();
            t.text = "Max Runtime";
            t.textBox.text = string.Format ("{0} mins", WaterLevel.atoMaxRuntime / 60);
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;
                
                try {
                    int idx = args.text.IndexOf ("mins", StringComparison.InvariantCultureIgnoreCase);
                    uint time;
                    if (idx == -1)
                        time = Convert.ToUInt32 (args.text);
                    else {
                        string timeString = args.text.Substring (0, idx);
                        time = Convert.ToUInt32 (timeString);
                    }

                    args.text = string.Format ("{0} mins", time);
                } catch {
                    MessageBox.Show ("Improper format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

            t = new SettingsTextBox ();
            t.text = "Cooldown";
            t.textBox.text = string.Format ("{0} mins", WaterLevel.atoCooldown / 60);
            t.textBox.TextChangedEvent += (sender, args) => {
                if (string.IsNullOrWhiteSpace (args.text))
                    args.keepText = false;

                try {
                    int idx = args.text.IndexOf ("mins", StringComparison.InvariantCultureIgnoreCase);
                    uint time;
                    if (idx == -1)
                        time = Convert.ToUInt32 (args.text);
                    else {
                        string timeString = args.text.Substring (0, idx);
                        time = Convert.ToUInt32 (timeString);
                    }

                    args.text = string.Format ("{0} mins", time);
                } catch {
                    MessageBox.Show ("Improper format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

            s = new SettingSelectorSwitch ();
            s.text = "Enable Reservoir Level";
            if (WaterLevel.atoReservoirLevelEnabled) {
                s.selectorSwitch.currentSelected = 0;
            } else {
                s.selectorSwitch.currentSelected = 1;
            }
            AddOptionalSetting (s);

            c = new SettingsComboBox ();
            c.text = "Reservoir Channel";
            string[] availCh = AquaPicDrivers.AnalogInput.GetAllAvaiableChannels ();
            if ((WaterLevel.atoReservoirLevelEnabled) || (WaterLevel.atoReservoirLevelChannel.IsNotEmpty ())) {
                IndividualControl ic = WaterLevel.atoReservoirLevelChannel;
                string chName = AquaPicDrivers.AnalogInput.GetCardName (ic.Group);
                chName = string.Format ("{0}.i{1}", chName, ic.Individual);
                c.combo.comboList.Add (string.Format ("Current: {0}", chName));
                c.combo.activeIndex = 0;
            } else {
                c.combo.nonActiveMessage = "Select input";
            }
            c.combo.comboList.AddRange (availCh);
            AddOptionalSetting (c);

            s = new SettingSelectorSwitch ();
            s.text = "Disable on Low Reservoir Level";
            if (WaterLevel.atoReservoirDisableOnLowLevel) {
                s.selectorSwitch.currentSelected = 0;
            } else {
                s.selectorSwitch.currentSelected = 1;
            }
            AddOptionalSetting (s);

            t = new SettingsTextBox ();
            t.text = "Reservoir Low Level";
            t.textBox.text = WaterLevel.atoReservoirLowLevelSetpoint.ToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    float lowStpnt = Convert.ToSingle (args.text);

                    if (lowStpnt < 0.0f) {
                        MessageBox.Show ("Low level setpoint can't be negative");
                        args.keepText = false;
                        return;
                    }
                } catch {
                    MessageBox.Show ("Improper low level setpoint format");
                    args.keepText = false;
                }
            };
            AddOptionalSetting (t);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            bool enable;
            try {
                SettingSelectorSwitch s = settings ["Enable"] as SettingSelectorSwitch;
                if (s.selectorSwitch.currentSelected == 0)
                    enable = true;
                else
                    enable = false;
            } catch {
                return false;
            }

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "waterLevelProperties.json");

            string jstring = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (jstring);

            if (enable) {
                bool useAnalog;
                SettingSelectorSwitch s = settings ["Use Analog"] as SettingSelectorSwitch;
                if (s.selectorSwitch.currentSelected == 0)
                    useAnalog = true;
                else
                    useAnalog = false;

                WaterLevel.atoUseAnalogSensor = useAnalog;

                float analogOnStpnt = Convert.ToSingle (Convert.ToSingle (((SettingsTextBox)settings ["Analog On"]).textBox.text));

                if (analogOnStpnt < 0.0f) {
                    MessageBox.Show ("Analog on setpoint can't be negative");
                    return false;
                }

                float analogOffStpnt = Convert.ToSingle (((SettingsTextBox)settings ["Analog Off"]).textBox.text);

                if (analogOffStpnt < 0.0f) {
                    MessageBox.Show ("Analog off setpoint can't be negative");
                    return false;
                }

                if (analogOnStpnt >= analogOffStpnt) {
                    MessageBox.Show ("Analog on setpoint can't be greater than or equal to analog off setpoint");
                    return false;
                }

                WaterLevel.atoAnalogOnSetpoint = analogOnStpnt;
                WaterLevel.atoAnalogOffSetpoint = analogOffStpnt;

                bool useFloatSwitch;
                s = settings ["Use Float Switch"] as SettingSelectorSwitch;
                if (s.selectorSwitch.currentSelected == 0)
                    useFloatSwitch = true;
                else
                    useFloatSwitch = false;

                WaterLevel.atoUseAnalogSensor = useFloatSwitch;

                try {
                    if (((SettingsComboBox)settings ["Pump Outlet"]).combo.activeIndex == -1) {
                        MessageBox.Show ("Please select power outlet for the pump");
                        return false;
                    }

                    string outletString = ((SettingsComboBox)settings ["Pump Outlet"]).combo.activeText;

                    if (!outletString.StartsWith ("Current:")) {
                        int idx = outletString.IndexOf ('.');
                        string pwrName = outletString.Substring (0, idx);
                        int pwrId = Power.GetPowerStripIndex (pwrName);
                        int outletId = Convert.ToInt32 (outletString.Substring (idx + 2));

                        IndividualControl ic = IndividualControl.Empty;
                        ic.Group = pwrId;
                        ic.Individual = outletId;
                        WaterLevel.atoPumpOutlet = ic;
                    }
                } catch (Exception ex) {
                    Logger.AddError (ex.ToString ());
                    MessageBox.Show ("Something went wrong, check logger");
                    return false;
                }

                uint maxRuntime = 0;
                string timeString = ((SettingsTextBox)settings ["Max Runtime"]).textBox.text;
                int periodIdx = timeString.IndexOf ("mins", StringComparison.InvariantCultureIgnoreCase);
                if (periodIdx != -1)
                    timeString = timeString.Substring (0, periodIdx);
                maxRuntime = Convert.ToUInt32 (timeString) * 60;

                WaterLevel.atoMaxRuntime = maxRuntime;

                uint cooldown = 0;
                timeString = ((SettingsTextBox)settings ["Cooldown"]).textBox.text;
                periodIdx = timeString.IndexOf ("mins", StringComparison.InvariantCultureIgnoreCase);
                if (periodIdx != -1)
                    timeString = timeString.Substring (0, periodIdx);
                cooldown = Convert.ToUInt32 (timeString) * 60;
                WaterLevel.atoCooldown = cooldown;

                bool reservoirEnable;
                try {
                    s = settings["Enable Reservoir Level"] as SettingSelectorSwitch;
                    if (s.selectorSwitch.currentSelected == 0) {
                        reservoirEnable = true;
                    } else {
                        reservoirEnable = false;
                    }
                } catch {
                    return false;
                }

                if (reservoirEnable) {
                    try {
                        if (((SettingsComboBox)settings["Reservoir Channel"]).combo.activeIndex == -1) {
                            MessageBox.Show ("Please Select an input channel");
                            return false;
                        }

                        string text = ((SettingsComboBox)settings["Reservoir Channel"]).combo.activeText;

                        if (!text.StartsWith ("Current:")) {
                            int idx = text.IndexOf ('.');
                            string cardName = text.Substring (0, idx);
                            int cardId = AquaPicDrivers.AnalogInput.GetCardIndex (cardName);
                            int channelId = Convert.ToInt32 (text.Substring (idx + 2));

                            var ic = IndividualControl.Empty;
                            ic.Group = cardId;
                            ic.Individual = channelId;
                            WaterLevel.atoReservoirLevelChannel = ic;
                        }
                    } catch (Exception ex) {
                        Logger.AddError (ex.ToString ());
                        MessageBox.Show ("Something went wrong, check logger");
                        return false;
                    }

                    bool disableOnLow;
                    try {
                        s = settings["Disable on Low Reservoir Level"] as SettingSelectorSwitch;
                        if (s.selectorSwitch.currentSelected == 0) {
                            disableOnLow = true;
                        } else {
                            disableOnLow = false;
                        }
                    } catch {
                        return false;
                    }

                    WaterLevel.atoReservoirDisableOnLowLevel = disableOnLow;

                    float lowLevel = Convert.ToSingle (((SettingsTextBox)settings["Reservoir Low Level"]).textBox.text);
                    WaterLevel.atoReservoirLowLevelSetpoint = lowLevel;
                } else {
                    WaterLevel.atoReservoirLevelChannel = IndividualControl.Empty;
                }
            }

            //this has to be last because if for whatever reason something above this crashes we need leave the module disable
            WaterLevel.atoEnabled = enable;

            jo["AutoTopOff"]["enableAto"] = WaterLevel.atoEnabled.ToString ();
            jo["AutoTopOff"]["useAnalogSensor"] = WaterLevel.atoUseAnalogSensor.ToString ();
            jo["AutoTopOff"]["analogOnSetpoint"] = WaterLevel.atoAnalogOnSetpoint.ToString ();
            jo["AutoTopOff"]["analogOffSetpoint"] = WaterLevel.atoAnalogOffSetpoint.ToString ();
            jo["AutoTopOff"]["useFloatSwitch"] = WaterLevel.atoUseFloatSwitch.ToString ();
            if (WaterLevel.atoPumpOutlet.IsNotEmpty ()) {
                jo["AutoTopOff"]["powerStrip"] = Power.GetPowerStripName (WaterLevel.atoPumpOutlet.Group);
                jo["AutoTopOff"]["outlet"] = WaterLevel.atoPumpOutlet.Individual.ToString ();
            } else {
                jo["AutoTopOff"]["powerStrip"] = string.Empty;
                jo["AutoTopOff"]["outlet"] = string.Empty;
            }
            jo["AutoTopOff"]["maxPumpOnTime"] = string.Format ("{0:D2}:00:00", WaterLevel.atoMaxRuntime / 60);
            jo["AutoTopOff"]["minPumpOffTime"] = string.Format ("{0:D2}:00:00", WaterLevel.atoCooldown / 60);

            if (WaterLevel.atoReservoirLevelEnabled) {
                jo["AutoTopOff"]["reservoirInputCard"] = AquaPicDrivers.AnalogInput.GetCardName (WaterLevel.atoReservoirLevelChannel.Group);
                jo["AutoTopOff"]["reservoirChannel"] = WaterLevel.atoReservoirLevelChannel.Individual.ToString ();
                jo["AutoTopOff"]["reservoirLowLevelSetpoint"] = WaterLevel.atoReservoirLowLevelSetpoint.ToString ();
                jo["AutoTopOff"]["disableOnLowResevoirLevel"] = WaterLevel.atoReservoirDisableOnLowLevel.ToString ();
            } else {
                jo["AutoTopOff"]["reservoirInputCard"] = "";
                jo["AutoTopOff"]["reservoirChannel"] = "";
            }

            File.WriteAllText (path, jo.ToString ());

            return true;
        }
    }
}
