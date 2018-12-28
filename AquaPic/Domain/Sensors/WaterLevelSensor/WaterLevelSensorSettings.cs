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
using GoodtimeDevelopment.Utilites;
using AquaPic.Globals;

namespace AquaPic.Sensors
{
    public class WaterLevelSensorSettings : GenericSensorSettings
    {
        [EntitySetting (typeof (FloatMutatorDefaultWaterLevelSensorZeroScaleValue), "zeroScaleCalibrationValue")]
        public float zeroScaleCalibrationValue { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultWaterLevelSensorFullScaleActual), "fullScaleCalibrationActual")]
        public float fullScaleCalibrationActual { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultWaterLevelFullScaleValue), "fullScaleCalibrationValue")]
        public float fullScaleCalibrationValue { get; set; }

        public WaterLevelSensorSettings () {
            name = string.Empty;
            channel = IndividualControl.Empty;
            zeroScaleCalibrationValue = 819.2f;
            fullScaleCalibrationActual = 15f;
            fullScaleCalibrationValue = 3003.73f;
        }
    }

    public class FloatMutatorDefaultWaterLevelSensorZeroScaleValue : FloatMutator
    {
        public override float Default () {
            return 819.2f;
        }
    }

    public class FloatMutatorDefaultWaterLevelSensorFullScaleActual : FloatMutator
    {
        public override float Default () {
            return 15f;
        }
    }

    public class FloatMutatorDefaultWaterLevelFullScaleValue : FloatMutator
    {
        public override float Default () {
            return 3003.73f;
        }
    }
}
