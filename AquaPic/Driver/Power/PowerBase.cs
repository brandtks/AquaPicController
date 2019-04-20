#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using AquaPic.Globals;

namespace AquaPic.Drivers
{
    public partial class PowerBase : GenericOutputBase
    {
        public static PowerBase SharedPowerInstance = new PowerBase ();

        protected PowerBase ()
            : base ("Power") { }

        protected override GenericCard CardCreater (string cardName, int address) {
            return new PowerStrip (cardName, address);
        }

        public override string GetCardAcyronym () {
            return "PS";
        }

        public override CardType GetCardType () {
            return CardType.Power;
        }

        protected override void Run () {
            foreach (var card in cards.Values) {
                var powerStrip = card as PowerStrip;
                powerStrip.GetStatusCommunication ();
            }
        }

        [Obsolete ("Use AddOutlet instead")]
        public sealed override void AddOutputChannel (string card, int channel, string channelName, Guid subscriptionKey) => throw new NotSupportedException ();
        [Obsolete ("Use AddOutlet instead")]
        public sealed override void AddOutputChannel (IndividualControl channel, string channelName, Guid subscriptionKey) => throw new NotSupportedException ();

        public void AddOutlet (IndividualControl outlet, string name, MyState fallback, Guid subscriptionKey) {
            AddOutlet (outlet.Group, outlet.Individual, name, fallback, subscriptionKey);
        }

        public void AddOutlet (string powerStrip, int outlet, string name, MyState fallback, Guid subscriptionKey) {
            base.AddOutputChannel (powerStrip, outlet, name, subscriptionKey);
            SetOutletFallback (powerStrip, outlet, fallback);
        }

        public MyState GetOutletFallback (string outletName) {
            var outlet = GetChannelIndividualControl (outletName);
            return GetOutletFallback (outlet.Group, outlet.Individual);
        }

        public MyState GetOutletFallback (IndividualControl outlet) {
            return GetOutletFallback (outlet.Group, outlet.Individual);
        }

        public MyState GetOutletFallback (string powerStrip, int outlet) {
            CheckCardKey (powerStrip);
            var strip = cards[powerStrip] as PowerStrip;
            return strip.GetOutletFallback (outlet);
        }

        public void SetOutletFallback (string outletName, MyState fallback) {
            var outlet = GetChannelIndividualControl (outletName);
            SetOutletFallback (outlet.Group, outlet.Individual, fallback);
        }

        public void SetOutletFallback (IndividualControl outlet, MyState fallback) {
            SetOutletFallback (outlet.Group, outlet.Individual, fallback);
        }

        public void SetOutletFallback (string powerStrip, int outlet, MyState fallback) {
            CheckCardKey (powerStrip);
            var strip = cards[powerStrip] as PowerStrip;
            strip.SetOutletFallback (outlet, fallback);
        }

        public int GetPowerLossAlarmIndex (string powerStrip) {
            CheckCardKey (powerStrip);
            var strip = cards[powerStrip] as PowerStrip;
            return strip.powerLossAlarmIndex;
        }
    }
}

