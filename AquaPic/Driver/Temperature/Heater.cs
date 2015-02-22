using System;
using AquaPic.PowerDriver;
using AquaPic.Globals;

namespace AquaPic.TemperatureDriver
{
    public partial class Temperature
    {
        private class Heater 
        {
            public IndividualControl plug;
            public bool controlTemp { get; set; }
            public float setpoint { get; set; }
            public float offset { get; set; }
            public string name { get; set; }

            public Heater (byte powerID, byte plugID, bool controlTemp, float setpoint, float offset) {
                this.plug.Group = powerID;
                this.plug.Individual = plugID;
                this.controlTemp = controlTemp;
                this.setpoint = setpoint;
                this.offset = offset;
                this.name = name;
                Power.AddPlug (this.plug.Group, this.plug.Individual, name, true);
            }

            public void turnHeaterOn (bool modeOverride = false) {
                Power.SetPlug (plug, true, modeOverride);
            }

            public void turnHeaterOff (bool modeOverride = false) {
                Power.SetPlug (plug, false, modeOverride);
            }
        }
    }
}

