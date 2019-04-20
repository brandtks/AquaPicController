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
using AquaPic.Runtime;

namespace AquaPic.Gadgets.Device.Pumps
{
    public class PumpCollection : GenericEquipmentCollection
    {
        public static PumpCollection SharedPumpCollectionInstance = new PumpCollection ();

        protected PumpCollection () : base ("pumps") { }

        public override void ReadAllGadgetsFromFile () {
            var equipmentSettings = SettingsHelper.ReadAllSettingsInArray<PumpSettings> (gadgetSettingsFileName, gadgetSettingsArrayName);
            foreach (var setting in equipmentSettings) {
                CreateGadget (setting, false);
            }
        }

        protected override GenericGadget GadgetCreater (GenericGadgetSettings settings) {
            var pumpSettings = settings as PumpSettings;
            if (pumpSettings == null) {
                throw new ArgumentException ("Settings must be PumpSettings");
            }
            var pump = new Pump (pumpSettings);
            return pump;
        }
    }
}
