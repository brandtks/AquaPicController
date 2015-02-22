using System;
using AquaPic.Globals;

namespace AquaPic.AnalogInputDriver
{
    public partial class AnalogInput
    {
        private class AnalogInputChannel
        {
            private AnalogType _type;
            public AnalogType type {
                get { return _type; }
                set {
                    if ((type == AnalogType.Level) || (type == AnalogType.Temperature))
                        _type = type;
                }
            }
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
                this._type = AnalogType.None;
                this.name = null;
                this.value = 0.0f;
            }
        }
    }
}

