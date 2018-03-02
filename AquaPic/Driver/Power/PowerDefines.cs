#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using AquaPic.Globals;

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
}

