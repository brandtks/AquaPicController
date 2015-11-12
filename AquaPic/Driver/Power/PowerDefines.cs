using System;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public delegate void ModeChangedHandler (object sender, ModeChangeEventArgs args);
    public delegate void StateChangeHandler (object sender, StateChangeEventArgs args);

    public class ModeChangeEventArgs : EventArgs {
        public byte outletID;
        public byte powerID;
        public Mode mode;

        public ModeChangeEventArgs (int plugID, byte powerID, Mode mode) {
            this.outletID = (byte)plugID;
            this.powerID = powerID;
            this.mode = mode;
        }
    }

    public class StateChangeEventArgs : EventArgs {
        public byte outletID;
        public byte powerID;
        public MyState state;

        public StateChangeEventArgs (int plugID, byte powerID, MyState state) {
            this.outletID = (byte)plugID;
            this.powerID = powerID;
            this.state = state;
        }
    }

    public class ModeChangedObj {
        public event ModeChangedHandler ModeChangedEvent;

        public ModeChangedObj () {
        }

        public void CallEvent (object sender, ModeChangeEventArgs args) {
            if (ModeChangedEvent != null)
                ModeChangedEvent (sender, args);
        }
    }

    public class StateChangedObj {
        public event StateChangeHandler StateChangedEvent;

        public StateChangedObj () {
        }

        public void CallEvent (object sender, StateChangeEventArgs args) {
            if (StateChangedEvent != null)
                StateChangedEvent (sender, args);
        }
    }

    // AquaPicBus communication struct
    public struct PlugComms {
        public byte outletID;
        public bool state;
    }

    public struct PowerComms {
        public bool acPowerAvailable;
        public byte currentAvailableMask;
    }

    public struct AmpComms {
        public byte outletID;
        public float current;
    }
}

