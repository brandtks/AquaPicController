using System;
using System.Collections.Generic;
using AquaPic.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
    public partial class AnalogInput
    {
        private static List<AnalogInputCard> cards = new List<AnalogInputCard> ();

        static AnalogInput () {
            TaskManager.AddCyclicInterrupt ("Analog Input", 1000, Run);
        }

        public static int AddCard (int address, string name) {
            int count = cards.Count;
            cards.Add (new AnalogInputCard ((byte)address, (byte)count, name));
            return count;
        }

        public static void AddChannel (IndividualControl channel, AnalogType type, string name) {
            AddChannel (channel.Group, channel.Individual, type, name);
        }

        public static void AddChannel (int cardID, int channelID, AnalogType type, string name) {
            if (cardID == -1)
                throw new Exception ("Card does not exist");

            if ((channelID < 0) || (channelID >= cards [cardID].channels.Length))
                throw new Exception ("Input ID out of range");

            string s = string.Format ("{0}.i{1}", cards [cardID].name, channelID);
            if (cards [cardID].channels [channelID].name != s)
                throw new Exception (string.Format ("Channel already taken by {0}", cards [cardID].channels [channelID].name));
            
            cards [cardID].AddChannel (channelID, type, name);
        }

        public static void Run () {
            for (int i = 0; i < cards.Count; ++i) {
                cards [i].GetValues ();
            }
        }

        public static float GetValue (IndividualControl channel, bool realTimeUpdate = false) {
            return GetValue (channel.Group, channel.Individual, realTimeUpdate);
        }

        public static float GetValue (int card, int channel, bool realTimeUpdate = false) {
            if ((card >= 0) && (card < cards.Count)) {
                if (realTimeUpdate) {
                    cards [card].GetValue ((byte)channel);
                    while (cards [card].updating)
                        continue;
                }

                if ((channel >= 0) && (channel < cards [card].channels.Length)) {
                    return cards [card].channels [channel].value;
                }

                return 0.0f;
            }

            return 0.0f;
        }

        public static float[] GetAllValues (int cardId) {
            if ((cardId >= 0) && (cardId < cards.Count)) {
                float[] types = new float[cards [cardId].channels.Length];

                for (int i = 0; i < types.Length; ++i)
                    types [i] = cards [cardId].channels [i].value;

                return types;
            }

            return null;
        }

        public static int GetCardIndex (string name) {
            for (int i = 0; i < cards.Count; ++i) {
                if (string.Equals (cards [i].name, name, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }
            return -1;
        }

        public static string[] GetAllCardNames () {
            string[] names = new string[cards.Count];

            for (int i = 0; i < cards.Count; ++i)
                names [i] = cards [i].name;

            return names;
        }

        public static string[] GetAllChannelNames (int cardId) {
            if ((cardId >= 0) && (cardId < cards.Count)) {
                string[] names = new string[cards [cardId].channels.Length];

                for (int i = 0; i < names.Length; ++i)
                    names [i] = cards [cardId].channels [i].name;

                return names;
            }

            return null;
        }
    }
}