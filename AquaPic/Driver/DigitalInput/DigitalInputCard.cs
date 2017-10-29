#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using GoodtimeDevelopment.Utilites;
using AquaPic.SerialBus;

namespace AquaPic.Drivers
{
    public partial class DigitalInputBase
    {
        private class DigitalInputCard<T> : GenericCard<T>
        {
            public DigitalInputCard (string name, int cardId, int address)
                : base (
                    name, 
                    CardType.DigitalInputCard, 
                    cardId,
                    address,
                    6) { }

            protected override GenericChannel<T> ChannelCreater (int index) {
                return new DigitalInputChannel<T> (GetDefualtName (index));
            }

            public override void GetAllValuesCommunication () {
                Read (20, sizeof(byte), GetInputsCallback);
            }

            protected void GetInputsCallback (CallbackArgs args) {
                byte stateMask;
                stateMask = args.GetDataFromReadBuffer<byte> (0);
                for (int i = 0; i < channelCount; ++i) {
                    channels[i].SetValue (Utils.MaskToBoolean (stateMask, i));
                }
            }

            public override void GetValueCommunication (int channel) {
                CheckChannelRange (channel);
                ReadWrite (10, (byte)channel, 2, GetInputCallback);
            }

            protected void GetInputCallback (CallbackArgs args) {
                var ch = args.GetDataFromReadBuffer<byte> (0);
                var value = args.GetDataFromReadBuffer<bool> (1);
                channels [ch].SetValue(value);
            }
        }
    }
}

