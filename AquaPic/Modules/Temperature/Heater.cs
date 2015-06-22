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
            public IndividualControl plug;
            public bool controlWaterTemperature;
            public float setpoint;
            public float deadband;
            public string name;
            public Coil plugControl;

            public Heater (string name, byte powerID, byte plugID, bool controlTemp, float setpoint, float bandwidth) {
                this.plug.Group = powerID;
                this.plug.Individual = plugID;
                this.controlWaterTemperature = controlTemp;
                this.setpoint = setpoint;
                this.deadband = bandwidth / 2;
                this.name = name;
                plugControl = Power.AddOutlet (this.plug, name, MyState.On);
                plugControl.ConditionChecker = OnPlugControl;
            }

            protected bool OnPlugControl () {
                bool cond = true;
                cond &= !Alarm.CheckAlarming (HighTemperatureAlarmIndex);
                cond &= CheckTemperature ();
                return cond;
            }

            protected bool CheckTemperature () {
                if (controlWaterTemperature) {
                    if (temperature >= (temperatureSetpoint + temperatureDeadband))
                        return false;

                    if (temperature <= (temperatureSetpoint - temperatureDeadband))
                        return true;
                } else {
                    if (temperature >= (setpoint + deadband))
                        return false;

                    if (temperature <= (setpoint - deadband))
                        return true;
                }

                return false;
            }
        }
    }
}

