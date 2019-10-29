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
using AquaPic.Service;

namespace AquaPic.Gadgets.Sensor
{
    public class SwitchCollection : GenericSensorCollection
    {
        public static SwitchCollection SharedSwitchCollectionInstance = new SwitchCollection ();

        protected SwitchCollection () : base ("switches") { }

        public override void ReadAllGadgetsFromFile () {
            var switchSettings = SettingsHelper.ReadAllSettingsInArray<SwitchSettings> (gadgetSettingsFileName, gadgetSettingsArrayName);
            foreach (var setting in switchSettings) {
                CreateGadget (setting, false);
            }
        }

        protected override GenericGadget GadgetCreater (GenericGadgetSettings settings) {
            var switchSettings = settings as SwitchSettings;
            if (switchSettings == null) {
                throw new ArgumentException ("Settings must be SwitchSettings");
            }
            var sw = new Switch (switchSettings);
            return sw;
        }
    }
}
