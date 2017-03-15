using System;
using AquaPic.Utilites;

namespace AquaPic.Equipment
{
    interface IEquipment<T>
    {
        void Add (IndividualControl channel);
        void Remove ();
        T Set ();
        void SetName (string name);
    }
}
