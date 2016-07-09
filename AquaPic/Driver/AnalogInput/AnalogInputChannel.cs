using System;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public partial class AnalogInputBase
    {
        protected class AnalogInputChannel<T> : GenericChannel<T>
        {
            public AnalogInputChannel (string name) 
                : base (name, (T)(object)0.0f) { }
        }
    }
}

