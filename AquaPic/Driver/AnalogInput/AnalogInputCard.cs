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
using AquaPic.Utilites;
using AquaPic.SerialBus;

namespace AquaPic.Drivers
{
    public partial class AnalogInputBase
    {
        protected class AnalogInputCard<T> : GenericCard<T>
        {
            public AnalogInputCard (string name, int cardId, int address)
                : base (
                    name, 
                    CardType.AnalogInputCard, 
                    cardId,
                    address,
                    4) { }

            protected override GenericChannel<T> ChannelCreater (int index) {
                return new AnalogInputChannel<T> (GetDefualtName (index));
            }

            public override void GetValueCommunication (int channel) {
                CheckChannelRange (channel);

                if (channels [channel].mode == Mode.Auto) {
                    ReadWrite (10, (byte)channel, 3, GetValueCommunicationCallback);
                }
            }

            protected void GetValueCommunicationCallback (CallbackArgs args) {
                byte ch = args.GetDataFromReadBuffer<byte> (0);
                short value = args.GetDataFromReadBuffer<short> (1);
                channels [ch].SetValue (value);
            }

            public override void GetAllValuesCommunication () {
                Read (20, sizeof(short) * 4, GetAllValuesCommunicationCallback);
            }

            protected void GetAllValuesCommunicationCallback (CallbackArgs args) {
                short[] values = new short[4];

                for (int i = 0; i < values.Length; ++i) {
                    values [i] = args.GetDataFromReadBuffer<short> (i * 2);
                }

                for (int i = 0; i < channels.Length; ++i) {
                    if (channels [i].mode == Mode.Auto)
                        channels [i].SetValue (values [i]);
                }
            }

            public override void SetChannelValue (int channel, T value) {
                CheckChannelRange (channel);

                if (channels [channel].mode == Mode.Manual) {
                    channels [channel].SetValue (value);
                } else {
                    throw new Exception ("Can only modify analong input value with channel forced");
                }
            }

            public override void SetAllChannelValues (T[] values) {
                if (values.Length != channels.Length)
                    throw new ArgumentOutOfRangeException ("values length");

                for (int i = 0; i < channels.Length; ++i) {
                    if (channels [i].mode == Mode.Manual) {
                        channels [i].SetValue (values [i]);
                    }
                }
            }
        }
    }
}

