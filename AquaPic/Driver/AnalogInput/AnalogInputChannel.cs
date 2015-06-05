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

            /*
            public AnalogInputChannel (AnalogType type, string name) {
                if ((type == AnalogType.Level) || (type == AnalogType.Temperature))
                    this._type = type;
                this.name = name;
                this.value = 0.0f;
            }
            */

            public AnalogInputChannel (string name) {
                this.type = AnalogType.None;
                this.name = name;
                this.value = 0.0f;
            }
        }
    }
}

