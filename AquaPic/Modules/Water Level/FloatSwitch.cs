using System;
using AquaPic.Utilites;

namespace AquaPic.Modules
{
    public enum SwitchType {
        [Description("Normally opened")]
        NormallyOpened,
        [Description("Normally closed")]
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
            public SwitchType type;
            public SwitchFunction function;
            public float physicalLevel;
            public IndividualControl channel;

            public FloatSwitch () {
                type = SwitchType.NormallyOpened;
                function = SwitchFunction.None;
                physicalLevel = -1.0f;
                channel = IndividualControl.Empty;
            }
        }
    }
}

