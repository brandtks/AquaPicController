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

            public bool AquaPicBusCommunicationOk {
                get {
                    return ((slave.Status == AquaPicBusStatus.CommunicationStart) || (slave.Status == AquaPicBusStatus.CommunicationSuccess));
                }
            }

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

                slave.Write (2, message, true);
            }

            public void GetStatus () {
                slave.Read (20, 3, GetStatusCallback);
            }

            protected void GetStatusCallback (CallbackArgs callArgs) {
                if (slave.Status != AquaPicBusStatus.CommunicationSuccess)
                    return;

                AcPowerAvailable = callArgs.GetDataFromReadBuffer<bool> (0);

                if (!AcPowerAvailable)
                    Alarm.Post (powerLossAlarmIndex);

                byte stateMask = callArgs.GetDataFromReadBuffer<byte> (1);
                for (int i = 0; i < outlets.Length; ++i) {
                    if (Utils.MaskToBoolean (stateMask, i)) {
                        outlets [i].currentState = MyState.On;
                    } else {
                        outlets [i].currentState = MyState.Off;
                    }
                }

                #if INCLUDE_POWER_STRIP_CURRENT
                byte currentMask = callArgs.GetDataFromReadBuffer<byte> (2);
                for (int i = 0; i < outlets.Length; ++i) {
                    if (Utils.MaskToBoolean (currentMask, i))
                        ReadOutletCurrent ((byte)i);
                }
                #endif
            }

            public void ReadOutletCurrent (byte outletId) {
                slave.ReadWrite (10, outletId, 5, ReadOutletCurrentCallback);
            }

            protected void ReadOutletCurrentCallback (CallbackArgs callArgs) {
                if (slave.Status != AquaPicBusStatus.CommunicationSuccess)
                    return;

                int outletId = callArgs.GetDataFromReadBuffer<byte> (0);
                outlets [outletId].SetAmpCurrent (callArgs.GetDataFromReadBuffer<float> (1));
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

                slave.ReadWrite (
                    30,
                    message,
                    0, // this just returns the default response so there is no data, ie 0
                    (args) => {
                        outlets [outletId].currentState = state;
                        OnStateChange (outlets [outletId], new StateChangeEventArgs (outletId, powerID, state));
                    });


                #if !RPI_BUILD
                //<TEST> this is here only because the slave never responds so the callback never happens
                //outlets [outletId].currentState = state;
                //OnStateChange (outlets [outletId], new StateChangeEventArgs (outletId, powerID, state));
                #endif
            }

            public void SetPlugMode (byte outletId, Mode mode) {
                outlets [outletId].mode = mode;
                OnModeChange (outlets [outletId], new ModeChangeEventArgs (outletId, powerID, outlets [outletId].mode));
            }
    	}
    }
}