using System;
using AquaPic;
using AquaPic.SerialBus;
using AquaPic.Alarm;
using AquaPic.Utilites;

namespace AquaPic.Power
{
    public class powerStrip
	{
        private byte _address;
        private byte _powerID;
        private int _commsAlarmIdx;
        private int _powerAvailAlarmIdx;
        public byte address {
            get { return _address; }
        }
        public byte powerID {
            get { return _powerID; }
        }
        public int powerAvailAlarmIdx {
            get { return _powerAvailAlarmIdx; }
        }
        public string name;
        public APBstatus commsStatus { get; set; }
        public bool acPowerAvail { get; set; }
        public plugData[] plugs { get; set; }

        /* <For Future Expansion>
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

        public powerStrip (byte address, byte powerID, string name) {
            this._address = address;
            this._powerID = powerID;
            this._commsAlarmIdx = alarm.subscribe ("APB communication error", "Power Strip at address " + this.address.ToString ());
            this._powerAvailAlarmIdx = alarm.subscribe ("Loss of power", "Mains power not available at address " + this.address.ToString ());
            this.name = name;
            this.commsStatus = APBstatus.notOpen;
            this.acPowerAvail = false;
            this.plugs = new plugData[8];
            for (int i = 0; i < 8; ++i) {
                this.plugs [i] = new plugData ();
            }
        }

        public void getStatus () {
            pwrComms status;

            unsafe {
                commsStatus = APB.read (_address, 1, &status, sizeof(pwrComms));
            }

            if (commsStatus != APBstatus.readSuccess) {
                alarm.post (_commsAlarmIdx, true);
                return;
            }

            for (int i = 0; i < 8; ++i) {
                bool s = mtob (status.stateMask, i);
                if (s != plugs [i].currentState) {
                    plugs [i].currentState = s;
                    stateChangeEventArgs args = new stateChangeEventArgs (i, powerID, plugs [i].mode, plugs [i].currentState);
                    plugs [i].onChangeState (args);
                }

                bool m = mtob (status.modeMask, i); 
                if (m && (plugs [i].mode == Mode.Manual)) { // m true is auto
                    plugs [i].mode = Mode.Auto; 
                    modeChangeEventArgs args = new modeChangeEventArgs (i, _address, plugs [i].mode);
                    plugs [i].onModeChangedAuto (args);
                } else if (!m && (plugs [i].mode == Mode.Auto)) { // m false is manual
                    plugs [i].mode = Mode.Manual;
                    modeChangeEventArgs args = new modeChangeEventArgs (i, _address, plugs [i].mode);
                    plugs [i].onModeChangedManual (args);
                }
            }
           
            acPowerAvail = status.acPowerAvail;
            if (!acPowerAvail) {
                alarm.post (_powerAvailAlarmIdx, true);
            }
        }

        public bool setPlugState (byte plugID, bool state, bool modeOverride) {
            plugComms status;
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
                    commsStatus = APB.readWrite (_address, 2, ptr, sizeof(byte) * 3, &status, sizeof(plugComms));
                }
            }

            if (commsStatus != APBstatus.readWriteSuccess) {
                alarm.post (_commsAlarmIdx, true);
                return false;
            }

            plugs [plugID].mode = (Mode)status.mode;

            if (plugs [plugID].requestedState == status.state) { // the state changed
                plugs [plugID].currentState = status.state;
                stateChangeEventArgs args = new stateChangeEventArgs (plugID, powerID, plugs [plugID].mode, plugs [plugID].currentState);
                plugs [plugID].onChangeState (args);
                return true;
            }

            return false;
        }

        /* <TODO> this need lots of work */
//        public void setAllPlugsState (byte mask) {
//            for (int i = 0; i < 8; ++i)
//                plugs [i].requestedState = mtob (mask, i);
//
//            unsafe {
//                commsStatus = APB.write (_address, 2, &mask, sizeof(byte));
//            }
//
//            if (commsStatus != APBstatus.writeSuccess) {
//                alarm.post (_alarmIdx, true);
//            }
//        }

        public void setPlugMode (byte plugID, Mode mode) {
            byte[] message = new byte[2];
            plugs [plugID].mode = mode;

            message [0] = plugID;
            message [1] = (byte)plugs [plugID].mode;

            unsafe {
                fixed (byte* ptr = message) {
                    commsStatus = APB.write (_address, 3, ptr, sizeof(byte) * 2);
                }
            }

            if (commsStatus != APBstatus.writeSuccess) {
                alarm.post (_commsAlarmIdx, true);
            }
        }

        public void setAllPlugsMode (Mode mode) {
            byte message;

            for (int i = 0; i < 8; ++i)
                plugs [i].mode = mode;

            if (plugs [0].mode == Mode.Auto)
                message = 0xFF;
            else
                message = 0x00;

            unsafe {
                commsStatus = APB.write (_address, 4, &message, sizeof(byte));
            }

            if (commsStatus != APBstatus.writeSuccess) {
                alarm.post (_commsAlarmIdx, true);
            }
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

