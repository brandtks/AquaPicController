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
using AquaPic.Globals;

namespace AquaPic.Gadgets.Sensor
{ 
    public class SpecificGravitySensorSettings : GenericSensorSettings
    {
        [EntitySetting (typeof (StringMutator), "topWaterLevelSensor")]
        public string topWaterLevelSensor { get; set; }

        [EntitySetting (typeof (StringMutator), "bottomWaterLevelSensor")]
        public string bottomWaterLevelSensor { get; set; }

        [EntitySetting (typeof (FloatMutator), "levelDifference")]
        public float levelDifference { get; set; }

        [EntitySetting (typeof (FloatMutator), "calibratedSpecificGravity")]
        public float calibratedSpecificGravity { get; set; }

        public SpecificGravitySensorSettings () {
            name = string.Empty;
            topWaterLevelSensor = "None";
            bottomWaterLevelSensor = "None";
            levelDifference = 3f;
            calibratedSpecificGravity = 1.025f;
        }
    }
}
