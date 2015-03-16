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
            private string code = @"
                using System;
                using AquaPic.PowerDriver;
                using AquaPic.Globals;
                using AquaPic.AlarmRuntime;

                namespace ScriptingInterface
                {
                    public class ScriptCoil
                    {
                        public static bool CoilCondition () {
                            if (Power.GetPlugMode (Plug) == Mode.Manual) {
                                if (Power.GetManualPlugState (Plug) == MyState.On)
                                    return true;
                                else
                                    return false;
                            } else {
                                if (Alarm.CheckAlarming (HighTemperatureAlarmIndex))
                                    return false;

                                if (ControlTemperature) {
                                    if (WaterColumnTemperature >= (Setpoint + BandWidth))
                                        return false;

                                    if (WaterColumnTemperature <= (Setpoint - BandWidth))
                                        return true;
                                }
                                return true;
                            }
                        }
                    }
                }";

            public IndividualControl Plug;
            public bool ControlTemperature;
            public float Setpoint;
            public float BandWidth;
            public string Name;
            public Coil PlugControl;
            //public Condition RequestedState;

            public Heater (byte powerID, byte plugID, bool controlTemp, float setpoint, float bandwidth, string name) {
                this.Plug.Group = powerID;
                this.Plug.Individual = plugID;
                this.ControlTemperature = controlTemp;
                this.Setpoint = setpoint;
                this.BandWidth = bandwidth / 2;
                this.Name = name;
                PlugControl = Power.AddPlug (this.Plug, name, MyState.On);
                PlugControl.ConditionChecker = OnPlugControl;
            }

            protected bool OnPlugControl () {
                if (Power.GetPlugMode (Plug) == Mode.Manual) {
                    if (Power.GetManualPlugState (Plug) == MyState.On)
                        return true;
                    else
                        return false;
                } else {
                    if (Alarm.CheckAlarming (HighTemperatureAlarmIndex))
                        return false;

                    if (ControlTemperature) {
                        if (WaterColumnTemperature >= (Setpoint + BandWidth))
                            return false;

                        if (WaterColumnTemperature <= (Setpoint - BandWidth))
                            return true;
                    }
                    return true;
                }
            }

//            protected void OnHeaterOnOutput () {
//                if (Power.GetPlugState (Plug) == MyState.Off)
//                    Power.SetPlugState (Plug, MyState.On);
//            }
//
//            protected void OnHeaterOffOutput () {
//                if (Power.GetPlugState (Plug) == MyState.On)
//                    Power.SetPlugState (Plug, MyState.Off);
//            }
        }
    }
}

