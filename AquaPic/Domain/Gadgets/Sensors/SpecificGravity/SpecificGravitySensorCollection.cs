#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2018 Goodtime Development

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
using AquaPic.DataLogging;
using AquaPic.Service;

namespace AquaPic.Gadgets.Sensor
{
    public class SpecificGravitySensorCollection : GenericSensorCollection
    {
        public static SpecificGravitySensorCollection SharedSpecificGravityCollection = new SpecificGravitySensorCollection ();

        protected SpecificGravitySensorCollection () : base ("specificGravitySensors") { }

        protected override GenericGadget GadgetCreater (GenericGadgetSettings settings) {
            var specificGravity = settings as SpecificGravitySensorSettings;
            if (specificGravity == null) {
                throw new ArgumentException ("Settings must be SpecificGravitySettings");
            }

            var sensor = new SpecificGravitySensor (specificGravity);
            return sensor;
        }

        public override void ReadAllGadgetsFromFile () {
            var sgSensor = SettingsHelper.ReadAllSettingsInArray<SpecificGravitySensorSettings> (gadgetSettingsFileName, gadgetSettingsArrayName);
            foreach (var setting in sgSensor) {
                CreateGadget (setting, false);
            }
        }

        public IDataLogger GetDataLogger (string name) {
            CheckGadgetKey (name);
            var sg = gadgets[name] as SpecificGravitySensor;
            return sg.dataLogger;
        }
    }
}
