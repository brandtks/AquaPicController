using System;

namespace AquaPic.Globals
{
    public struct IndividualControl {
        public byte Group;
        public byte Individual;
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

