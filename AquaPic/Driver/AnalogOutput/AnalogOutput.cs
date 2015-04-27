using System;
using System.Collections.Generic;
using AquaPic.Globals;

namespace AquaPic.AnalogOutputDriver
{
    public partial class AnalogOutput
    {
        //public static AnalogOutput Main = new AnalogOutput ();

        private static List<AnalogOutputCard> cards = new List<AnalogOutputCard> ();

        //private AnalogOutput () {
            //cards = new List<AnalogOutputCard> ();
        //}

        public static int AddCard (int address, string name) {
            int count = cards.Count;
            cards.Add (new AnalogOutputCard ((byte)address, (byte)count));
            return count;
        }

        public static void AddChannel (IndividualControl channel, AnalogType type, string name) {
            cards [channel.Group].AddChannel (channel.Individual, type, name);
        }

        public static void AddChannel (int cardID, int channelID, AnalogType type, string name) {
            cards [cardID].AddChannel (channelID, type, name);
        }

        public static void SetAnalogValue (IndividualControl channel, int value) {
            cards [channel.Group].SetAnalogValue (channel.Individual, value);
        }
    }
}

