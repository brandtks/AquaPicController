using System;
using AquaPic.Utilites;

namespace AquaPic.AnalogInput
{
    public class analogInputChannel
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

        public analogInputChannel (AnalogType type, string name) {
            this._type = type;
            this.name = name;
            this.value = 0.0f;
        }
    }
}

