#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2019 Goodtime Development

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

namespace AquaPic.Drivers
{
    public class GenericOutputBase : GenericBase
    {
        public GenericOutputBase (string name, uint runtime = 1000) 
            : base (name, runtime) { }

        public sealed override void AddChannel (string card, int channel, string channelName) => throw new NotSupportedException ();
        public sealed override void AddChannel (IndividualControl channel, string channelName) => throw new NotSupportedException ();

        public virtual void AddOutputChannel (IndividualControl channel, string channelName, Guid subscriptionKey) {
            AddOutputChannel (channel.Group, channel.Individual, channelName, subscriptionKey);
        }

        public virtual void AddOutputChannel (string card, int channel, string channelName, Guid subscriptionKey) {
            base.AddChannel (card, channel, channelName);
            SubscribeChannel (card, channel, subscriptionKey);
        }

        public void SubscribeChannel (string channelName, Guid key) {
            var channel = GetChannelIndividualControl (channelName);
            SubscribeChannel (channel.Group, channel.Individual, key);
        }

        public void SubscribeChannel (IndividualControl channel, Guid key) {
            SubscribeChannel (channel.Group, channel.Individual, key);
        }

        public void SubscribeChannel (string card, int channel, Guid key) {
            CheckCardKey (card);
            var outputCard = cards[card] as GenericOutputCard;
            outputCard.SubscribeChannel (channel, key);
        }

        public void UnsubscribeChannel (string channelName) {
            var channel = GetChannelIndividualControl (channelName);
            UnsubscribeChannel (channel.Group, channel.Individual);
        }

        public void UnsubscribeChannel (IndividualControl channel) {
            UnsubscribeChannel (channel.Group, channel.Individual);
        }

        public void UnsubscribeChannel (string card, int channel) {
            CheckCardKey (card);
            var outputCard = cards[card] as GenericOutputCard;
            outputCard.UnsubscribeChannel (channel);
        }

        public Guid GetSubscriptionKey (string channelName) {
            var channel = GetChannelIndividualControl (channelName);
            return GetSubscriptionKey (channel.Group, channel.Individual);
        }

        public Guid GetSubscriptionKey (IndividualControl channel) {
            return GetSubscriptionKey (channel.Group, channel.Individual);
        }

        public Guid GetSubscriptionKey (string card, int channel) {
            CheckCardKey (card);
            var outputCard = cards[card] as GenericOutputCard;
            return outputCard.GetSubscriptionKey (channel);
        }

        public Guid[] GetAllSubscriptionKeys (string card) {
            CheckCardKey (card);
            var outputCard = cards[card] as GenericOutputCard;
            return outputCard.GetAllSubscriptionKeys ();
        }
    }
}
