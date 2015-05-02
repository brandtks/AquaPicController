using System;
using System.Collections.Generic;
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
            public OutletData[] Outlets;

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

                this.Outlets = new OutletData[8];
                for (int i = 0; i < 8; ++i) {
                    int plugID = i;
                    string plugName = this.name + "." + "p" + plugID.ToString() ;
                        
                    this.Outlets [plugID] = new OutletData (
                        plugName,
                        delegate() {
                            SetOutletState ((byte)plugID, MyState.On, false);
                        },
                        delegate() {
                            SetOutletState ((byte)plugID, MyState.Off, false);
                        });
                }
            }

            #if SIMULATION
            public void SetupOutlet (byte outletID, MyState fallback) {
                const int messageLength = 2;
                string[] message = new string[messageLength];
                message[0] = outletID.ToString ();
                if (fallback == MyState.On)
                    message[1] = "true";
                else
                    message[1] = "false";

                slave.Write (2, message, messageLength);
            }
            #else
            public unsafe void SetupOutlet (byte outletID, MyState fallback) {
                const int messageLength = 2; 

                byte[] message = new byte[messageLength];

                message [0] = outletID;

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
                for (int i = 0; i < Outlets.Length; ++i) {
                    if (Utils.mtob (mask, i))
                        ReadOutletCurrent (i);
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

                for (int i = 0; i < Outlets.Length; ++i) {
                    if (Utils.mtob (status.currentAvailableMask, i))
                        ReadOutletCurrent ((byte)i);
                }
            }
            #endif

            #if SIMULATION
            public void ReadOutletCurrent (int i) {
                const int messageLength = 1;
                string[] m = new string[messageLength];
                m [0] = i.ToString ();
                slave.ReadWrite (10, m, messageLength, 2, ReadOutletCurrentCallback);
            }
            #else
            public void ReadOutletCurrent (byte outletID) {
                unsafe {
                    slave.ReadWrite (10, &outletID, sizeof (byte), sizeof (AmpComms), ReadOutletCurrentCallback);
                }
            }
            #endif

            #if SIMULATION
            protected void ReadOutletCurrentCallback (CallbackArgs callArgs) {
                int plug = Convert.ToInt32(callArgs.readMessage [3]);
                float current = Convert.ToSingle(callArgs.readMessage [4]);
                Outlets [plug].SetAmpCurrent (current);

            }
            #else
            protected void ReadOutletCurrentCallback (CallbackArgs callArgs) {
                if (slave.Status != AquaPicBusStatus.communicationSuccess)
                    return;

                AmpComms message;

                unsafe {
                    callArgs.copyBuffer (&message, sizeof(AmpComms));
                }

                Outlets [message.outletID].SetAmpCurrent (message.current);

            }
            #endif

            #if SIMULATION
            public void SetOutletState (byte outletID, MyState state, bool modeOverride) {
                if ((state != Outlets [outletID].currentState) && (Outlets [outletID].Updated)) {
                    Outlets [outletID].Updated = false;

                    const int messageLength = 2;
                    string[] message = new string[messageLength];
                    message [0] = outletID.ToString ();
                    if (state == MyState.On)
                        message [1] = "true";
                    else
                        message [1] = "false";

                    slave.ReadWrite (30, message, messageLength, 0, 
                        delegate (CallbackArgs args) {
                            Outlets [outletID].OnChangeState (new StateChangeEventArgs (outletID, powerID, state, Outlets [outletID].mode));
                        });
                }
            }
            #else
            public void SetOutletState (byte outletID, MyState state, bool modeOverride) {
                const int messageLength = 2;

//                if (plugs [plugID].mode == Mode.Manual && !modeOverride) {
//                    plugs [plugID].requestedState = state;
//                    return;
//                }
//
//                if (plugs [plugID].mode == Mode.Auto)
//                    plugs [plugID].requestedState = state;

                // plugs [plugID].currentState = state;
                Outlets [outletID].manualState = state;

                byte[] message = new byte[messageLength];

                message [0] = outletID;

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
                                Outlets [outletID].OnChangeState (new StateChangeEventArgs (outletID, powerID, state, Outlets [outletID].mode));
                            });
                    }
                }

                // @test
                Outlets [outletID].OnChangeState (new StateChangeEventArgs (outletID, powerID, state, Outlets [outletID].mode));
            }
            #endif

            public void SetPlugMode (byte outletID, Mode mode) {
                Outlets [outletID].mode = mode;
                if (Outlets [outletID].mode == Mode.Auto)
                    Outlets [outletID].OnModeChangedAuto (new ModeChangeEventArgs (outletID, powerID, Outlets [outletID].mode));
                else
                    Outlets [outletID].OnModeChangedManual (new ModeChangeEventArgs (outletID, powerID, Outlets [outletID].mode));
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((slave.Status != AquaPicBusStatus.communicationSuccess) || (slave.Status != AquaPicBusStatus.communicationStart))
                    Alarm.Post (commsAlarmIdx);
            }
    	}
    }
}