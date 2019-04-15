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
using AquaPic.Globals;
using AquaPic.Operands;

namespace AquaPic.Drivers
{
    public partial class AnalogOutputBase : GenericOutputBase
    {
        public static AnalogOutputBase SharedAnalogOutputInstance = new AnalogOutputBase ();

        protected AnalogOutputBase ()
            : base ("Analog Output") { }

        protected override GenericCard CardCreater (string cardName, int address) {
            return new AnalogOutputCard (cardName, address);
        }

        public override string GetCardAcyronym () {
            return "AQ";
        }

        public override CardType GetCardType () {
            return CardType.AnalogOutput;
        }

        public AnalogType GetChannelType (string channelName) {
            IndividualControl channel = GetChannelIndividualControl (channelName);
            return GetChannelType (channel.Group, channel.Individual);
        }

        public AnalogType GetChannelType (IndividualControl channel) {
            return GetChannelType (channel.Group, channel.Individual);
        }

        public AnalogType GetChannelType (string card, int channel) {
            CheckCardKey (card);
            var analogOutputCard = cards[card] as AnalogOutputCard;
            return analogOutputCard.GetChannelType (channel);
        }

        public AnalogType[] GetAllChannelTypes (string card) {
            CheckCardKey (card);
            var analogOutputCard = cards[card] as AnalogOutputCard;
            return analogOutputCard.GetAllChannelTypes ();
        }

        public void SetChannelType (string channelName, AnalogType type) {
            IndividualControl channel = GetChannelIndividualControl (channelName);
            SetChannelType (channel.Group, channel.Individual, type);
        }

        public void SetChannelType (IndividualControl channel, AnalogType type) {
            SetChannelType (channel.Group, channel.Individual, type);
        }

        public void SetChannelType (string card, int channel, AnalogType type) {
            CheckCardKey (card);
            var analogOutputCard = cards[card] as AnalogOutputCard;
            analogOutputCard.SetChannelType (channel, type);
        }
    }
}

