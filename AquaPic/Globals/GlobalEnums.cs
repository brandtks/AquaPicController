using System;

namespace AquaPic.Utilites
{
    public enum Mode : byte {
        Manual = 1,
        Auto
    }

    public enum AnalogType : byte {
        [Description("0-10Vdc")]
        ZeroTen = 0,
        [Description("0-5Vdc")]
        ZeroFive,
        PWM = 255
    }

    public enum MyState : byte {
        Off = 0,
        On = 1,
        Set,
        Reset,
        Invalid
    }

    public enum LightingTime : byte {
        Daytime = 1,
        Nighttime
    }
}

