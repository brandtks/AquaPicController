using System;
using System.Collections.Generic;
using AquaPic.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
    public partial class AnalogOutput
    {
        private static List<AnalogOutputCard> cards = new List<AnalogOutputCard> ();

        static AnalogOutput () {
            TaskManager.AddCyclicInterrupt ("Analog Output", 1000, Run);
        }

        public static void Run () {
            foreach (var card in cards) {
                foreach (var channel in card.channels) {
                    channel.ValueControl.Execute ();
                }
            }
        }

        public static int AddCard (int address, string name) {
            int count = cards.Count;
            cards.Add (new AnalogOutputCard ((byte)address, (byte)count, name));
            return count;
        }

        public static Value AddChannel (IndividualControl channel, AnalogType type, string name) {
            return AddChannel (channel.Group, channel.Individual, type, name);
        }

        public static Value AddChannel (int cardID, int channelID, AnalogType type, string name) {
            if (cardID == -1)
                throw new Exception ("Card does not exist");

            if ((channelID < 0) || (channelID >= cards [cardID].channels.Length))
                throw new Exception ("Input ID out of range");

            string s = string.Format ("{0}.q{1}", cards [cardID].name, channelID);
            if (cards [cardID].channels [channelID].name != s)
                throw new Exception (string.Format ("Channel already taken by {0}", cards [cardID].channels [channelID].name));
            
            cards [cardID].AddChannel (channelID, type, name);
            return cards [cardID].channels [channelID].ValueControl;
        }

//        public static void SetAnalogValue (IndividualControl channel, int value) {
//            cards [channel.Group].SetAnalogValue (channel.Individual, value);
//        }

        public static int GetCardIndex (string name) {
            for (int i = 0; i < cards.Count; ++i) {
                if (cards [i].name == name)
                    return i;
            }
            return -1;
        }
    }
}

