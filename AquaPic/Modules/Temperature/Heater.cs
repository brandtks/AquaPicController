using System;
using AquaPic.PowerDriver;
using AquaPic.Globals;
using AquaPic.CoilCondition;

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
            //public Condition RequestedState;

            public Heater (byte powerID, byte plugID, bool controlTemp, float setpoint, float bandwidth, string name) {
                this.Plug.Group = powerID;
                this.Plug.Individual = plugID;
                this.ControlTemperature = controlTemp;
                this.Setpoint = setpoint;
                this.BandWidth = bandwidth / 2;
                this.Name = name;
                PlugControl = Power.AddPlug (this.Plug, name, MyState.On);

                Condition autoControl = new Condition (name + " auto control");
                autoControl.CheckHandler += OnRequestedState;

                //PlugControl.Conditions.Add (RequestedState);
               // PlugControl.Conditions.Add (ConditionLocker.GetCondition ("High temperature"));
                PlugControl.Conditions.Script += " OR START " + autoControl.Name + " AND NOT high temperature END";
            }

            protected bool OnRequestedState () {
                if (Power.GetPlugMode (Plug) == Mode.Auto) {
                    if (ControlTemperature) {
                        if (WaterColumnTemperature >= (Setpoint + BandWidth))
                            return false;

                        if (WaterColumnTemperature <= (Setpoint - BandWidth))
                            return true;
                    }
                    return true;
                }
                return false;
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

