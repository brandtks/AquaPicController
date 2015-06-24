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
                throw new ArgumentOutOfRangeException ("Input ID out of range");

            string s = string.Format ("{0}.q{1}", cards [cardID].name, channelID);
            if (cards [cardID].channels [channelID].name != s)
                throw new Exception (string.Format ("Channel already taken by {0}", cards [cardID].channels [channelID].name));
            
            cards [cardID].AddChannel (channelID, type, name);
            return cards [cardID].channels [channelID].ValueControl;
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

        public static AnalogType[] GetAllAnalogTypes (int cardId) {
            if ((cardId >= 0) && (cardId < cards.Count)) {
                AnalogType[] types = new AnalogType[cards [cardId].channels.Length];

                for (int i = 0; i < types.Length; ++i)
                    types [i] = cards [cardId].channels [i].type;

                return types;
            }

            return null;
        }

        public static AnalogType GetAnalogType (IndividualControl ic) {
            if ((ic.Group >= 0) && (ic.Group < cards.Count)) {
                if ((ic.Individual >= 0) && (ic.Individual < cards [ic.Group].channels.Length))
                    return cards [ic.Group].channels [ic.Individual].type;
            }

            return AnalogType.None;
        }

        public static float[] GetAllValues (int cardId) {
            if ((cardId >= 0) && (cardId < cards.Count)) {
                float[] types = new float[cards [cardId].channels.Length];

                for (int i = 0; i < types.Length; ++i)
                    types [i] = cards [cardId].channels [i].ValueControl.value;

                return types;
            }

            return null;
        }

        public static float GetValue (IndividualControl ic) {
            if ((ic.Group >= 0) && (ic.Group < cards.Count)) {
                if ((ic.Individual >= 0) && (ic.Individual < cards [ic.Group].channels.Length))
                    return cards [ic.Group].channels [ic.Individual].ValueControl.value;
            }

            return 0.0f;
        }
    }
}

