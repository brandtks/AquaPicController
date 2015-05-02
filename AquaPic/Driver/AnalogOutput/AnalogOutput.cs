using System;
using System.Collections.Generic;
using AquaPic.Globals;
using AquaPic.ValueRuntime;

namespace AquaPic.AnalogOutputDriver
{
    public partial class AnalogOutput
    {
        //public static AnalogOutput Main = new AnalogOutput ();

        private static List<AnalogOutputCard> cards = new List<AnalogOutputCard> ();

        //private AnalogOutput () {
            //cards = new List<AnalogOutputCard> ();
        //}

        public static void Run () {
            foreach (var card in cards) {
                foreach (var channel in card.channels) {
                    channel.ValueControl.Execute ();
                }
            }
        }

        public static int AddCard (int address, string name) {
            int count = cards.Count;
            cards.Add (new AnalogOutputCard ((byte)address, (byte)count));
            return count;
        }

        public static Value AddChannel (IndividualControl channel, AnalogType type, string name) {
            return AddChannel (channel.Group, channel.Individual, type, name);
        }

        public static Value AddChannel (int cardID, int channelID, AnalogType type, string name) {
            cards [cardID].AddChannel (channelID, type, name);
            return cards [cardID].channels [channelID].ValueControl;
        }

        public static void SetAnalogValue (IndividualControl channel, int value) {
            cards [channel.Group].SetAnalogValue (channel.Individual, value);
        }
    }
}

