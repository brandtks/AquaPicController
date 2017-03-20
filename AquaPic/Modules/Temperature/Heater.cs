#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using AquaPic.Drivers;
using AquaPic.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Modules
{
    public partial class Temperature
    {
        private class Heater 
        {
            public string name;
            public IndividualControl plug;
            public string temperatureGroupName;

            public Heater (string name, int powerID, int plugID, string temperatureGroupName) {
                this.name = name;
                plug.Group = powerID;
                plug.Individual = plugID;
                this.temperatureGroupName = temperatureGroupName;
                var plugControl = Power.AddOutlet (plug, name, MyState.On, "Temperature");
                plugControl.ConditionChecker = OnPlugControl;
                Power.AddHandlerOnStateChange (plug, OnStateChange);
            }

            public bool OnPlugControl () {
                if (CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                    bool cond = true;
                    cond &= !Alarm.CheckAlarming (temperatureGroups[temperatureGroupName].highTemperatureAlarmIndex);
                    cond &= CheckTemperature ();
                    return cond;
                }

                return false;
            }

            public bool CheckTemperature () {
                if (CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                    var deadband =  temperatureGroups[temperatureGroupName].temperatureDeadband / 2;
                    var temp = temperatureGroups[temperatureGroupName].temperatureSetpoint + deadband;
                    if (temperatureGroups[temperatureGroupName].temperature >= temp)
                        return false;

                    temp = temperatureGroups[temperatureGroupName].temperatureSetpoint - deadband;
                    if (temperatureGroups[temperatureGroupName].temperature <= temp)
                        return true;
                }

                return false;
            }

            public void OnStateChange (object obj, StateChangeEventArgs args) {
                if (args.state == MyState.On) {
                    if (CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                        GetTemperatureGroupDataLogger (temperatureGroupName).AddEntry ("heater on");
                    }
                } else {
                    if (CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                        GetTemperatureGroupDataLogger (temperatureGroupName).AddEntry ("heater off");
                    }
                }
            }
        }
    }
}

