using System;
using AquaPic.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
    public partial class AnalogOutputBase
    {
        protected class AnalogOutputChannel<T> : GenericChannel<T>
        {
            public AnalogType type;
            public Value ValueControl;

            public AnalogOutputChannel (string name)
                : base (name, (T)(object)0) 
            {
                type = AnalogType.ZeroTen;
                ValueControl = new Value ();
                ValueControl.ValueSetter = (value) => SetValue (value);
            }
        }
    }
}