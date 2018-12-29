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
using System.Collections.Generic;
using AquaPic.Globals;
using AquaPic.Runtime;
using AquaPic.PubSub;

namespace AquaPic.Sensors
{
    public class GenericAnalogSensorCollection : GenericSensorCollection
    {
        public GenericAnalogSensorCollection (string sensorSettingsArrayName) : base (sensorSettingsArrayName) { }

        public void SetCalibrationData (string name, float zeroScaleActual, float zeroScaleValue, float fullScaleActual, float fullScaleValue) {
            CheckSensorKey (name);

            if (fullScaleValue <= zeroScaleValue)
                throw new ArgumentException ("Full scale value can't be less than or equal to zero value");

            if (fullScaleActual < 0.0f)
                throw new ArgumentException ("Full scale actual can't be less than zero");

            var phProbe = sensors[name] as GenericAnalogSensor;

            phProbe.zeroScaleActual = zeroScaleActual;
            phProbe.zeroScaleValue = zeroScaleValue;
            phProbe.fullScaleActual = fullScaleActual;
            phProbe.fullScaleValue = fullScaleValue;

            UpdateSensorSettingsInFile (name);
        }
    }
}
