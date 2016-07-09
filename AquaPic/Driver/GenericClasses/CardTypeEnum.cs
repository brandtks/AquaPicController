using System;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public enum CardType {
        [Description("Analog Input Card")]
        AnalogInputCard,

        [Description("Analog Output Card")]
        AnalogOutputCard,

        [Description("Digital Input Card")]
        DigitalInputCard,

        [Description("Power Strip")]
        PowerStrip
    }
}

