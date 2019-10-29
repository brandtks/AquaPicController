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
using AquaPic.DataLogging;
using AquaPic.Drivers;
using AquaPic.Service;

namespace AquaPic.Gadgets.Sensor
{
    public class SpecificGravity : WaterLevelSensor
    {
        public IDataLogger dataLogger { get; protected set; }
        public WaterLevelSensor secondSensor { get; protected set; }
        public float levelDifference { get; protected set; }
        public float calibratedSpecificGravity { get; protected set; }
        public double fullScalePressureDelta { get; protected set; }

        public SpecificGravity (SpecificGravitySettings settings) : base (settings) {
            dataLogger = Factory.GetDataLogger (string.Format ("{0}PhProbe", name.RemoveWhitespace ()));

            var secondSettings = new WaterLevelSensorSettings ();
            secondSettings.channel = settings.secondChannel;
            secondSettings.zeroScaleCalibrationValue = settings.secondZeroScaleCalibrationValue;
            secondSettings.zeroScaleCalibrationActual = settings.zeroScaleCalibrationActual;
            secondSettings.fullScaleCalibrationValue = settings.fullScaleCalibrationValue;
            secondSettings.fullScaleCalibrationActual = settings.fullScaleCalibrationActual;
            secondSettings.lowPassFilterFactor = settings.lowPassFilterFactor;
            secondSensor = new WaterLevelSensor (secondSettings);

            levelDifference = settings.levelDifference;
            calibratedSpecificGravity = settings.calibratedSpecificGravity;

            // If we assume that the higher physical level sensor is effectively 0 pressure and then the lower sensors 
            // level can be used to calculate the 'full scale' point to convert height difference to a pressure 
            // difference
            // 249.08891 = g * m/in * 1000 = 9.80665 * 0.0254 * 1000
            // Multiply by 1000 because the calibratedSpecificGravity is given in whatever units we reefers are used to, 
            // ie 1.025, but pascals equation uses 1000 times that number for bla bla bla reasons
            fullScalePressureDelta = 249.08891 * calibratedSpecificGravity * levelDifference;

            // Subscribe to a value change of the first level sensor
            MessageHub.Instance.Subscribe<ValueChangedEvent> (key, OnEitherLevelChanged);
            // Subscrive to a value change of the second level sensor
            MessageHub.Instance.Subscribe<ValueChangedEvent> (secondSensor.key, OnEitherLevelChanged);
        }

        protected void OnEitherLevelChanged (object parm) {
            /* Pascals law is P = pgh where 
             *   P is pressure
             *   p is the density of the fluid            
             *   g is gravitational acceleration 9.80665 m/sec^2
             *   h is height of the fluid
             *  
             * If we rearrange the equation we get p = P/gh. Thus a change in pressure is directly proportional to a 
             * change in density. However with the level sensors we only have a change an arbitrary raw counts and 
             * height. However we can use that change of pressure to figure out an effective change of pressure and with
             * that effective change in pressure we can determine the density of the fluid            
            */

            // Find the delta between the calibrated level and the current level difference
            var delta = Math.Abs ((double)value - (double)secondSensor.value);

            // Convert the measured level difference to a difference in pressure
            var pressureDelta = delta.Map (0, levelDifference, 0, fullScalePressureDelta);
            // Now use pascals law to calculate the density and 
            // 0.24908891 = g * m/in = 9.80665 * 0.0254 
            var density = pressureDelta / (levelDifference * 0.24908891);
            // Now divide by 1000 because unit reasons. I could look it up but who has time for that
            density /= 1000;
            // Add the new density to the datalogger
            dataLogger.AddEntry (density);
            // Set the sensor's value to the density 
            value = density;
        }

        public override void Dispose () {
            base.Dispose ();
            secondSensor.Dispose ();
        }
    }
}
