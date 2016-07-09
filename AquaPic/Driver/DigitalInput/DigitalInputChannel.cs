using System;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public partial class DigitalInputBase
    {
        private class DigitalInputChannel <T> : GenericChannel<T> 
        {
            public DigitalInputChannel (string name) 
                : base (name, (T)(object)false) { }
        }
    }
}

