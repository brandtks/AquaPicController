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

namespace AquaPic.Sensors.PhProbe
{
    public class PhProbeSettings : GenericSensorSettings
    {
        [EntitySetting (typeof (FloatMutatorDefaultPhProbeZeroScaleActual), "zeroScaleCalibrationActual")]
        public float zeroScaleCalibrationActual { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultPhProbeZeroScaleValue), "zeroScaleCalibrationValue")]
        public float zeroScaleCalibrationValue { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultPhProbeFullScaleActual), "fullScaleCalibrationActual")]
        public float fullScaleCalibrationActual { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultPhProbeScaleValue), "fullScaleCalibrationValue")]
        public float fullScaleCalibrationValue { get; set; }

        public PhProbeSettings () {
            name = string.Empty;
            channel = IndividualControl.Empty;
            zeroScaleCalibrationActual = 0f;
            zeroScaleCalibrationValue = 14f;
            fullScaleCalibrationActual = 0f;
            fullScaleCalibrationValue = 4095f;
        }
    }

    public class FloatMutatorDefaultPhProbeZeroScaleActual : FloatMutator
    {
        public override float Default () {
            return 0f;
        }
    }

    public class FloatMutatorDefaultPhProbeZeroScaleValue : FloatMutator
    {
        public override float Default () {
            return 14f;
        }
    }

    public class FloatMutatorDefaultPhProbeFullScaleActual : FloatMutator
    {
        public override float Default () {
            return 0f;
        }
    }

    public class FloatMutatorDefaultPhProbeScaleValue : FloatMutator
    {
        public override float Default () {
            return 4095f;
        }
    }
}
