using System;
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

            public Heater (string name, byte powerID, byte plugID, string temperatureGroupName) {
                this.name = name;
                plug.Group = powerID;
                plug.Individual = plugID;
                this.temperatureGroupName = temperatureGroupName;
                var plugControl = Power.AddOutlet (plug, name, MyState.On, "Temperature");
                plugControl.ConditionChecker = OnPlugControl;
                Power.AddHandlerOnStateChange (plug, OnStateChange);
            }

            ~Heater () {
                Power.RemoveHandlerOnStateChange (plug, OnStateChange);
            }

            public bool OnPlugControl () {
                if (CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                    bool cond = true;
                    cond &= !Alarm.CheckAlarming (temperatureGroups[temperatureGroupName].highTemperatureAlarmIndex);
                    cond &= CheckTemperature ();
                    return cond;
                } else {
                    return false;
                }
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

            protected void OnStateChange (object obj, StateChangeEventArgs args) {
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

