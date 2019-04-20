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
using AquaPic.Drivers;
using AquaPic.Globals;
using AquaPic.Service;
using AquaPic.Gadgets.Device;
using AquaPic.PubSub;

namespace AquaPic.Modules.Temperature
{
    public partial class Temperature
    {
        private class Heater : GenericDevice
        {
            public string temperatureGroupName;

            public Heater (HeaterSettings settings, string temperatureGroupName) : base (settings) {
                this.temperatureGroupName = temperatureGroupName;
                Driver.Power.AddOutlet (channel, name, MyState.On, key);
                Subscribe (Driver.Power.GetChannelEventPublisherKey (channel));
            }

            protected override ValueType OnRun () {
                if (CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                    bool cond = true;
                    cond &= !Alarm.CheckAlarming (temperatureGroups[temperatureGroupName].highTemperatureAlarmIndex);
                    cond &= CheckTemperature ();
                    return cond;
                }

                return false;
            }

            protected bool CheckTemperature () {
                if (CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                    var deadband = temperatureGroups[temperatureGroupName].temperatureDeadband / 2;
                    var temp = temperatureGroups[temperatureGroupName].temperatureSetpoint + deadband;
                    if (temperatureGroups[temperatureGroupName].temperature >= temp)
                        return false;

                    temp = temperatureGroups[temperatureGroupName].temperatureSetpoint - deadband;
                    if (temperatureGroups[temperatureGroupName].temperature <= temp)
                        return true;
                }

                return false;
            }

            public override void OnValueChangedAction (object parm) {
                var args = parm as ValueChangedEvent;
                var state = Convert.ToBoolean (args.newValue);
                if (state) {
                    if (CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                        GetTemperatureGroupDataLogger (temperatureGroupName).AddEntry ("heater on");
                    }
                } else {
                    if (CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                        GetTemperatureGroupDataLogger (temperatureGroupName).AddEntry ("heater off");
                    }
                }
            }

            public override void Dispose () {
                Driver.Power.RemoveChannel (channel);
                Unsubscribe ();
            }
        }
    }
}

