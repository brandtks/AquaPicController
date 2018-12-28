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

namespace AquaPic.Sensors.TemperatureProbe
{
    public class TemperatureProbeSettings : GenericSensorSettings
    {
        [EntitySetting (typeof (FloatMutatorDefaultTemperatureProbeZeroScaleActual), "zeroScaleCalibrationActual")]
        public float zeroScaleCalibrationActual { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultTemperatureProbeZeroScaleValue), "zeroScaleCalibrationValue")]
        public float zeroScaleCalibrationValue { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultTemperatureProbeFullScaleActual), "fullScaleCalibrationActual")]
        public float fullScaleCalibrationActual { get; set; }

        [EntitySetting (typeof (FloatMutatorDefaultTemperatureProbeScaleValue), "fullScaleCalibrationValue")]
        public float fullScaleCalibrationValue { get; set; }

        public TemperatureProbeSettings () {
            name = string.Empty;
            channel = IndividualControl.Empty;
            zeroScaleCalibrationValue = 819.2f;
            fullScaleCalibrationActual = 15f;
            fullScaleCalibrationValue = 3003.73f;
        }
    }

    public class FloatMutatorDefaultTemperatureProbeZeroScaleActual : FloatMutator
    {
        public override float Default () {
            return 32f;
        }
    }

    public class FloatMutatorDefaultTemperatureProbeZeroScaleValue : FloatMutator
    {
        public override float Default () {
            return 100f;
        }
    }

    public class FloatMutatorDefaultTemperatureProbeFullScaleActual : FloatMutator
    {
        public override float Default () {
            return 82f;
        }
    }

    public class FloatMutatorDefaultTemperatureProbeScaleValue : FloatMutator
    {
        public override float Default () {
            return 4095f;
        }
    }       
}
