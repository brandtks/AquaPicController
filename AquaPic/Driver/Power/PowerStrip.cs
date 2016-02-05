using System;
using System.Collections.Generic;
using AquaPic;
using AquaPic.SerialBus;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public partial class Power
    {
        private class PowerStrip {
            public AquaPicBus.Slave slave;
            public byte powerID;
            public int powerLossAlarmIndex;
            public bool AcPowerAvailable;
            public string name;
            public OutletData[] outlets;

            public PowerStrip (byte address, byte powerID, string name, bool alarmOnLossOfPower, int powerLossAlarmIndex) {
                this.slave = new AquaPicBus.Slave (address, name + " (Power Strip)");
                this.powerID = powerID;

                if (alarmOnLossOfPower && (powerLossAlarmIndex == -1)) {
                    this.powerLossAlarmIndex = Alarm.Subscribe ("Loss of power");
                } else {
                    this.powerLossAlarmIndex = powerLossAlarmIndex;
                }

                this.name = name;
                this.AcPowerAvailable = false;

                this.outlets = new OutletData[8];
                for (int i = 0; i < 8; ++i) {
                    int plugID = i;
                    string plugName = this.name + "." + "p" + plugID.ToString() ;
                        
                    this.outlets [plugID] = new OutletData (
                        plugName,
                        () => {
                            if (outlets [plugID].currentState != MyState.On)
                                SetOutletState ((byte)plugID, MyState.On);
                        },
                        () => {
                            if (outlets [plugID].currentState != MyState.Off)
                                SetOutletState ((byte)plugID, MyState.Off);
                        });
                }
            }

            public void SetupOutlet (int outletId, MyState fallback) {
                const int messageLength = 2; 
                byte[] message = new byte[messageLength];

                message [0] = (byte)outletId;

                if (fallback == MyState.On)
                    message [1] = 0xFF;
                else
                    message [1] = 0x00;

                #if UNSAFE_COMMS
                unsafe {
                    fixed (byte* ptr = message) {
                        slave.Write (2, ptr, sizeof(byte) * messageLength);
                    }
                }
                #else
                slave.Write (2, message);
                #endif
            }


            public void GetStatus () {
                #if UNSAFE_COMMS
                unsafe {
                    slave.Read (20, sizeof(PowerComms), GetStatusCallback);
                }
                #else
                slave.Read (20, 2, GetStatusCallback);
                #endif
            }

            protected void GetStatusCallback (CallbackArgs callArgs) {
                if (slave.Status != AquaPicBusStatus.communicationSuccess)
                    return;

                #if UNSAFE_COMMS
                PowerComms status = new PowerComms ();

                unsafe {
                    callArgs.CopyBuffer (&status, sizeof(PowerComms));
                }

                AcPowerAvailable = status.acPowerAvailable;
                if (!AcPowerAvailable)
                    Alarm.Post (powerLossAlarmIndex);

                for (int i = 0; i < outlets.Length; ++i) {
                    if (Utils.mtob (status.currentAvailableMask, i))
                        ReadOutletCurrent ((byte)i);
                }
                #else
                AcPowerAvailable = callArgs.GetDataFromReadBuffer<bool> (0);

                if (!AcPowerAvailable)
                    Alarm.Post (powerLossAlarmIndex);

                #if INCLUDE_POWER_STRIP_CURRENT
                byte currentMask = callArgs.GetDataFromReadBuffer<byte> (1);
                for (int i = 0; i < outlets.Length; ++i) {
                    if (Utils.mtob (currentMask, i))
                        ReadOutletCurrent ((byte)i);
                }
                #endif //INCLUDE_POWER_STRIP_CURRENT
                #endif //UNSAFE_COMMS
            }

            public void ReadOutletCurrent (byte outletId) {
                #if UNSAFE_COMMS
                unsafe {
                    slave.ReadWrite (10, &outletId, sizeof (byte), sizeof (AmpComms), ReadOutletCurrentCallback);
                }
                #else
                slave.ReadWrite (10, outletId, 5, ReadOutletCurrentCallback);
                #endif
            }

            protected void ReadOutletCurrentCallback (CallbackArgs callArgs) {
                if (slave.Status != AquaPicBusStatus.communicationSuccess)
                    return;

                #if UNSAFE_COMMS
                AmpComms message;

                unsafe {
                    callArgs.CopyBuffer (&message, sizeof(AmpComms));
                }

                outlets [message.outletID].SetAmpCurrent (message.current);
                #else
                int outletId = callArgs.GetDataFromReadBuffer<byte> (0);
                outlets [outletId].SetAmpCurrent (callArgs.GetDataFromReadBuffer<float> (1));
                #endif
            }

            public void SetOutletState (byte outletId, MyState state) {
                const int messageLength = 2;

                outlets [outletId].manualState = state;

                byte[] message = new byte[messageLength];
                message [0] = outletId;

                if (state == MyState.On)
                    message [1] = 0xFF;
                else
                    message [1] = 0x00;

                #if UNSAFE_COMMS
                unsafe {
                    fixed (byte* ptr = message) {
                        slave.ReadWrite (
                            30, 
                            ptr, 
                            sizeof(byte) * messageLength, 
                            0, 
                            (args) => {
                                //outlets [outletId].OnChangeState (new StateChangeEventArgs (outletId, powerID, state)));
                                outlets [outletId].currentState = state;
                                OnStateChange (outlets [outletId], new StateChangeEventArgs (outletId, powerID, state));
                            });
                    }
                }
                #else
                slave.ReadWrite (
                    30,
                    message,
                    0, // this just returns the default response so there is no data, ie 0
                    (args) => {
                        outlets [outletId].currentState = state;
                        OnStateChange (outlets [outletId], new StateChangeEventArgs (outletId, powerID, state));
                    });
                #endif


                #if !RPI_BUILD
                //<TEST> this is here only because the slave never responds so the callback never happens
                outlets [outletId].currentState = state;
                OnStateChange (outlets [outletId], new StateChangeEventArgs (outletId, powerID, state));
                #endif
            }

            public void SetPlugMode (byte outletId, Mode mode) {
                outlets [outletId].mode = mode;
                OnModeChange (outlets [outletId], new ModeChangeEventArgs (outletId, powerID, outlets [outletId].mode));
            }
    	}
    }
}