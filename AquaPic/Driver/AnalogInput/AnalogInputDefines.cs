using System;

namespace AquaPic.AnalogInputDriver
{
    public struct analogInputCh {
        public byte cardID;
        public byte channelID;
    }

    public struct channelValue {
        public byte channel;
        public float value;
    }
}

