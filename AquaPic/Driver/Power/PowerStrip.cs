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
    public partial class PowerBase
    {
        class PowerStrip : GenericOutputCard
        {
            public int powerLossAlarmIndex;
            public bool acPowerAvailable;

            public PowerStrip (string name, int address)
                : base (name, address, 8) {
                acPowerAvailable = false;
                powerLossAlarmIndex = Alarm.Subscribe (string.Format ("{0}: Loss of Power", name));
            }

            protected override GenericOutputChannel OutputChannelCreater (int index) {
                return new PowerOutlet (GetDefualtName (index));
            }

            public override string GetChannelPrefix () {
                return "p";
            }

            protected void SetupOutletCommunication (int outlet, MyState fallback) {
                byte valueToSend = 0x00;
                if (fallback == MyState.On) {
                    valueToSend = 0xFF;
                }
                var buf = new WriteBuffer ();
                buf.Add ((byte)outlet, sizeof (byte));
                buf.Add (valueToSend, sizeof (byte));
                Write (2, buf, true);
            }

            public void GetStatusCommunication () {
                Read (20, 3, GetStatusCallback);
            }

            protected void GetStatusCallback (CallbackArgs callArgs) {
                acPowerAvailable = callArgs.GetDataFromReadBuffer<bool> (0);

                if (!acPowerAvailable) {
                    Alarm.Post (powerLossAlarmIndex);
                } else {
                    Alarm.Clear (powerLossAlarmIndex);
                }

                byte stateMask = callArgs.GetDataFromReadBuffer<byte> (1);
                for (var i = 0; i < channels.Length; ++i) {
                    channels[i].SetValue (Utils.MaskToBoolean (stateMask, i));
                }

#if INCLUDE_POWER_STRIP_CURRENT
                var currentMask = callArgs.GetDataFromReadBuffer<byte> (2);
                for (var i = 0; i < channels.Length; ++i) {
                    if (Utils.MaskToBoolean (currentMask, i))
                        ReadOutletCurrentCommunication (i);
                }
#endif
            }

            public void ReadOutletCurrentCommunication (int outlet) {
                CheckChannelRange (outlet);
                ReadWrite (10, (byte)outlet, 5, ReadOutletCurrentCallback);
            }

            protected void ReadOutletCurrentCallback (CallbackArgs callArgs) {
                var outletIndex = callArgs.GetDataFromReadBuffer<byte> (0);
                var outlet = channels[outletIndex] as PowerOutlet;
                outlet.SetAmperage (callArgs.GetDataFromReadBuffer<float> (1));
            }

            public override void SetValueCommunication (int channel, ValueType value) {
                CheckChannelRange (channel);
                var buf = new WriteBuffer ();
                buf.Add ((byte)channel, sizeof (byte));
                buf.Add (Convert.ToBoolean (value), sizeof (bool));
                Write (30, buf);
            }

            public MyState GetOutletFallback (int outlet) {
                CheckChannelRange (outlet);
                var powerOutlet = channels[outlet] as PowerOutlet;
                return powerOutlet.fallback;
            }

            public void SetOutletFallback (int outlet, MyState fallback) {
                CheckChannelRange (outlet);
                var powerOutlet = channels[outlet] as PowerOutlet;
                powerOutlet.fallback = fallback;
                SetupOutletCommunication (outlet, fallback);
            }
        }
    }
}

