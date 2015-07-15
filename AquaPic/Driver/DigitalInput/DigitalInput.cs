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
            TaskManager.AddCyclicInterrupt ("Digital Input", 1000, Run);
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
            if ((cardID < 0) || (cardID >= cards.Count))
                throw new ArgumentOutOfRangeException ("cardID");

            if ((inputID < 0) || (inputID >= cards [cardID].inputs.Length))
                throw new ArgumentOutOfRangeException ("inputId");

            string s = string.Format ("{0}.i{1}", cards [cardID].name, inputID);
            if (cards [cardID].inputs [inputID].name != s)
                throw new Exception (string.Format ("Input already taken by {0}", cards [cardID].inputs [inputID].name));
            
            cards [cardID].inputs [inputID].name = name;
        }

        public static void RemoveInput(IndividualControl ic) {
            RemoveInput (ic.Group, ic.Individual);
        }

        public static void RemoveInput(int cardId, int inputId) {
            if ((cardId < 0) || (cardId >= cards.Count))
                throw new ArgumentOutOfRangeException ("cardID");

            if ((inputId < 0) || (inputId >= cards [cardId].inputs.Length))
                throw new ArgumentOutOfRangeException ("inputId");

            string s = string.Format ("{0}.i{1}", cards [cardId].name, inputId);
            cards [cardId].inputs [inputId].name = s;
            cards [cardId].inputs [inputId].mode = Mode.Auto;
        }

        public static void Run () {
            foreach (var card in cards) {
                card.GetInputs ();
            }
        }

//        public static bool GetInputValue (IndividualControl input, bool realTimeUpdate = false) {
//            return GetInputValue (input.Group, input.Individual, realTimeUpdate);
//        }
//
//        public static bool GetInputValue (int card, int channel, bool realTimeUpdate = false) {
//            if (realTimeUpdate) {
//                cards [card].GetInput ((byte)channel);
//                while (cards [card].updating)
//                    continue;
//            }
//
//            return cards [card].inputs [channel].state;
//        }

        public static int GetCardIndex (string name) {
            for (int i = 0; i < cards.Count; ++i) {
                if (string.Equals (cards [i].name, name, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            return -1;
        }

        public static string GetCardName (int cardId) {
            if ((cardId < 0) && (cardId >= cards.Count))
                throw new ArgumentOutOfRangeException ("cardID");

            return cards [cardId].name;
        }

        public static int GetInputIndex (int cardId, string name) {
            if ((cardId >= 0) && (cardId < cards.Count)) {
                for (int i = 0; i < cards [cardId].inputs.Length; ++i) {
                    if (string.Equals (cards [cardId].inputs [i].name, name, StringComparison.InvariantCultureIgnoreCase))
                        return i;
                }
            }

            return -1;
        }

        public static string[] GetAllCardNames () {
            string[] names = new string[cards.Count];

            for (int i = 0; i < cards.Count; ++i)
                names [i] = cards [i].name;

            return names;
        }

        public static string[] GetAllInputNames (int cardId) {
            if ((cardId >= 0) && (cardId < cards.Count)) {
                string[] names = new string[cards [cardId].inputs.Length];

                for (int i = 0; i < names.Length; ++i)
                    names [i] = cards [cardId].inputs [i].name;

                return names;
            }

            return null;
        }

        public static bool[] GetAllStates (int cardId) {
            if ((cardId >= 0) && (cardId < cards.Count)) {
                bool[] types = new bool[cards [cardId].inputs.Length];

                for (int i = 0; i < types.Length; ++i)
                    types [i] = cards [cardId].inputs [i].state;

                return types;
            }

            return null;
        }

        public static bool GetState (IndividualControl ic, bool realTimeUpdate = false) {
            return GetState (ic.Group, ic.Individual, realTimeUpdate);
        }

        public static bool GetState (int cardId, int inputId, bool realTimeUpdate = false) {
            if ((cardId < 0) && (cardId >= cards.Count))
                throw new ArgumentOutOfRangeException ("cardID");

            if ((inputId < 0) || (inputId >= cards [cardId].inputs.Length))
                throw new ArgumentOutOfRangeException ("inputId");

            if (realTimeUpdate) {
                cards [cardId].GetInput ((byte)inputId);
                while (cards [cardId].updating)
                    continue;
            }

            return cards [cardId].inputs [inputId].state;
        }

        public static void SetState (IndividualControl ic, bool state) {
            if ((ic.Group < 0) && (ic.Group >= cards.Count))
                throw new ArgumentOutOfRangeException ("ic.Group");

            if ((ic.Individual < 0) || (ic.Individual >= cards [ic.Group].inputs.Length))
                throw new ArgumentOutOfRangeException ("ic.Individual");

            if (cards [ic.Group].inputs [ic.Individual].mode == Mode.Manual)
                cards [ic.Group].inputs [ic.Individual].state = state;
            else
                throw new Exception ("Can only modify state with input forced");
        }

        public static void SetMode (IndividualControl ic, Mode mode) {
            if ((ic.Group >= 0) && (ic.Group < cards.Count)) {
                if ((ic.Individual >= 0) && (ic.Individual < cards [ic.Group].inputs.Length))
                    cards [ic.Group].inputs [ic.Individual].mode = mode;
            }
        }

        public static Mode[] GetAllModes (int cardId) {
            if ((cardId >= 0) && (cardId < cards.Count)) {
                Mode[] modes = new Mode[cards [cardId].inputs.Length];

                for (int i = 0; i < modes.Length; ++i)
                    modes [i] = cards [cardId].inputs [i].mode;

                return modes;
            }

            return null;
        }

        public static Mode GetMode (IndividualControl ic) {
            if ((ic.Group >= 0) && (ic.Group < cards.Count)) {
                if ((ic.Individual >= 0) && (ic.Individual < cards [ic.Group].inputs.Length))
                    return cards [ic.Group].inputs [ic.Individual].mode;
            }

            return Mode.Manual;
        }

        public static string[] GetAllAvaiableInputs () {
            List<string> availInputs = new List<string> ();
            foreach (var card in cards) {
                for (int i = 0; i < card.inputs.Length; ++i) {
                    string s = string.Format ("{0}.i{1}", card.name, i);
                    if (s == card.inputs [i].name)
                        availInputs.Add (s);
                }
            }

            return availInputs.ToArray ();
        }
    }
}

