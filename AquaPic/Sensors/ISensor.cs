using System;
using AquaPic.Utilites;

namespace AquaPic.Sensors
{
    interface ISensor<T>
    {
        void Add (IndividualControl channel);
        void Remove ();
        T Get ();
        void SetName (string name);
    }
}
