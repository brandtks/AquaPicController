using System;
using AquaPic.Utilites;
using AquaPic.Runtime;
using AquaPic.Drivers;

namespace AquaPic.Sensors
{
    public enum SwitchType {
        [Description("Normally Opened")]
        NormallyOpened,
        [Description("Normally Closed")]
        NormallyClosed
    }

    public enum SwitchFunction {
        LowLevel,
        HighLevel,
        ATO
    }

    public class FloatSwitch
    {
        public string name;
        public bool activated;
        public SwitchType type;
        public SwitchFunction function;
        public float physicalLevel;
        public IndividualControl channel;
        public OnDelayTimer odt;

        public FloatSwitch (
            string name,
            SwitchType type,
            SwitchFunction function,
            float physicalLevel,
            IndividualControl channel,
            uint timeOffset
        ) {
            activated = false;
            this.name = name;
            this.type = type;
            this.function = function;
            this.physicalLevel = physicalLevel;
            this.channel = channel;
            odt = new OnDelayTimer (timeOffset);

            AquaPicDrivers.DigitalInput.AddChannel (this.channel, this.name);
        }
    }
}

