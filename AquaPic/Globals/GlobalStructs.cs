using System;

namespace AquaPic.Utilites
{
    public struct IndividualControl {
        public static IndividualControl Empty {
            get {
                IndividualControl ic = new IndividualControl ();
                ic.Group = -1;
                ic.Individual = -1;
                return ic;
            }
        }

        public int Group;
        public int Individual;

        public bool IsNotEmpty () {
            bool check = true;
            check &= (Individual != -1);
            check &= (Group != -1);
            return check;
        }
    }
}

