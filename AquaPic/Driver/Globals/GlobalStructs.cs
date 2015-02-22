using System;

namespace AquaPic.Globals
{
    public struct IndividualControl {
        public byte Group;
        public byte Individual;
    }

    public struct ValueGetterFloat {
        public byte channel;
        public float value;
    }

    public struct ValueGetterInt {
        public byte channel;
        public int value;
    }

    public struct ValueSetter {
        public byte channelID;
        public int value;
    }
}

