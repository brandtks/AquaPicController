using System;
using AquaPic.PowerDriver;
using AquaPic.Globals;
using AquaPic.CoilCondition;

namespace AquaPic.TemperatureDriver
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
                PlugControl = Power.AddPlug (this.Plug, name, MyState.On, true);

                Condition rs = new Condition (name + " requested state");
                rs.CheckHandler += OnRequestedState;

                //PlugControl.Conditions.Add (RequestedState);
               // PlugControl.Conditions.Add (ConditionLocker.GetCondition ("High temperature"));
                PlugControl.Conditions.Script = "AND " + rs.Name + " AND NOT high temperature";
            }

            protected bool OnRequestedState () {
                if (ControlTemperature) {
                    if (WaterColumnTemperature >= (Setpoint + BandWidth))
                        return false;

                    if (WaterColumnTemperature <= (Setpoint - BandWidth))
                        return true;
                }
                return true;
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

