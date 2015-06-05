using System;
using AquaPic.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
    public partial class AnalogOutput
    {
        private class AnalogOutputChannel
        {
            public AnalogType type;
            public string name { get; set; }
            public int value { get; set; }
            public Value ValueControl;

            /*
            public AnalogOutputChannel (AnalogType type, string name) {
                if ((type == AnalogType.ZeroTen) || (type == AnalogType.PWM))
                    this._type = type;
                this.name = name;
                this.value = 0.0f;
            }
            */

            public AnalogOutputChannel (string name, ValueSetterHandler valueSetter) {
                this.type = AnalogType.None;
                this.name = name;
                this.value = 0;

                ValueControl = new Value ();
                ValueControl.ValueSetter = valueSetter;
            }
        }
    }
}