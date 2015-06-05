using System;
using System.Collections.Generic;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public partial class DigitalInput
    {
        private static List<DigitalInputCard> cards = new List<DigitalInputCard> ();

        static DigitalInput () {
            TaskManager.AddTask ("Digital Input", 1000, Run);
        }

        public static int AddCard (int address, string name) {
            int count = cards.Count;
            cards.Add (new DigitalInputCard ((byte)address, (byte)count, name));
            return count;
        }

        public static void AddInput (IndividualControl input, string name) {
            AddInput (input.Group, input.Individual, name);
        }

        public static void AddInput (int cardID, int inputID, string name) {
            if (cardID == -1)
                throw new Exception ("Card does not exist");

            if ((inputID < 0) || (cardID >= cards [cardID].inputs.Length))
                throw new Exception ("Input ID out of range");

            string s = string.Format ("{0}.i{1}", cards [cardID].name, inputID);
            if (cards [cardID].inputs [inputID].name != s)
                throw new Exception (string.Format ("Input already taken by {0}", cards [cardID].inputs [inputID].name));
            
            cards [cardID].inputs [inputID].name = name;
        }

        public static void Run () {
            foreach (var card in cards) {
                card.GetInputs ();
            }
        }

        public static bool GetInputValue (IndividualControl input, bool realTimeUpdate = false) {
            return GetInputValue (input.Group, input.Individual, realTimeUpdate);
        }

        public static bool GetInputValue (int card, int channel, bool realTimeUpdate = false) {
            if (realTimeUpdate) {
                cards [card].GetInput ((byte)channel);
                while (cards [card].updating)
                    continue;
            }

            return cards [card].inputs [channel].state;
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

