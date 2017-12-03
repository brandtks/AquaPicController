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
using AquaPic.Operands;
using AquaPic.Globals;
using AquaPic.SerialBus;

namespace AquaPic.Drivers
{
    public partial class AnalogOutputBase
    {
        protected class AnalogOutputCard<T> : GenericCard<T>
        {
            public AnalogOutputCard (string name, int cardId, int address)
                : base (
                    name, 
                    CardType.AnalogOutputCard, 
                    cardId,
                    address,
                    4) { }

            protected override GenericChannel<T> ChannelCreater (int index) {
                return new AnalogOutputChannel<T> (GetDefualtName (index));
            }

            public override void GetValueCommunication (int channel) {
                CheckChannelRange (channel);
                ReadWrite (10, (byte)channel, 3, GetValueCommunicationCallback); // byte channel id and float value, 5 bytes
            }

            protected void GetValueCommunicationCallback (CallbackArgs args) {
                var ch = args.GetDataFromReadBuffer<byte>(0);
                var value = args.GetDataFromReadBuffer<float>(1);
                channels[ch].SetValue (value);
            }

            public override void SetValueCommunication<CommunicationType> (int channel, CommunicationType value) {
                CheckChannelRange (channel);
                channels[channel].SetValue (value);
                var valueToSend = Convert.ToSingle (value);
                var buf = new WriteBuffer ();
                buf.Add ((byte)channel, sizeof(byte));
                buf.Add (valueToSend, sizeof(float));
                Write (31, buf);
            }

            public override void SetAllValuesCommunication<CommunicationType> (CommunicationType[] values) {
                if (values.Length < channelCount)
                    throw new ArgumentOutOfRangeException (nameof (values), "values length");

                var valuesToSend = new float[channelCount];
                for (int i = 0; i < 4; ++i) {
                    channels [i].SetValue (values [i]);
                    valuesToSend [i] = Convert.ToSingle (values [i]);
                }

                var buf = new WriteBuffer ();
                foreach (var val in valuesToSend) {
                    buf.Add (val, sizeof (float));
                }
                Write (30, buf);
            }

            public override void GetAllValuesCommunication () {
                Read (20, sizeof(float) * 4, GetAllValuesCommunicationCallback);
            }

            protected void GetAllValuesCommunicationCallback (CallbackArgs args) {
                var values = new float[4];

                for (int i = 0; i < values.Length; ++i) {
                    values [i] = args.GetDataFromReadBuffer<float> (i * 4);
                }

                for (int i = 0; i < channels.Length; ++i) {
                    channels [i].SetValue (values [i]);
                }
            }

            public void SetChannelType (int channel, AnalogType type) {
                CheckChannelRange (channel);
                #if SELECTABLE_ANALOG_OUTPUT_TYPE
                channels [ch].type = type;

                byte[] arr = new byte[2];
                arr [0] = (byte)ch;
                arr [1] = (byte)channels [ch].type;

                Write (2, arr);
                #else
                if (type != AnalogType.ZeroTen)
                    throw new Exception ("Dimming card only does 0-10V");
                #endif
            }

            public AnalogType GetChannelType (int channel) {
                CheckChannelRange (channel);
                var analogOutputChannel = channels [channel] as AnalogOutputChannel<T>;
                return analogOutputChannel.type;
            }

            public AnalogType[] GetAllChannelTypes () {
                AnalogType[] types = new AnalogType[channelCount];
                for (int i = 0; i < channelCount; ++i) {
                    var analogOutputChannel = channels [i] as AnalogOutputChannel<T>;
                    types [i] = analogOutputChannel.type;
                }
                return types;
            }

            public Value GetChannelValueControl (int channel) {
                CheckChannelRange (channel);
                var analogOutputChannel = channels [channel] as AnalogOutputChannel<T>;
                return analogOutputChannel.valueControl;
            }
        }
    }
}

