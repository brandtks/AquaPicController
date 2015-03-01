using System;
using AquaPic.Globals;

namespace AquaPic.AnalogInputDriver
{
    public partial class AnalogInput
    {
        private class AnalogInputChannel
        {
            public AnalogType type;
            public string name { get; set; }
            public float value { get; set; }

            /*
            public AnalogInputChannel (AnalogType type, string name) {
                if ((type == AnalogType.Level) || (type == AnalogType.Temperature))
                    this._type = type;
                this.name = name;
                this.value = 0.0f;
            }
            */

            public AnalogInputChannel () {
                this.type = AnalogType.None;
                this.name = null;
                this.value = 0.0f;
            }
        }
    }
}

