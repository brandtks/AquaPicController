#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2019 Goodtime Development

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
using AquaPic.Globals;
using AquaPic.Service;

namespace AquaPic.Gadgets.Device.Heater
{
    public class HeaterCollection : GenericDeviceCollection
    {
        public static HeaterCollection SharedHeaterCollectionInstance = new HeaterCollection ();

        public HeaterCollection () : base ("heaters") { }

        public override void ReadAllGadgetsFromFile () {
            var equipmentSettings = SettingsHelper.ReadAllSettingsInArray<HeaterSettings> (gadgetSettingsFileName, gadgetSettingsArrayName);
            foreach (var setting in equipmentSettings) {
                CreateGadget (setting, false);
            }
        }

        protected override GenericGadget GadgetCreater (GenericGadgetSettings settings) {
            var heaterSettings = settings as HeaterSettings;
            if (heaterSettings == null) {
                throw new ArgumentException ("Settings must be HeaterSettings");
            }

            return new Heater (heaterSettings);
        }

        public override GenericGadgetSettings GetGadgetSettings (string name) {
            CheckGadgetKey (name);
            var settings = new HeaterSettings ();
            var gadget = gadgets[name] as Heater;
            settings.name = gadget.name;
            settings.channel = gadget.channel;
            settings.temperatureGroup = gadget.temperatureGroup;
            return settings;
        }

        public string GetTemperatureGroup (string name) {
            CheckGadgetKey (name);
            var heater = gadgets[name] as Heater;
            return heater.temperatureGroup;
        }

        public string[] GetAllHeatersForTemperatureGroup (string temperatureGroup) {
            var heaters = new List<string> ();
            foreach (var gadget in gadgets.Values) {
                var heater = gadget as Heater;
                if (heater.temperatureGroup == temperatureGroup) {
                    heaters.Add (heater.name);
                }
            }
            return heaters.ToArray ();
        }
    }
}
