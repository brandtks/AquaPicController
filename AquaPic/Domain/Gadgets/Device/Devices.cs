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
using Newtonsoft.Json.Linq;
using AquaPic.Runtime;
using AquaPic.Gadgets.Device.Pump;

namespace AquaPic.Gadgets.Device
{
    public class Devices
    {
        public static PumpCollection Pumps = PumpCollection.SharedPumpCollectionInstance;

        public static void AddDevices () {
            if (SettingsHelper.SettingsFileExists (GenericDeviceCollection.equipmentSettingsFileName)) {
                Pumps.ReadAllGadgetsFromFile ();
            } else {
                Logger.Add ("Sensors settings file did not exist, created new water level settings");

                var jo = new JObject ();
                jo.Add (new JProperty (Pumps.gadgetSettingsArrayName, new JArray ()));

                SettingsHelper.WriteSettingsFile (GenericDeviceCollection.equipmentSettingsFileName, jo);
            }
        }
    }
}
