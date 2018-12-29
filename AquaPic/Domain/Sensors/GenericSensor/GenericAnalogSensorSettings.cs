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
using AquaPic.Globals;

namespace AquaPic.Sensors
{
    public class GenericAnalogSensorSettings : GenericSensorSettings
    {
        [EntitySetting (typeof (FloatMutatorDefaultZeroScaleActual), "zeroScaleCalibrationActual")]
        public float zeroScaleCalibrationActual { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultZeroScaleValue), "zeroScaleCalibrationValue")]
        public float zeroScaleCalibrationValue { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultFullScaleActual), "fullScaleCalibrationActual")]
        public float fullScaleCalibrationActual { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultFullScaleValue), "fullScaleCalibrationValue")]
        public float fullScaleCalibrationValue { get; set; }

        [EntitySetting (typeof (IntMutatorDefaultLowPassFilterFactor), "lowPassFilterFactor")]
        public int lowPassFilterFactor { get; set; }

        public GenericAnalogSensorSettings () {
            name = string.Empty;
            channel = IndividualControl.Empty;
            zeroScaleCalibrationActual = 0f;
            zeroScaleCalibrationValue = 1f;
            fullScaleCalibrationActual = 0f;
            fullScaleCalibrationValue = 4096f;
            lowPassFilterFactor = 5;
        }
    }

    public class FloatMutatorDefaultZeroScaleActual : FloatMutator
    {
        public override float Default () {
            return 0f;
        }
    }

    public class FloatMutatorDefaultZeroScaleValue : FloatMutator
    {
        public override float Default () {
            return 1f;
        }
    }

    public class FloatMutatorDefaultFullScaleActual : FloatMutator
    {
        public override float Default () {
            return 0f;
        }
    }

    public class FloatMutatorDefaultFullScaleValue : FloatMutator
    {
        public override float Default () {
            return 4096f;
        }
    }

    public class IntMutatorDefaultLowPassFilterFactor : IntMutator
    {
        public override int Default () {
            return 5;
        }
    }
}
