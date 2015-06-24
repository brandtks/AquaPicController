using System;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public partial class AnalogInput
    {
        private class AnalogInputChannel
        {
            public AnalogType type;
            public string name;
            public float value;
            public Mode mode;

            public AnalogInputChannel (string name) {
                this.type = AnalogType.None;
                this.name = name;
                this.value = 0.0f;
                mode = Mode.Auto;
            }
        }
    }
}

