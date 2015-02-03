using System;
using AquaPic.Power;

namespace AquaPic.Temp
{
    public class Heater
    {
        public pwrPlug plug;
        public bool controlTemp { get; set; }
        public float setpoint { get; set; }
        public float offset { get; set; }

        public Heater (byte powerID, byte plugID, bool controlTemp, float setpoint, float offset) {
            this.plug.powerID = powerID;
            this.plug.plugID = plugID;
            this.controlTemp = controlTemp;
            this.setpoint = setpoint;
            this.offset = offset;
        }

        public void turnHeaterOn (bool modeOverride = false) {
            //power.setPlug (plug, true, modeOverride);
        }

        public void turnHeaterOff (bool modeOverride = false) {
            // power.setPlug (plug, false, modeOverride);
        }
    }
}

