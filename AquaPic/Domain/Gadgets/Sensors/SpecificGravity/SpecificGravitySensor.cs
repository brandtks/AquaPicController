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
    public class SpecificGravitySensor : GenericSensor
    {
        public IDataLogger dataLogger { get; protected set; }
        public float levelDifference { get; protected set; }
        public float calibratedSpecificGravity { get; protected set; }
        public double fullScalePressureDelta { get; protected set; }
        public string topWaterLevelSensor { get; protected set; }
        public string bottomWaterLevelSensor { get; protected set; }
        protected bool sensorsValid;

        public SpecificGravitySensor (SpecificGravitySensorSettings settings) : base (settings) {
            dataLogger = Factory.GetDataLogger (string.Format ("{0}SgProbe", name.RemoveWhitespace ()));

            levelDifference = settings.levelDifference;
            calibratedSpecificGravity = settings.calibratedSpecificGravity;

            // If we assume that the higher physical level sensor is effectively 0 pressure and then the lower sensors 
            // level can be used to calculate the 'full scale' point to convert height difference to a pressure 
            // difference
            // 249.08891 = g * m/in * 1000 = 9.80665 * 0.0254 * 1000
            // Multiply by 1000 because the calibratedSpecificGravity is given in whatever units we reefers are used to, 
            // ie 1.025, but pascals equation uses 1000 times that number for bla bla bla reasons
            fullScalePressureDelta = 249.08891 * calibratedSpecificGravity * levelDifference;

            // Set this instance's top water level sensor name to the settings value
            topWaterLevelSensor = settings.topWaterLevelSensor;
            // Set this instance's top water level sensor name to the settings value
            bottomWaterLevelSensor = settings.bottomWaterLevelSensor;

            // Make sure the top water level sensor is valid
            sensorsValid = Sensors.WaterLevelSensors.GadgetNameExists (topWaterLevelSensor);
            // Make sure the bottom water level sensor is valid
            sensorsValid &= Sensors.WaterLevelSensors.GadgetNameExists (bottomWaterLevelSensor);
            // If the top water level sensor 
            if (sensorsValid) {
                // Get the GUID key for the top level sensor
                var sensorKey = Sensors.WaterLevelSensors.GetGadgetEventPublisherKey (topWaterLevelSensor);
                // Subscribe to a value change of the top level sensor
                MessageHub.Instance.Subscribe<ValueChangedEvent> (sensorKey, OnEitherWaterLevelChangedEvent);
                // Subscrive to the gadget deleted event to make the sensor valid flag as false
                MessageHub.Instance.Subscribe<GadgetRemovedEvent> (sensorKey, OnEitherWaterLevelDeletedEvent);
                // Subscribe to the gadget update event to get a possible name change
                MessageHub.Instance.Subscribe<GadgetUpdatedEvent> (sensorKey, (parm) => {
                    // Convert the parm object to GadgetUpdatedEvent
                    var args = parm as GadgetUpdatedEvent;
                    // The GadgetUpdatedEvent object also includes a name property but that's the old name of the gadget
                    // so we want to use the new name from settings
                    topWaterLevelSensor = args.settings.name;
                });

                // Get the GUID key for the bottom level sensor
                sensorKey = Sensors.WaterLevelSensors.GetGadgetEventPublisherKey (bottomWaterLevelSensor);
                // Subscribe to a value change of the bottom level sensor
                MessageHub.Instance.Subscribe<ValueChangedEvent> (sensorKey, OnEitherWaterLevelChangedEvent);
                // Subscrive to the gadget deleted event to make the sensor valid flag as false
                MessageHub.Instance.Subscribe<GadgetRemovedEvent> (sensorKey, OnEitherWaterLevelDeletedEvent);
                // Subscribe to the gadget update event to get a possible name change
                MessageHub.Instance.Subscribe<GadgetUpdatedEvent> (sensorKey, (parm) => {
                    // Convert the parm object to GadgetUpdatedEvent
                    var args = parm as GadgetUpdatedEvent;
                    // The GadgetUpdatedEvent object also includes a name property but that's the old name of the gadget
                    // so we want to use the new name from settings
                    bottomWaterLevelSensor = args.settings.name;
                });
            } else {
                Logger.AddWarning ("Adding a Specific Gravity Sensor with at least one invalid water level sensor");
            }
        }

        protected void OnEitherWaterLevelChangedEvent (object parm) {
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

            var oldValue = value;

            if (sensorsValid) {
                // Find the delta between the calibrated level and the current level difference
                var topLevel = (float)Sensors.WaterLevelSensors.GetGadgeValue (topWaterLevelSensor);
                var bottomLevel = (float)Sensors.WaterLevelSensors.GetGadgeValue (bottomWaterLevelSensor);
                var delta = (double)Math.Abs (topLevel - bottomLevel);

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
            } else {
                // One of the water level sensors isn't valid so just return the calibrated value
                value = calibratedSpecificGravity;
            }

            // If the value changed
            if (oldValue != value) {
                // Notify the subcribers that the value changed
                NotifyValueChanged (name, value, oldValue);
            }

            NotifyValueUpdated (name, value);
        }

        protected void OnEitherWaterLevelDeletedEvent (object parm) {
            sensorsValid = false;
        }

        public override void Dispose () {
            var sensorKey = Sensors.WaterLevelSensors.GetGadgetEventPublisherKey (topWaterLevelSensor);
            MessageHub.Instance.Unsubscribe (sensorKey);

            sensorKey = Sensors.WaterLevelSensors.GetGadgetEventPublisherKey (bottomWaterLevelSensor);
            MessageHub.Instance.Unsubscribe (sensorKey);
        }
    }
}
