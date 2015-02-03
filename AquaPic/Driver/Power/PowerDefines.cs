using System;
using AquaPic.Globals;

namespace AquaPic.Power
{
    public delegate void modeChangedHandler (object sender, modeChangeEventArgs args);
    public delegate void stateChangeHandler (object sender, stateChangeEventArgs args);

    public class modeChangeEventArgs : EventArgs
    {
        public byte plugID;
        public byte powerID;
        public Mode mode;

        public modeChangeEventArgs (int plugID, byte powerID, Mode mode) {
            this.plugID = (byte)plugID;
            this.powerID = powerID;
            this.mode = mode;
        }
    }

    public class stateChangeEventArgs : EventArgs
    {
        public byte plugID;
        public byte powerID;
        public Mode mode;
        public bool state;

        public stateChangeEventArgs (int plugID, byte powerID, Mode mode, bool state) {
            this.plugID = (byte)plugID;
            this.powerID = powerID;
            this.mode = mode;
            this.state = state;
        }
    }

    // AquaPicBus communication struct
    public struct plugComms {
        public byte plug;
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

