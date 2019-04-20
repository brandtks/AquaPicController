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
using System.Collections.Generic;
using GoodtimeDevelopment.Utilites;
using AquaPic.Service;
using AquaPic.DataLogging;
using AquaPic.PubSub;
using AquaPic.Gadgets;
using AquaPic.Gadgets.Sensor;
using AquaPic.Gadgets.Sensor.TemperatureProbe;

namespace AquaPic.Modules.Temperature
{
    public partial class Temperature
    {
        private class TemperatureGroup : GadgetSubscriber
        {
            public string name;
            public float temperature;
            public IDataLogger dataLogger;

            public float highTemperatureAlarmSetpoint;
            public float lowTemperatureAlarmSetpoint;
            public float temperatureSetpoint;
            public float temperatureDeadband;

            public int highTemperatureAlarmIndex;
            public int lowTemperatureAlarmIndex;

            public Dictionary<string, InternalTemperatureProbeState> temperatureProbes;
            public Dictionary<string, Heater> heaters;

            public TemperatureGroup (
                string name,
                float highTemperatureAlarmSetpoint,
                float lowTemperatureAlarmSetpoint,
                float temperatureSetpoint,
                float temperatureDeadband,
                IEnumerable<string> temperatureProbes,
                IEnumerable<HeaterSettings> heaters) 
            {
                this.name = name;
                this.highTemperatureAlarmSetpoint = highTemperatureAlarmSetpoint;
                this.lowTemperatureAlarmSetpoint = lowTemperatureAlarmSetpoint;
                this.temperatureSetpoint = temperatureSetpoint;
                this.temperatureDeadband = temperatureDeadband;

                temperature = 0.0f;
                dataLogger = Factory.GetDataLogger (string.Format ("{0}Temperature", this.name.RemoveWhitespace ()));

                highTemperatureAlarmIndex = Alarm.Subscribe (string.Format ("{0} high temperature", name));
                lowTemperatureAlarmIndex = Alarm.Subscribe (string.Format ("{0} low temperature", name));

                this.temperatureProbes = new Dictionary<string, InternalTemperatureProbeState> ();
                foreach (var temperatureProbe in temperatureProbes) {
                    this.temperatureProbes.Add (temperatureProbe, new InternalTemperatureProbeState ());
                    Subscribe (Sensors.TemperatureProbes.GetGadgetEventPublisherKey (temperatureProbe));
                }

                this.heaters = new Dictionary<string, Heater> ();
                foreach (var heater in heaters) {
                    if (this.heaters.ContainsKey (heater.name)) {
                        throw new Exception (string.Format ("Heater: {0} already exists", heater.name));
                    }

                    var heaterSettings = new HeaterSettings ();
                    heaterSettings.name = heater.name;
                    heaterSettings.channel = heater.channel;
                    this.heaters[heater.name] = new Heater (heaterSettings, this.name);
                }
            }

            public void GroupRun () {
                if (temperatureProbes.Count > 0) {
                    dataLogger.AddEntry (temperature);
                } else {
                    dataLogger.AddEntry ("no probes");
                }
            }

            public override void OnGadgetUpdatedAction (object parm) {
                var args = parm as GadgetUpdatedEvent;
                if (temperatureProbes.ContainsKey (args.name)) {
                    if (args.name != args.settings.name) {
                        var sensorState = temperatureProbes[args.name];
                        temperatureProbes.Remove (args.name);
                        temperatureProbes[args.settings.name] = sensorState;
                    }
                }
                UpdateTemperatureGroupSettingsInFile (name);
            }

            public override void OnGadgetRemovedAction (object parm) {
                var args = parm as GadgetRemovedEvent;
                if (temperatureProbes.ContainsKey (args.name)) {
                    temperatureProbes.Remove (args.name);
                    CalculateTemperature ();
                }
                UpdateTemperatureGroupSettingsInFile (name);
            }

            public override void OnValueChangedAction (object parm) {
                var args = parm as ValueChangedEvent;
                if (temperatureProbes.ContainsKey (args.name)) {
                    var temperatureProbe = Sensors.TemperatureProbes.GetGadget (args.name) as TemperatureProbe;
                    temperatureProbes[temperatureProbe.name].connected = temperatureProbe.connected;
                    temperatureProbes[temperatureProbe.name].temperature = Convert.ToSingle (temperatureProbe.value);
                    CalculateTemperature ();
                }
            }

            protected void CalculateTemperature () {
                temperature = 0;
                var connectedTemperatureProbes = 0;
                foreach (var internalTemperatureProbe in temperatureProbes.Values) {
                    if (internalTemperatureProbe.connected) {
                        temperature += internalTemperatureProbe.temperature;
                        connectedTemperatureProbes++;
                    }
                }
                temperature /= connectedTemperatureProbes;

                if (temperature > highTemperatureAlarmSetpoint) {
                    if (!Alarm.CheckAlarming (highTemperatureAlarmIndex)) {
                        Alarm.Post (highTemperatureAlarmIndex);
                        dataLogger.AddEntry ("high alarm");
                    }
                } else {
                    if (Alarm.CheckAlarming (highTemperatureAlarmIndex)) {
                        Alarm.Clear (highTemperatureAlarmIndex);
                    }
                }

                if (temperature < lowTemperatureAlarmSetpoint) {
                    if (!Alarm.CheckAlarming (lowTemperatureAlarmIndex)) {
                        Alarm.Post (lowTemperatureAlarmIndex);
                        dataLogger.AddEntry ("low alarm");
                    }
                } else {
                    if (Alarm.CheckAlarming (lowTemperatureAlarmIndex)) {
                        Alarm.Clear (lowTemperatureAlarmIndex);
                    }
                }
            }

            public class InternalTemperatureProbeState
            {
                public bool connected;
                public float temperature;

                public InternalTemperatureProbeState () {
                    connected = false;
                    temperature = 0f;
                }
            }
        }
    }
}

