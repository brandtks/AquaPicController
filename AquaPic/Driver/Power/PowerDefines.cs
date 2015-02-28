using System;
using AquaPic.Globals;

namespace AquaPic.PowerDriver
{
    public delegate void ModeChangedHandler (object sender, ModeChangeEventArgs args);
    public delegate void StateChangeHandler (object sender, StateChangeEventArgs args);

    public class ModeChangeEventArgs : EventArgs {
        public byte plugID;
        public byte powerID;
        public Mode mode;

        public ModeChangeEventArgs (int plugID, byte powerID, Mode mode) {
            this.plugID = (byte)plugID;
            this.powerID = powerID;
            this.mode = mode;
        }
    }

    public class StateChangeEventArgs : EventArgs {
        public byte plugID;
        public byte powerID;
        public MyState state;
        public Mode mode;

        public StateChangeEventArgs (int plugID, byte powerID, MyState state, Mode mode) {
            this.plugID = (byte)plugID;
            this.powerID = powerID;
            this.state = state;
        }
    }

    // AquaPicBus communication struct
    public struct PlugComms {
        public byte plugID;
        public bool state;
    }

    public struct PowerComms {
        public bool acPowerAvailable;
        public byte currentAvailableMask;
    }

    public struct AmpComms {
        public byte plugID;
        public float current;
    }
}

