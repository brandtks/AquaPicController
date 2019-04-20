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
using AquaPic.Runtime;

namespace AquaPic.Gadgets
{
    public class WaterLevelSensorCollection : GenericAnalogSensorCollection
    {
        public static WaterLevelSensorCollection SharedWaterLevelSensorCollectionInstance = new WaterLevelSensorCollection ();

        protected WaterLevelSensorCollection () : base ("waterLevelSensors") { }

        protected override GenericAnalogSensor AnalogSensorCreater (GenericAnalogSensorSettings settings) {
            var sensor = new WaterLevelSensor (settings);
            return sensor;
        }

        public void SetCalibrationData (string name, float zeroScaleValue, float fullScaleActual, float fullScaleValue) {
            SetCalibrationData (name, 0, zeroScaleValue, fullScaleActual, fullScaleValue);
        }
    }
}
