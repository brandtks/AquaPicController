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
using System.Collections.Generic;
using AquaPic.Utilites;
using AquaPic.Operands;

namespace AquaPic.Drivers
{
    public partial class AnalogOutputBase : GenericBase<int>
    {
        public static AnalogOutputBase SharedAnalogOutputInstance = new AnalogOutputBase ();

        protected AnalogOutputBase () 
            : base ("Analog Output") { }

        protected override void Run () {
            foreach (var card in cards) {
                byte channelId = 0;

                short[] values = new short[4];

                foreach (var genericChannel in card.channels) {
                    var channel = genericChannel as AnalogOutputChannel<int>;

                    if (channel.mode == Mode.Auto) {
                        channel.valueControl.Execute ();
                    }

                    values [channelId] = Convert.ToInt16 (channel.value);

                    ++channelId;
                }

                card.SetAllValuesCommunication<short> (values);
            }
        }

        protected override GenericCard<int> CardCreater (string cardName, int cardId, int address) {
            return new AnalogOutputCard<int> (cardName, cardId, address);
        }

        public AnalogType GetChannelType (string channelName) {
            IndividualControl channel = GetChannelIndividualControl (channelName);
            return GetChannelType (channel.Group, channel.Individual);
        }

        public AnalogType GetChannelType (IndividualControl channel) {
            return GetChannelType (channel.Group, channel.Individual);
        }

        public AnalogType GetChannelType (int card, int channel) {
            CheckCardRange (card);
            var analogOutputCard = cards [card] as AnalogOutputCard<int>;
            return analogOutputCard.GetChannelType (channel);
        }

        public AnalogType[] GetAllChannelTypes (string cardName) {
            int card = GetCardIndex (cardName);
            return GetAllChannelTypes (card);
        }

        public AnalogType[] GetAllChannelTypes (int card) {
            CheckCardRange (card);
            var analogOutputCard = cards [card] as AnalogOutputCard<int>;
            return analogOutputCard.GetAllChannelTypes ();
        }

        public void SetChannelType (string channelName, AnalogType type) {
            IndividualControl channel = GetChannelIndividualControl (channelName);
            SetChannelType (channel.Group, channel.Individual, type);
        }

        public void SetChannelType (IndividualControl channel, AnalogType type) {
            SetChannelType (channel.Group, channel.Individual, type);
        }

        public void SetChannelType (int card, int channel, AnalogType type) {
            CheckCardRange (card);
            var analogOutputCard = cards [card] as AnalogOutputCard<int>;
            analogOutputCard.SetChannelType (channel, type);
        }

        public Value GetChannelValueControl (string channelName) {
            IndividualControl channel = GetChannelIndividualControl (channelName);
            return GetChannelValueControl (channel.Group, channel.Individual);
        }

        public Value GetChannelValueControl (IndividualControl channel) {
            return GetChannelValueControl (channel.Group, channel.Individual);
        }

        public Value GetChannelValueControl (int card, int channel) {
            CheckCardRange (card);
            var analogOutputCard = cards [card] as AnalogOutputCard<int>;
            return analogOutputCard.GetChannelValueControl (channel);
        }
    }
}