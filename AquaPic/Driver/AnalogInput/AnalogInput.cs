using System;
using System.Collections.Generic;
using AquaPic.Globals;

namespace AquaPic.AnalogInputDriver
{
    public partial class AnalogInput
    {
        public static AnalogInput Main = new AnalogInput ();

        public List<analogInputCard> cards;

        public AnalogInput () {
            cards = new List<analogInputCard> ();
        }

        public int addCard (int address, string name) {
            int count = cards.Count;
            cards.Add (new analogInputCard ((byte)address, (byte)count));
            return count;
        }

        public void addChannel (int card, int ch, AnalogType type, string name) {
            cards [card].addChannel (ch, type, name);
        }

        public float getAnalog (analogInputCh ch) {
            return cards [ch.cardID].channels [ch.channelID].value;
        }
    }
}