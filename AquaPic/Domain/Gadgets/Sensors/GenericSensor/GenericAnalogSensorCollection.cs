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
using AquaPic.Service;

namespace AquaPic.Gadgets.Sensor
{
    public class GenericAnalogSensorCollection : GenericSensorCollection
    {
        public GenericAnalogSensorCollection (string sensorSettingsArrayName) : base (sensorSettingsArrayName) { }

        public override void ReadAllGadgetsFromFile () {
            var sensorSettings = SettingsHelper.ReadAllSettingsInArray<GenericAnalogSensorSettings> (gadgetSettingsFileName, gadgetSettingsArrayName);
            foreach (var setting in sensorSettings) {
                CreateGadget (setting, false);
            }
        }

        public override GenericGadgetSettings GetGadgetSettings (string name) {
            CheckGadgetKey (name);
            var genericAnalogSensor = gadgets[name] as GenericAnalogSensor;
            var settings = new GenericAnalogSensorSettings ();
            settings.name = genericAnalogSensor.name;
            settings.channel = genericAnalogSensor.channel;
            settings.zeroScaleCalibrationActual = genericAnalogSensor.zeroScaleCalibrationActual;
            settings.zeroScaleCalibrationValue = genericAnalogSensor.zeroScaleCalibrationValue;
            settings.fullScaleCalibrationActual = genericAnalogSensor.fullScaleCalibrationActual;
            settings.fullScaleCalibrationValue = genericAnalogSensor.fullScaleCalibrationValue;
            settings.lowPassFilterFactor = genericAnalogSensor.lowPassFilterFactor;
            return settings;
        }

        public void SetCalibrationData (string name, float zeroScaleActual, float zeroScaleValue, float fullScaleActual, float fullScaleValue) {
            CheckGadgetKey (name);

            if (fullScaleValue <= zeroScaleValue)
                throw new ArgumentException ("Full scale value can't be less than or equal to zero value");

            if (fullScaleActual < 0.0f)
                throw new ArgumentException ("Full scale actual can't be less than zero");

            var phProbe = gadgets[name] as GenericAnalogSensor;

            phProbe.zeroScaleCalibrationActual = zeroScaleActual;
            phProbe.zeroScaleCalibrationValue = zeroScaleValue;
            phProbe.fullScaleCalibrationActual = fullScaleActual;
            phProbe.fullScaleCalibrationValue = fullScaleValue;

            UpdateGadgetSettingsInFile (name);
        }

        public bool GetAnalogSensorConnected (string name) {
            CheckGadgetKey (name);
            var analogSensor = gadgets[name] as GenericAnalogSensor;
            return analogSensor.connected;
        }
    }
}
