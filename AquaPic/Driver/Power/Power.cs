using System;
using System.Collections.Generic; // for List
using AquaPic.Globals;

namespace AquaPic.PowerDriver
{
    public partial class Power
    {
        //public static Power Main = new Power ();

        private static List<PowerStrip> pwrStrips = new List<PowerStrip> ();

        //public Power () {
            //pwrStrips = new List<PowerStrip> ();
        //}

        public static void Run () {
            for (int i = 0; i < pwrStrips.Count; ++i) {
                pwrStrips [i].GetStatus ();
            }
        }

        public static int AddPowerStrip (int address, string name) {
            int count = pwrStrips.Count;
            pwrStrips.Add (new PowerStrip ((byte)address, (byte)count, name));
            return count;
        }

        public static void AddPlug (int powerID, int plugID, string name, bool rtnToRequested = false) {
            pwrStrips [powerID].plugs [plugID].name = name;
            pwrStrips [powerID].plugs [plugID].rtnToRequested = rtnToRequested;
        }

        public static void SetPlug (IndividualControl plug, bool state, bool modeOverride = false) {
            pwrStrips [plug.Group].SetPlugState (plug.Individual, state, modeOverride);
        }

        public static void AddHandlerOnAuto (IndividualControl plug, modeChangedHandler handler) {
            pwrStrips [plug.Group].plugs [plug.Individual].onAuto += handler;
        }

        public static void AddHandlerOnManual (IndividualControl plug, modeChangedHandler handler) {
            pwrStrips [plug.Group].plugs [plug.Individual].onManual += handler;
        }

        public static void AddHandlerOnStateChange (IndividualControl plug, stateChangeHandler handler) {
            pwrStrips [plug.Group].plugs [plug.Individual].onStateChange += handler;
        }
    }
}

