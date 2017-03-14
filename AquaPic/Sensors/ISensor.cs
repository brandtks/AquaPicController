using System;

namespace AquaPic.Sensors
{
    interface ISensor<T>
    {
        void Add ();
        void Remove ();
        T Get ();
    }
}
