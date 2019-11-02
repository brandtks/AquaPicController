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
    public class SpecificGravitySettings : WaterLevelSensorSettings
    {
        [EntitySetting (typeof (IndividualControlMutator), new string[] { "secondCard", "secondChannel" })]
        public IndividualControl secondChannel { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultZeroScaleActual), "secondZeroScaleCalibrationActual")]
        public float secondZeroScaleCalibrationActual { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultZeroScaleValue), "secondZeroScaleCalibrationValue")]
        public float secondZeroScaleCalibrationValue { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultFullScaleActual), "secondFullScaleCalibrationActual")]
        public float secondFullScaleCalibrationActual { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultFullScaleValue), "secondFullScaleCalibrationValue")]
        public float secondFullScaleCalibrationValue { get; set; }

        [EntitySetting (typeof (FloatMutator), "levelDifference")]
        public float levelDifference { get; set; }

        [EntitySetting (typeof (FloatMutator), "calibratedSpecificGravity")]
        public float calibratedSpecificGravity { get; set; }

        public SpecificGravitySettings () {
            name = string.Empty;
            channel = IndividualControl.Empty;
            secondChannel = IndividualControl.Empty;
            zeroScaleCalibrationActual = 0f;
            zeroScaleCalibrationValue = 819.2f;
            fullScaleCalibrationActual = 15f;
            fullScaleCalibrationValue = 3003.73f;
            secondZeroScaleCalibrationActual = 0f;
            secondZeroScaleCalibrationValue = 819.2f;
            secondFullScaleCalibrationActual = 15f;
            secondFullScaleCalibrationValue = 3003.73f;
            lowPassFilterFactor = 5;
            levelDifference = 3f;
            calibratedSpecificGravity = 1.025f;
        }
    }
}
