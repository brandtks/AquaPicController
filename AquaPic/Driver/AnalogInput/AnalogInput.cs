using System;
using System.Collections.Generic;
using AquaPic.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
    public partial class AnalogInputBase : GenericBase<float>
    {
        public static AnalogInputBase SharedAnalogInputInstance = new AnalogInputBase ();

        protected AnalogInputBase () 
            : base ("Analog Input") { }
        
        protected override void Run () {
            foreach (var card in cards) {
                card.GetAllValuesCommunication ();
            }
        }

        protected override GenericCard<float> CardCreater (string cardName, int cardId, int address) {
            return new AnalogInputCard<float> (cardName, cardId, address);
        }
    }
}