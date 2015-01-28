using System;
using System.Collections.Generic;

namespace AquaPic.Power
{
    public static class power
	{
        private static List<powerStrip> pwrs;

        static power () {
            pwrs = new List<powerStrip> ();
        }

        public static int addPowerStrip (int address, string name) {
            int count = pwrs.Count;
            pwrs.Add (new powerStrip ((byte)address, (byte)count, name));
            return count;
        }

        public static void addPlug (int powerID, int plugID, string name, bool rtnToRequested = false) {
            pwrs [powerID].plugs [plugID].name = name;
            pwrs [powerID].plugs [plugID].rtnToRequested = rtnToRequested;
        }

        public static bool setPlug (pwrPlug plug, bool state, bool modeOverride = false) {
            return pwrs [plug.powerID].setPlugState (plug.plugID, state, modeOverride);
		}

        public static void addHandlerOnAuto (pwrPlug plug, modeChangedHandler handler) {
            pwrs [plug.powerID].plugs [plug.plugID].onAuto += handler;
        }

        public static void addHandlerOnManual (pwrPlug plug, modeChangedHandler handler) {
            pwrs [plug.powerID].plugs [plug.plugID].onManual += handler;
        }

        public static void addHandlerOnStateChange (pwrPlug plug, stateChangeHandler handler) {
            pwrs [plug.powerID].plugs [plug.plugID].onStateChange += handler;
        }
	}
}

