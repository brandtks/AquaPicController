using System;
using AquaPic;
using AquaPic.SerialBus;
using AquaPic.AlarmRuntime;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.CoilRuntime;

namespace AquaPic.PowerDriver
{
    public partial class Power
    {
        private class PowerStrip {
            public AquaPicBus.Slave slave;
            public byte powerID;
            public int commsAlarmIdx;
            public int powerLossAlarmIndex;
            public bool AcPowerAvailable;
            public string name;
            public PlugData[] plugs;

            //<Future>
//            public powerStrip (byte address, byte powerID, string[] names, byte rtnToRequestedMask) {
//                this._address = address;
//                this._powerID = powerID;
//                this._commsAlarmIdx = alarm.subscribe ("APB communication error", "Power Strip at address " + this.address.ToString ());
//                this._powerAvailAlarmIdx = alarm.subscribe ("Loss of power", "Mains power not available at address " + this.address.ToString ());
//                this.commsStatus = APBstatus.notOpen;
//                this.acPowerAvail = false;
//                this.plugs = new plugData[8];
//                for (int i = 0; i < 8; ++i) {
//                    this.plugs [i] = new plugData (names[i], mtob (rtnToRequestedMask, i));
//                }
//    		}

            public PowerStrip (byte address, byte powerID, string name, bool alarmOnLossOfPower, int powerLossAlarmIndex) {
                try { // if address is already used this will throw an exception
                    this.slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address);
                    this.slave.OnStatusUpdate += OnSlaveStatusUpdate;
                } catch (Exception ex) {
                    Console.WriteLine (ex.ToString ());
                    Console.WriteLine (ex.Message);
                }

                this.powerID = powerID;

                this.commsAlarmIdx = Alarm.Subscribe (
                    this.slave.Address.ToString () + " communication fault", 
                    this.name + "serial communications fault");

                if (alarmOnLossOfPower && (powerLossAlarmIndex == -1))
                    this.powerLossAlarmIndex = Alarm.Subscribe (
                        "Loss of power", 
                        "Mains power not available at " + this.name);
                else
                    this.powerLossAlarmIndex = powerLossAlarmIndex;

                this.name = name;
                this.AcPowerAvailable = false;

                this.plugs = new PlugData[8];
                for (int i = 0; i < 8; ++i) {
                    int plugID = i;
                    string plugName = "plug " + plugID.ToString() + " on " + this.name;
                        
                    this.plugs [plugID] = new PlugData (
                        plugName,
                        delegate() {
                            if (plugs [plugID].mode == Mode.Manual) {
                                if (plugs [plugID].manualState == MyState.On)
                                    return true;
                                else
                                    return false;
                            }
                            return false;
                        },
                        delegate() {
                            SetPlugState ((byte)plugID, MyState.On, false);
                        },
                        delegate() {
                            SetPlugState ((byte)plugID, MyState.Off, false);
                        });
                }
            }

            #if SIMULATION
            public void SetupPlug (byte plugID, MyState fallback) {
                const int messageLength = 2;
                string[] message = new string[messageLength];
                message[0] = plugID.ToString ();
                if (fallback == MyState.On)
                    message[1] = "true";
                else
                    message[1] = "false";

                slave.Write (2, message, messageLength);
            }
            #else
            public unsafe void SetupPlug (byte plugID, MyState fallback) {
                const int messageLength = 2; 

                byte[] message = new byte[messageLength];

                message [0] = plugID;

                if (fallback == MyState.On)
                    message [1] = 0xFF;
                else
                    message [1] = 0x00;

                unsafe {
                    fixed (byte* ptr = message) {
                        slave.Write (2, ptr, sizeof(byte) * messageLength);
                    }
                }
            }
            #endif

            #if SIMULATION
            public void GetStatus () {
                slave.Read (20, 2, GetStatusCallback);
            }
            #else
            public unsafe void GetStatus () {
                slave.Read (20, sizeof(PowerComms), GetStatusCallback);
            }
            #endif

            #if SIMULATION
            protected void GetStatusCallback (CallbackArgs callArgs) {
                AcPowerAvailable = Convert.ToBoolean(callArgs.readMessage[3]);
                byte mask = Convert.ToByte(callArgs.readMessage[4]);
                for (int i = 0; i < plugs.Length; ++i) {
                    if (Utils.mtob (mask, i))
                        ReadPlugCurrent (i);
                }
            }
            #else
            protected void GetStatusCallback (CallbackArgs callArgs) {
                if (slave.Status != AquaPicBusStatus.communicationSuccess)
                    return;

                PowerComms status = new PowerComms ();

                unsafe {
                    callArgs.copyBuffer (&status, sizeof(PowerComms));
                }

                AcPowerAvailable = status.acPowerAvailable;
                if (!AcPowerAvailable)
                    Alarm.Post (powerLossAlarmIndex);

                for (int i = 0; i < plugs.Length; ++i) {
                    if (Utils.mtob (status.currentAvailableMask, i))
                        ReadPlugCurrent ((byte)i);
                }
            }
            #endif

            #if SIMULATION
            public void ReadPlugCurrent (int i) {
                const int messageLength = 1;
                string[] m = new string[messageLength];
                m [0] = i.ToString ();
                slave.ReadWrite (10, m, messageLength, 2, ReadPlugCurrentCallback);
            }
            #else
            public void ReadPlugCurrent (byte plugID) {
                unsafe {
                    slave.ReadWrite (10, &plugID, sizeof (byte), sizeof (AmpComms), ReadPlugCurrentCallback);
                }
            }
            #endif

            #if SIMULATION
            protected void ReadPlugCurrentCallback (CallbackArgs callArgs) {
                int plug = Convert.ToInt32(callArgs.readMessage [3]);
                float current = Convert.ToSingle(callArgs.readMessage [4]);
                plugs [plug].SetAmpCurrent (current);

            }
            #else
            protected void ReadPlugCurrentCallback (CallbackArgs callArgs) {
                if (slave.Status != AquaPicBusStatus.communicationSuccess)
                    return;

                AmpComms message;

                unsafe {
                    callArgs.copyBuffer (&message, sizeof(AmpComms));
                }

                plugs [message.plugID].SetAmpCurrent (message.current);

            }
            #endif

            #if SIMULATION
            public void SetPlugState (byte plugID, MyState state, bool modeOverride) {
                if ((state != plugs [plugID].currentState) && (plugs [plugID].Updated)) {
                    plugs [plugID].Updated = false;

                    const int messageLength = 2;
                    string[] message = new string[messageLength];
                    message [0] = plugID.ToString ();
                    if (state == MyState.On)
                        message [1] = "true";
                    else
                        message [1] = "false";

                    slave.ReadWrite (30, message, messageLength, 0, 
                        delegate (CallbackArgs args) {
                            plugs [plugID].OnChangeState (new StateChangeEventArgs (plugID, powerID, state, plugs [plugID].mode));
                        });
                }
            }
            #else
            public void SetPlugState (byte plugID, MyState state, bool modeOverride) {
                const int messageLength = 2;

//                if (plugs [plugID].mode == Mode.Manual && !modeOverride) {
//                    plugs [plugID].requestedState = state;
//                    return;
//                }
//
//                if (plugs [plugID].mode == Mode.Auto)
//                    plugs [plugID].requestedState = state;

                // plugs [plugID].currentState = state;
                plugs [plugID].manualState = state;

                byte[] message = new byte[messageLength];

                message [0] = plugID;

                if (state == MyState.On)
                    message [1] = 0xFF;
                else
                    message [1] = 0x00;

                unsafe {
                    fixed (byte* ptr = message) {
                        slave.ReadWrite (
                            30, 
                            ptr, 
                            sizeof(byte) * messageLength, 
                            0, 
                            delegate(CallbackArgs args) {
                                plugs [plugID].OnChangeState (new StateChangeEventArgs (plugID, powerID, state, plugs [plugID].mode));
                            });
                    }
                }

                // @test
                plugs [plugID].OnChangeState (new StateChangeEventArgs (plugID, powerID, state, plugs [plugID].mode));
            }
            #endif

            public void SetPlugMode (byte plugID, Mode mode) {
                plugs [plugID].mode = mode;
                if (plugs [plugID].mode == Mode.Auto)
                    plugs [plugID].OnModeChangedAuto (new ModeChangeEventArgs (plugID, powerID, plugs [plugID].mode));
                else
                    plugs [plugID].OnModeChangedManual (new ModeChangeEventArgs (plugID, powerID, plugs [plugID].mode));
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((slave.Status != AquaPicBusStatus.communicationSuccess) || (slave.Status != AquaPicBusStatus.communicationStart))
                    Alarm.Post (commsAlarmIdx);
            }
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