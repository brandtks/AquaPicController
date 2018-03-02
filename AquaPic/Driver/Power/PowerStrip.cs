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
using GoodtimeDevelopment.Utilites;
using AquaPic.SerialBus;
using AquaPic.Runtime;
using AquaPic.Globals;

namespace AquaPic.Drivers
{
    public partial class Power
    {
        private class PowerStrip : AquaPicBus.Slave
        {
            public byte powerID;
            public int powerLossAlarmIndex;
            public bool AcPowerAvailable;
            public string name;
            public OutletData[] outlets;

            public bool AquaPicBusCommunicationOk {
                get {
                    return ((Status == AquaPicBusStatus.CommunicationStart) || (Status == AquaPicBusStatus.CommunicationSuccess));
                }
            }

            public PowerStrip (byte address, byte powerID, string name, bool alarmOnLossOfPower, int powerLossAlarmIndex) 
                : base (address, name + " (Power Strip)")
            {
                this.powerID = powerID;

                if (alarmOnLossOfPower && (powerLossAlarmIndex == -1)) {
                    this.powerLossAlarmIndex = Alarm.Subscribe ("Loss of power");
                } else {
                    this.powerLossAlarmIndex = powerLossAlarmIndex;
                }

                this.name = name;
                AcPowerAvailable = false;

                outlets = new OutletData[8];
                for (int i = 0; i < 8; ++i) {
                    int plugID = i;
                    string plugName = this.name + "." + "p" + plugID.ToString() ;
                        
                    outlets [plugID] = new OutletData (
                        plugName,
                        (state) => {
                            if (state) {
                                if (outlets[plugID].currentState != MyState.On)
                                    SetOutletState ((byte)plugID, MyState.On);
                            } else {
                                if (outlets[plugID].currentState != MyState.Off)
                                    SetOutletState ((byte)plugID, MyState.Off);
                            }
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

                Write (2, message, true);
            }

            public void GetStatus () {
                Read (20, 3, GetStatusCallback);
            }

            protected void GetStatusCallback (CallbackArgs callArgs) {
                if (Status != AquaPicBusStatus.CommunicationSuccess)
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
                ReadWrite (10, outletId, 5, ReadOutletCurrentCallback);
            }

            protected void ReadOutletCurrentCallback (CallbackArgs callArgs) {
                if (Status != AquaPicBusStatus.CommunicationSuccess)
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

                ReadWrite (
                    30,
                    message,
                    0, // this just returns the default response so there is no data, ie 0
                    (args) => {
                        outlets [outletId].currentState = state;
                        OnStateChange (outlets [outletId], new StateChangeEventArgs (outletId, powerID, state));
                    });


                #if !RPI_BUILD
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

