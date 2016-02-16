using System;
using System.Collections.Generic;
using AquaPic.Utilites;
using AquaPic.Runtime;

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

                    if (channel.mode == Mode.Auto)
                        channel.ValueControl.Execute ();

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