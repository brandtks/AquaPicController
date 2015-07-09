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

    public struct CommValueFloat {
        public byte channel;
        public float value;
    }

    public struct CommValueInt {
        public byte channel;
        public int value;
    }

    public struct CommValueBool {
        public byte channel;
        public bool value;
    }


    // Can't use managed because you can't use a pointer to it
//    public struct ValueGetter<T> {
//        public byte channel;
//        public T value;
//
//        public int GetSize () {
//            int size = sizeof(byte);
//            size += sizeof(T);
//            return size;
//        }
//    }
//
//    public struct ValueSetter<T> {
//        public byte channel;
//        public T value;
//
//        public int GetSize () {
//            int size = sizeof(byte);
//            size += sizeof(T);
//            return size;
//        }
//    }
}

