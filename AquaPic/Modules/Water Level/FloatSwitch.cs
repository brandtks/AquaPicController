using System;
using AquaPic.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Modules
{
    public enum SwitchType {
        [Description("Normally Opened")]
        NormallyOpened,
        [Description("Normally Closed")]
        NormallyClosed
    }

    public enum SwitchFunction {
        None,
        LowLevel,
        HighLevel,
        ATO
    }

    public partial class WaterLevel
    {
        private class FloatSwitch
        {
            public string name;
            public bool activated;
            public SwitchType type;
            public SwitchFunction function;
            public float physicalLevel;
            public IndividualControl channel;
            public OnDelayTimer odt;

            public FloatSwitch (uint timeOffset) {
                activated = false;
                type = SwitchType.NormallyOpened;
                function = SwitchFunction.None;
                physicalLevel = -1.0f;
                channel = IndividualControl.Empty;
                odt = new OnDelayTimer (timeOffset);
            }
        }
    }
}

