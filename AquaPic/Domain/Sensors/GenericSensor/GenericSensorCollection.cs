﻿#region License

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
using AquaPic.Consumers;

namespace AquaPic.Sensors
{
    public class GenericSensorCollection 
    {
        protected Dictionary<string, GenericSensor> sensors;
        protected readonly string sensorSettingsFileName = "sensors";
        public string sensorSettingsArrayName { get; protected set; }

        public GenericSensorCollection (string sensorSettingsArrayName) {
            sensors = new Dictionary<string, GenericSensor> ();
            this.sensorSettingsArrayName = sensorSettingsArrayName;
        }

        public virtual void AddAllSensors () => throw new NotImplementedException ();

        public void AddSensor (GenericSensorSettings settings) {
            AddSensor (settings, true);
        }

        protected void AddSensor (GenericSensorSettings settings, bool saveToFile) {
            if (SensorNameExists (settings.name)) {
                throw new Exception (string.Format ("Sensor: {0} already exists", settings.name));
            }
            var sensor = OnCreateSensor (settings);
            sensors[sensor.name] = sensor;
            sensor.OnCreate ();
            if (saveToFile) {
                AddSensorSettingsToFile (settings);
            }
        }

        protected virtual GenericSensor OnCreateSensor (GenericSensorSettings settings) => throw new NotImplementedException ();

        public void UpdateSensor (string name, GenericSensorSettings settings) {
            if (SensorNameExists (name)) {
                settings = OnUpdateSensor (name, settings);
                RemoveSensor (name, false);
            }
            AddSensor (settings, true);
            sensors[settings.name].Updated (name, settings);
        }

        protected virtual GenericSensorSettings OnUpdateSensor (string name, GenericSensorSettings settings) {
            return settings;
        }

        public void RemoveSensor (string name) {
            RemoveSensor (name, true);
        }

        protected void RemoveSensor (string name, bool callRemoveEvent) {
            CheckSensorKey (name);
            var sensor = sensors[name];
            sensor.OnRemove ();
            sensors.Remove (name);
            DeleteSensorSettingsFromFile (name);
            if (callRemoveEvent) {
                sensor.Removed ();
            }
        }

        public void SubscribeConsumer (string name, SensorConsumer consumer) {
            CheckSensorKey (name);
            var sensor = sensors[name];
            var consumerType = consumer.GetType ();

            var methodInfo = consumerType.GetMethod (nameof (consumer.OnValueChangedEvent));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                sensor.ValueChangedEvent += consumer.OnValueChangedEvent;
            }

            methodInfo = consumerType.GetMethod (nameof (consumer.OnValueUpdatedEvent));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                sensor.ValueUpdatedEvent += consumer.OnValueUpdatedEvent;
            }

            methodInfo = consumerType.GetMethod (nameof (consumer.OnSensorUpdatedEvent));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                sensor.SensorUpdatedEvent += consumer.OnSensorUpdatedEvent;
            }

            methodInfo = consumerType.GetMethod (nameof (consumer.OnSensorRemovedEvent));
            if (methodInfo.DeclaringType != methodInfo.GetBaseDefinition ().DeclaringType) {
                sensor.SensorRemovedEvent += consumer.OnSensorRemovedEvent;
            }
        }

        public void UnsubscribeConsumer (string name, SensorConsumer consumer) {
            CheckSensorKey (name);
            var sensor = sensors[name];
            sensor.ValueChangedEvent -= consumer.OnValueChangedEvent;
            sensor.ValueUpdatedEvent -= consumer.OnValueUpdatedEvent;
            sensor.SensorUpdatedEvent -= consumer.OnSensorUpdatedEvent;
            sensor.SensorRemovedEvent -= consumer.OnSensorRemovedEvent;
        }

        public void CheckSensorKey (string name) {
            if (!sensors.ContainsKey (name)) {
                throw new ArgumentException ("name");
            }
        }

        public bool CheckSensorKeyNoThrow (string name) {
            try {
                CheckSensorKey (name);
                return true;
            } catch {
                return false;
            }
        }

        public bool SensorNameExists (string name) {
            return CheckSensorKeyNoThrow (name);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public virtual string[] GetAllSensorNames () {
            List<string> names = new List<string> ();
            foreach (var sensor in sensors.Values) {
                names.Add (sensor.name);
            }
            return names.ToArray ();
        }

        /***State***/
        public virtual GenericSensor GetSensor (string name) {
            CheckSensorKey (name);
            return sensors[name].Clone ();
        }

        /***State***/
        public virtual ValueType GetSensorValue (string name) {
            CheckSensorKey (name);
            return sensors[name].GetValue ();
        }

        /***Settings***************************************************************************************************/
        public virtual GenericSensorSettings GetSensorSettings (string name) {
            CheckSensorKey (name);
            var settings = new GenericSensorSettings ();
            settings.name = name;
            settings.channel = sensors[name].channel;
            return settings;
        }

        protected void AddSensorSettingsToFile (IEntitySettings settings) {
            SettingsHelper.AddSettingsToArray (sensorSettingsFileName, sensorSettingsArrayName, settings);
        }

        protected void UpdateSensorSettingsInFile (string name) {
            SettingsHelper.UpdateSettingsInArray (sensorSettingsFileName, sensorSettingsArrayName, name, GetSensorSettings (name));
        }

        protected void DeleteSensorSettingsFromFile (string name) {
            SettingsHelper.DeleteSettingsFromArray (sensorSettingsFileName, sensorSettingsArrayName, name);
        }
    }
}