using System;
using AquaPic.Globals;

namespace AquaPic.AnalogOutputDriver
{
    public partial class AnalogOutput
    {
        private class AnalogOutputChannel
        {
            private AnalogType _type;
            public AnalogType type {
                get { return _type; }
                set {
                    if ((type == AnalogType.ZeroTen) || (type == AnalogType.PWM))
                        _type = type;
                }
            }
            public string name { get; set; }
            public float value { get; set; }

            /*
            public AnalogOutputChannel (AnalogType type, string name) {
                if ((type == AnalogType.ZeroTen) || (type == AnalogType.PWM))
                    this._type = type;
                this.name = name;
                this.value = 0.0f;
            }
            */

            public AnalogOutputChannel () {
                this._type = AnalogType.None;
                this.name = null;
                this.value = 0.0f;
            }
        }
    }
}