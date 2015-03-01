using System;
using AquaPic;
using AquaPic.SerialBus;
using AquaPic.AlarmDriver;
using AquaPic.Globals;

namespace AquaPic.PowerDriver
{
    public partial class Power
    {
        private class PowerStrip {
            public AquaPicBus.Slave slave;
            public byte powerID;
            public int commsAlarmIdx;
            public int powerAvailAlarmIdx;
            public bool acPowerAvailable;
            public string name;
            public PlugData[] plugs;

            /* <Future>
            public powerStrip (byte address, byte powerID, string[] names, byte rtnToRequestedMask) {
                this._address = address;
                this._powerID = powerID;
                this._commsAlarmIdx = alarm.subscribe ("APB communication error", "Power Strip at address " + this.address.ToString ());
                this._powerAvailAlarmIdx = alarm.subscribe ("Loss of power", "Mains power not available at address " + this.address.ToString ());
                this.commsStatus = APBstatus.notOpen;
                this.acPowerAvail = false;
                this.plugs = new plugData[8];
                for (int i = 0; i < 8; ++i) {
                    this.plugs [i] = new plugData (names[i], mtob (rtnToRequestedMask, i));
                }
    		}*/ 

            public PowerStrip (byte address, byte powerID, string name) {
                try { // if address is already used this will throw an exception
                    this.slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address);
                    this.slave.OnStatusUpdate += OnSlaveStatusUpdate;
                } catch (Exception ex) {
                    Console.WriteLine (ex.ToString ());
                    Console.WriteLine (ex.Message);
                }
                this.powerID = powerID;
                this.commsAlarmIdx = Alarm.Subscribe ("APB communication error", "Power Strip at address " + this.slave.address.ToString ());
                this.powerAvailAlarmIdx = Alarm.Subscribe ("Loss of power", "Mains power not available at address " + this.slave.address.ToString ());
                this.name = name;
                this.acPowerAvailable = false;
                this.plugs = new PlugData[8];
                for (int i = 0; i < 8; ++i) {
                    this.plugs [i] = new PlugData ();
                }
            }

            public unsafe void GetStatus () {
                slave.Read (20, sizeof(PowerComms), GetStatusCallback);
            }

            protected void GetStatusCallback (CallbackArgs callArgs) {
                if (slave.status != AquaPicBusStatus.communicationSuccess)
                    return;

                PowerComms status = new PowerComms ();

                unsafe {
                    callArgs.copyBuffer (&status, sizeof(PowerComms));
                }

                acPowerAvailable = status.acPowerAvailable;
                if (!acPowerAvailable)
                    Alarm.Post (powerAvailAlarmIdx, true);

                for (int i = 0; i < plugs.Length; ++i) {
                    if (mtob (status.currentAvailableMask, i))
                        ReadPlugCurrent ((byte)i);
                }
            }

            public void ReadPlugCurrent (byte plugID) {
                unsafe {
                    slave.ReadWrite (12, &plugID, sizeof (byte), sizeof (AmpComms), ReadPlugCurrentCallback);
                }
            }

            protected void ReadPlugCurrentCallback (CallbackArgs callArgs) {
                if (slave.status != AquaPicBusStatus.communicationSuccess)
                    return;

                AmpComms message;

                unsafe {
                    callArgs.copyBuffer (&message, sizeof(AmpComms));
                }

                plugs [message.plugID].SetCurrent (message.current);
            }

            public void SetPlugState (byte plugID, MyState state, bool modeOverride) {
                if (plugs [plugID].mode == Mode.Manual && !modeOverride) {
                    plugs [plugID].requestedState = state;
                    return;
                }

                plugs [plugID].currentState = state;
                plugs [plugID].OnChangeState (new StateChangeEventArgs (plugID, powerID, plugs [plugID].currentState, plugs [plugID].mode));

                /* Commented out for test the below called serial comms
                byte[] message = new byte[2];

                message [0] = plugID;

                if (state == MyState.On) {
                    plugs [plugID].currentState = MyState.On;
                    message [1] = 0xFF;
                } else {
                    plugs [plugID].currentState = MyState.Off;
                    message [1] = 0x00;
                }
                plugs [plugID].OnChangeState (new StateChangeEventArgs (plugID, powerID, plugs [plugID].currentState, plugs [plugID].currentMode));

                unsafe {
                    fixed (byte* ptr = message) {
                        _apb.Write (11, ptr, sizeof(byte) * 2);
                    }
                }
                */
            }

            public void SetPlugMode (byte plugID, Mode mode) {
                plugs [plugID].mode = mode;
                if (plugs [plugID].mode == Mode.Auto)
                    plugs [plugID].OnModeChangedAuto (new ModeChangeEventArgs (plugID, powerID, plugs [plugID].mode));
                else
                    plugs [plugID].OnModeChangedManual (new ModeChangeEventArgs (plugID, powerID, plugs [plugID].mode));
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((slave.status != AquaPicBusStatus.communicationSuccess) || (slave.status != AquaPicBusStatus.communicationStart))
                    Alarm.Post (commsAlarmIdx, true);
            }

            private static bool mtob (byte mask, int shift) {
                byte b = mask;
                byte _shift = (byte)shift;
                b >>= _shift;
                if ((b & 0x01) == 1)
                    return true;
                else
                    return false;
            }

            // Not Used
//            private static void btom(ref byte mask, bool b, int shift) {
//                if (b)
//                    mask |= (byte)Math.Pow (2, shift);
//                else {
//                    int m = ~(int)Math.Pow (2, shift);
//                    mask &= (byte) m;
//                }
//            }
    	}
    }
}

/* Old Stuff
protected void GetStatusCallback (CallbackArgs callArgs) {
    if (_apb.status != AquaPicBusStatus.communicationSuccess)
        return;

    PwrComms status;

    unsafe {
        callArgs.copyBuffer (&status, sizeof(PwrComms));
    }

    for (int i = 0; i < 8; ++i) {
        bool s = mtob (status.stateMask, i);
        if (s != plugs [i].currentState) {
            plugs [i].currentState = s;
            stateChangeEventArgs args = new stateChangeEventArgs (i, powerID, plugs [i].mode, plugs [i].currentState);
            plugs [i].OnChangeState (args);
        }

        bool m = mtob (status.modeMask, i); 
        if (m && (plugs [i].mode == Mode.Manual)) { // m true is auto
            plugs [i].mode = Mode.Auto; 
            modeChangeEventArgs args = new modeChangeEventArgs (i, address, plugs [i].mode);
            plugs [i].OnModeChangedAuto (args);
        } else if (!m && (plugs [i].mode == Mode.Auto)) { // m false is manual
            plugs [i].mode = Mode.Manual;
            modeChangeEventArgs args = new modeChangeEventArgs (i, address, plugs [i].mode);
            plugs [i].OnModeChangedManual (args);
        }
    }

    acPowerAvail = status.acPowerAvail;
    if (!acPowerAvail) {
        Alarm.Post (_powerAvailAlarmIdx, true);
    }
}

public void SetPlugState (byte plugID, bool state, bool modeOverride) {
    byte[] message = new byte[3];

    message [0] = plugID;

    if (state) {
        plugs [plugID].requestedState = true;
        message [1] = 0xFF;
    } else {
        plugs [plugID].requestedState = false;
        message [1] = 0x00;
    }

    if (modeOverride)
        message [2] = 0xFF;
    else
        message [2] = 0x00;

    unsafe {
        fixed (byte* ptr = message) {
            _apb.ReadWrite (11, ptr, sizeof(byte) * 3, sizeof(PlugComms), SetPlugStateCallback);
        }
    }
}

protected void SetPlugStateCallback (CallbackArgs callArgs) {
    if (_apb.status != AquaPicBusStatus.communicationSuccess)
        return;

    PlugComms status;

    unsafe {
        callArgs.copyBuffer (&status, sizeof(PlugComms));
    }

    byte plugID = status.plugID;
    plugs [plugID].mode = (Mode)status.mode;

    if (plugs [plugID].requestedState == status.state) { // the state changed
        plugs [plugID].currentState = status.state;
        stateChangeEventArgs args = new stateChangeEventArgs (plugID, powerID, plugs [plugID].mode, plugs [plugID].currentState);
        plugs [plugID].OnChangeState (args);
    }
}

// <TODO> cann't decide if I want to set all the modes the same
// or be able to only set certain ones 
public void SetAllPlugsMode (Mode mode) {
    byte message;

    for (int i = 0; i < 8; ++i)
        plugs [i].mode = mode;

    if (plugs [0].mode == Mode.Auto)
        message = 0xFF;
    else
        message = 0x00;

    unsafe {
        _apb.Write (30, &message, sizeof(byte));
    }
}

// <TODO> this need lots of work
// need to determine how to handle the callback
public void setAllPlugsState (byte mask) {
    for (int i = 0; i < 8; ++i)
        plugs [i].requestedState = mtob (mask, i);

    unsafe {
        commsStatus = APB.write (_address, 10, &mask, sizeof(byte));
    }

    if (commsStatus != APBstatus.writeSuccess) {
        alarm.post (_alarmIdx, true);
    }
}

public void SetPlugMode (byte plugID, Mode mode) {
    byte[] message = new byte[2];
    plugs [plugID].mode = mode;

    message [0] = plugID;
    message [1] = (byte)plugs [plugID].mode;

    unsafe {
        fixed (byte* ptr = message) {
            _apb.Write (31, ptr, sizeof(byte) * 2);
        }
    }
}

*/