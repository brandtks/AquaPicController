using System;
using System.Collections.Generic;
using AquaPic.CoilRuntime;
using AquaPic.Globals;

namespace AquaPic.DigitalInputDriver
{
    public partial class DigitalInput
    {
        private static List<DigitalInputCard> cards = new List<DigitalInputCard> ();

        //public DigitalInput () { }

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

//        public static bool GetInputIndividualControl (string name, ref IndividualControl input) {
//
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

