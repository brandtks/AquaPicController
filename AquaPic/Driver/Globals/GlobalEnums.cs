using System;

namespace AquaPic.Globals
{
    public enum Mode : byte {
        Manual = 1,
        Auto,
        AutoAuto
    }

    public enum AnalogType : byte {
        None = 1,
        Temperature,
        Level
    }
}

