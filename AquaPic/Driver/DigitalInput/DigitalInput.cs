using System;
using System.Collections.Generic;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public partial class DigitalInputBase : GenericBase<bool>
    {
        public static DigitalInputBase SharedDigitalInputInstance = new DigitalInputBase ();

        protected DigitalInputBase () 
            : base ("Digital Input") { }

        protected override void Run () {
            foreach (var card in cards) {
                card.GetAllValuesCommunication ();
            }
        }

        protected override GenericCard<bool> CardCreater (string cardName, int cardId, int address) {
            return new DigitalInputCard<bool> (cardName, cardId, address);
        }
    }
}