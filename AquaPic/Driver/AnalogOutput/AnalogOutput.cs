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
                byte chId = 0;
                foreach (var channel in card.channels) {
                    if (channel.mode == Mode.Auto) {
                        channel.ValueControl.Execute ();
                        channel.value = channel.ValueControl.value.ToInt ();
                    } else
                        card.SetAnalogValue (chId, Convert.ToInt32 (channel.value));

                    ++chId;
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
            if ((cardID < 0) && (cardID >= cards.Count))
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

        public static int GetChannelIndex (int cardId, string name) {
            if ((cardId >= 0) && (cardId < cards.Count)) {
                for (int i = 0; i < cards [cardId].channels.Length; ++i) {
                    if (string.Equals (cards [cardId].channels [i].name, name, StringComparison.InvariantCultureIgnoreCase))
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
                    //types [i] = cards [cardId].channels [i].ValueControl.value;
                    types [i] = cards [cardId].channels [i].value;

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

        public static void SetValue (IndividualControl ic, float value) {
            if ((ic.Group >= 0) && (ic.Group < cards.Count)) {
                if ((ic.Individual >= 0) && (ic.Individual < cards [ic.Group].channels.Length)) {
                    if (cards [ic.Group].channels [ic.Individual].mode == Mode.Manual)
                        cards [ic.Group].channels [ic.Individual].value = value.ToInt ();
                    else
                        throw new Exception ("Can only modify analong output value with channel forced");
                }
            }
        }

        public static void SetMode (IndividualControl ic, Mode mode) {
            if ((ic.Group >= 0) && (ic.Group < cards.Count)) {
                if ((ic.Individual >= 0) && (ic.Individual < cards [ic.Group].channels.Length))
                    cards [ic.Group].channels [ic.Individual].mode = mode;
            }
        }

        public static Mode[] GetAllModes (int cardId) {
            if ((cardId >= 0) && (cardId < cards.Count)) {
                Mode[] modes = new Mode[cards [cardId].channels.Length];

                for (int i = 0; i < modes.Length; ++i)
                    modes [i] = cards [cardId].channels [i].mode;

                return modes;
            }

            return null;
        }

        public static Mode GetMode (IndividualControl ic) {
            if ((ic.Group >= 0) && (ic.Group < cards.Count)) {
                if ((ic.Individual >= 0) && (ic.Individual < cards [ic.Group].channels.Length))
                    return cards [ic.Group].channels [ic.Individual].mode;
            }

            return Mode.Manual;
        }
    }
}

