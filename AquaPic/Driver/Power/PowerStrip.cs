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
            private AquaPicBus.Slave _apb;
            private byte _powerID;
            private int _commsAlarmIdx;
            private int _powerAvailAlarmIdx;

            public byte address {
                get { return _apb.address; }
            }
            public byte powerID {
                get { return _powerID; }
            }
            public int commsAlarmIdx {
                get { return _commsAlarmIdx; }
            }
            public int powerAvailAlarmIdx {
                get { return _powerAvailAlarmIdx; }
            }
            public string name;
            public bool acPowerAvail { get; set; }
            public PlugData[] plugs { get; set; }

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
                    this._apb = new AquaPicBus.Slave (AquaPicBus.Bus1, address);
                    this._apb.OnStatusUpdate += OnSlaveStatusUpdate;
                } catch (Exception ex) {
                    Console.WriteLine (ex.ToString ());
                    Console.WriteLine (ex.Message);
                }
                this._powerID = powerID;
                this._commsAlarmIdx = Alarm.Subscribe ("APB communication error", "Power Strip at address " + this._apb.address.ToString ());
                this._powerAvailAlarmIdx = Alarm.Subscribe ("Loss of power", "Mains power not available at address " + this._apb.address.ToString ());
                this.name = name;
                this.acPowerAvail = false;
                this.plugs = new PlugData[8];
                for (int i = 0; i < 8; ++i) {
                    this.plugs [i] = new PlugData ();
                }
            }

            public unsafe void GetStatus () {
                _apb.Read (20, sizeof(pwrComms), GetStatusCallback);
            }

            protected void GetStatusCallback (CallbackArgs callArgs) {
                pwrComms status;

                if (_apb.status != AquaPicBusStatus.communicationSuccess)
                    return;

                unsafe {
                    callArgs.copyBuffer (&status, sizeof(pwrComms));
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
                        _apb.ReadWrite (11, ptr, sizeof(byte) * 3, sizeof(plugComms), SetPlugStateCallback);
                    }
                }
            }

            protected void SetPlugStateCallback (CallbackArgs a) {
                plugComms status;

                if (_apb.status != AquaPicBusStatus.communicationSuccess)
                    return;

                unsafe {
                    a.copyBuffer (&status, sizeof(plugComms));
                }

                byte plugID = status.plug;
                plugs [plugID].mode = (Mode)status.mode;

                if (plugs [plugID].requestedState == status.state) { // the state changed
                    plugs [plugID].currentState = status.state;
                    stateChangeEventArgs args = new stateChangeEventArgs (plugID, powerID, plugs [plugID].mode, plugs [plugID].currentState);
                    plugs [plugID].OnChangeState (args);
                }
            }

            // <TODO> this need lots of work
            // need to determine how to handle the callback
    //        public void setAllPlugsState (byte mask) {
    //            for (int i = 0; i < 8; ++i)
    //                plugs [i].requestedState = mtob (mask, i);
    //
    //            unsafe {
    //                commsStatus = APB.write (_address, 10, &mask, sizeof(byte));
    //            }
    //
    //            if (commsStatus != APBstatus.writeSuccess) {
    //                alarm.post (_alarmIdx, true);
    //            }
    //        }

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

            // <TODO> cann't decide if I want to set all the modes the same
            // or be able to only set certain ones 
//            public void SetAllPlugsMode (Mode mode) {
//                byte message;
//
//                for (int i = 0; i < 8; ++i)
//                    plugs [i].mode = mode;
//
//                if (plugs [0].mode == Mode.Auto)
//                    message = 0xFF;
//                else
//                    message = 0x00;
//
//                unsafe {
//                    _apb.Write (30, &message, sizeof(byte));
//                }
//            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((_apb.status != AquaPicBusStatus.communicationSuccess) || (_apb.status != AquaPicBusStatus.communicationStart))
                    Alarm.Post (_commsAlarmIdx, true);
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

            private static void btom(ref byte mask, bool b, int shift) {
                if (b)
                    mask |= (byte)Math.Pow (2, shift);
                else {
                    int m = ~(int)Math.Pow (2, shift);
                    mask &= (byte) m;
                }
            }
    	}
    }
}

