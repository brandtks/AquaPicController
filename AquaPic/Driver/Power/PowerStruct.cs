using System;
using AquaPic;

namespace AquaPic.Power
{
    // AquaPicBus communication struct
    public struct plugComms {
        public bool state;
        public byte mode;
    }

    // AquaPicBus communication struct
    public struct pwrComms {
        public byte stateMask;
        public byte modeMask;
        public bool acPowerAvail;
    }

    // data passing
    public struct pwrPlug {
        public byte powerID;
        public byte plugID;
    }
}

