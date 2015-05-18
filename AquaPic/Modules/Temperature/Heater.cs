using System;
using AquaPic.PowerDriver;
using AquaPic.Globals;
using AquaPic.CoilRuntime;
using AquaPic.AlarmRuntime;
using AquaPic.PluginRuntime;

namespace AquaPic.TemperatureModule
{
    public partial class Temperature
    {
        private class Heater 
        {
            public IndividualControl Plug;
            public bool ControlTemperature;
            public float Setpoint;
            public float BandWidth;
            public string Name;
            public Coil PlugControl;

            public Heater (byte powerID, byte plugID, bool controlTemp, float setpoint, float bandwidth, string name) {
                this.Plug.Group = powerID;
                this.Plug.Individual = plugID;
                this.ControlTemperature = controlTemp;
                this.Setpoint = setpoint;
                this.BandWidth = bandwidth / 2;
                this.Name = name;
                PlugControl = Power.AddOutlet (this.Plug, name, MyState.On);
                PlugControl.ConditionChecker = OnPlugControl;
            }

            protected bool OnPlugControl () {
                if (Alarm.CheckAlarming (HighTemperatureAlarmIndex))
                    return false;

                if (ControlTemperature) {
                    if (temperature >= (Setpoint + BandWidth))
                        return false;

                    if (temperature <= (Setpoint - BandWidth))
                        return true;
                }
                return true;
            }
        }
    }
}

