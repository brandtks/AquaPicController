using System;
using System.Collections.Generic;
using AquaPic.Globals;

namespace AquaPic.AnalogInput
{
    public static class analogInput
    {
        public static List<analogInputCard> cards;

        static analogInput () {
            cards = new List<analogInputCard> ();
        }

        public static int addCard (int address) {
            int count = cards.Count;
            cards.Add (new analogInputCard ((byte)address, (byte)count));
            return count;
        }

        public static void addChannel (int card, int ch, AnalogType type, string name) {
            cards [card].addChannel (ch, type, name);
        }

        public static float getAnalog (analogInputCh ch) {
            //cards [ch.cardID].getValue (ch.channelID);
            return cards [ch.cardID].channels [ch.channelID].value;
        }
    }
}