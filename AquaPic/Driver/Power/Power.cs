using System;
using System.Collections.Generic; // for List
using AquaPic.Globals;

namespace AquaPic.PowerDriver
{
    public partial class Power
    {
        //public static Power Main = new Power ();

        private static List<PowerStrip> pwrStrips = new List<PowerStrip> ();
        public static float Voltage = 115;

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
            pwrStrips [powerID].plugs [plugID].returnToRequested = rtnToRequested;
        }

        public static void SetPlugState (IndividualControl plug, MyState state, bool modeOverride = false) {
            pwrStrips [plug.Group].SetPlugState (plug.Individual, state, modeOverride);
        }

        public static void SetPlugMode (IndividualControl plug, Mode mode) {
            pwrStrips [plug.Group].SetPlugMode (plug.Individual, mode);
        }

        public static MyState GetPlugState (IndividualControl plug) {
            return pwrStrips [plug.Group].plugs [plug.Individual].currentState;
        }

        public static MyState[] GetAllStates (int powerID) {
            MyState[] states = new MyState[8];
            for (int i = 0; i < states.Length; ++i)
                states [i] = pwrStrips [powerID].plugs [i].currentState;
            return states;
        }

        public static Mode GetPlugMode (IndividualControl plug) {
            return pwrStrips [plug.Group].plugs [plug.Individual].currentMode;
        }

        public static Mode[] GetAllModes (int powerID) {
            Mode[] modes = new Mode[8];
            for (int i = 0; i < modes.Length; ++i)
                modes [i] = pwrStrips [powerID].plugs [i].currentMode;
            return modes;
        }

        public static string[] GetAllNames (int powerID) {
            string[] names = new string[8];
            for (int i = 0; i < names.Length; ++i)
                names [i] = pwrStrips [powerID].plugs [i].name;
            return names;
        }

        public static void AddHandlerOnAuto (IndividualControl plug, ModeChangedHandler handler) {
            pwrStrips [plug.Group].plugs [plug.Individual].onAuto += handler;
        }

        public static void AddHandlerOnManual (IndividualControl plug, ModeChangedHandler handler) {
            pwrStrips [plug.Group].plugs [plug.Individual].onManual += handler;
        }

        public static void AddHandlerOnStateChange (IndividualControl plug, StateChangeHandler handler) {
            pwrStrips [plug.Group].plugs [plug.Individual].onStateChange += handler;
        }
    }
}

