using System;
using AquaPic.Utilites;

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
        Level,
        PWM,
        [Description("0-10Vdc")]
        ZeroTen,
        [Description("0-5Vdc")]
        ZeroFive
    }

    public enum MyState : byte {
        Off = 0,
        On = 1
    }
}

