using System;
using System.Collections.Generic; // for List

namespace AquaPic.Power
{
    public partial class Power
    {
        public static Power Main = new Power ();

        private List<PowerStrip> pwrStrips;

        public Power () {
            pwrStrips = new List<PowerStrip> ();
        }

        public int addPowerStrip (int address, string name) {
            int count = pwrStrips.Count;
            pwrStrips.Add (new PowerStrip ((byte)address, (byte)count, name));
            return count;
        }

        public void addPlug (int powerID, int plugID, string name, bool rtnToRequested = false) {
            pwrStrips [powerID].plugs [plugID].name = name;
            pwrStrips [powerID].plugs [plugID].rtnToRequested = rtnToRequested;
        }

        public void setPlug (pwrPlug plug, bool state, bool modeOverride = false) {
            pwrStrips [plug.powerID].setPlugState (plug.plugID, state, modeOverride);
        }

        public void addHandlerOnAuto (pwrPlug plug, modeChangedHandler handler) {
            pwrStrips [plug.powerID].plugs [plug.plugID].onAuto += handler;
        }

        public void addHandlerOnManual (pwrPlug plug, modeChangedHandler handler) {
            pwrStrips [plug.powerID].plugs [plug.plugID].onManual += handler;
        }

        public void addHandlerOnStateChange (pwrPlug plug, stateChangeHandler handler) {
            pwrStrips [plug.powerID].plugs [plug.plugID].onStateChange += handler;
        }
    }
}

