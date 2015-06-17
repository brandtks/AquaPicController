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

        public static float GetAnalogValue (IndividualControl channel, bool realTimeUpdate = false) {
            if (realTimeUpdate) {
                cards [channel.Group].GetValue ((byte)channel.Individual);
                while (cards [channel.Group].updating)
                    continue;
            }
            return cards [channel.Group].channels [channel.Individual].value;
        }

        public static float GetInputValue (int card, int channel, bool realTimeUpdate = false) {
            if (realTimeUpdate) {
                cards [card].GetValue ((byte)channel);
                while (cards [card].updating)
                    continue;
            }

            return cards [card].channels [channel].value;
        }

        public static int GetCardIndex (string name) {
            for (int i = 0; i < cards.Count; ++i) {
                if (cards [i].name == name)
                    return i;
            }
            return -1;
        }
    }
}