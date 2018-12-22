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
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Drivers
{
    public partial class AnalogInputBase : GenericInputBase
    {
        public static AnalogInputBase SharedAnalogInputInstance = new AnalogInputBase ();

        protected AnalogInputBase ()
            : base ("Analog Input") { }

        protected override void Run () {
            foreach (var card in cards.Values) {
                card.GetAllValuesCommunication ();
            }
        }

        protected override GenericCard CardCreater (string cardName, int address) {
            return new AnalogInputCard (cardName, address);
        }

        public override string GetCardAcyronym () {
            return "AI";
        }

        public override CardType GetCardType () {
            return CardType.AnalogInput;
        }

        public int GetChannelLowPassFilterFactor (string channelName) {
            IndividualControl channel = GetChannelIndividualControl (channelName);
            return GetChannelLowPassFilterFactor (channel);
        }

        public int GetChannelLowPassFilterFactor (IndividualControl channel) {
            return GetChannelLowPassFilterFactor (channel.Group, channel.Individual);
        }

        public int GetChannelLowPassFilterFactor (string card, int channel) {
            CheckCardKey (card);
            var analogInputCard = cards[card] as AnalogInputCard;
            return analogInputCard.GetChannelLowPassFilterFactor (channel);
        }

        public int[] GetAllChannelLowPassFilterFactors (string card) {
            CheckCardKey (card);
            var analogInputCard = cards[card] as AnalogInputCard;
            return analogInputCard.GetAllChannelLowPassFilterFactors ();
        }

        public void SetChannelLowPassFilterFactor (string channelName, int lowPassFilterFactor) {
            IndividualControl channel = GetChannelIndividualControl (channelName);
            SetChannelLowPassFilterFactor (channel, lowPassFilterFactor);
        }

        public void SetChannelLowPassFilterFactor (IndividualControl channel, int lowPassFilterFactor) {
            SetChannelLowPassFilterFactor (channel.Group, channel.Individual, lowPassFilterFactor);
        }

        public void SetChannelLowPassFilterFactor (string card, int channel, int lowPassFilterFactor) {
            CheckCardKey (card);
            var analogInputCard = cards[card] as AnalogInputCard;
            analogInputCard.SetChannelLowPassFilterFactor (channel, lowPassFilterFactor);
        }
    }
}

