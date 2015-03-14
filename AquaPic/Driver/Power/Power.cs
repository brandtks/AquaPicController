using System;
using System.Collections.Generic; // for List
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.SerialBus;
using AquaPic.CoilRuntime;

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
                foreach (var plug in pwrStrips [i].plugs) {
                    plug.plugControl.Execute ();
                }
            }
        }

        public static int AddPowerStrip (int address, string name, bool alarmOnLossOfPower) {
            int count = pwrStrips.Count;
            int pwrLossAlarmIdx = -1;

            if (alarmOnLossOfPower) {
                for (int i = 0; i < pwrStrips.Count; ++i) {
                    if (pwrStrips [i].powerLossAlarmIndex != -1)
                        pwrLossAlarmIdx = pwrStrips [i].powerLossAlarmIndex;
                }
            }

            pwrStrips.Add (new PowerStrip ((byte)address, (byte)count, name, alarmOnLossOfPower, pwrLossAlarmIdx));
            return count;
        }

        public static Coil AddPlug (IndividualControl plug, string name, MyState fallback) {

            pwrStrips [plug.Group].plugs [plug.Individual].name = name;
            pwrStrips [plug.Group].plugs [plug.Individual].fallback = fallback;
            pwrStrips [plug.Group].plugs [plug.Individual].mode = Mode.Auto;
            pwrStrips [plug.Group].SetupPlug (
                plug.Individual,
                pwrStrips [plug.Group].plugs [plug.Individual].fallback);

            return pwrStrips [plug.Group].plugs [plug.Individual].plugControl;
        }

        public static Coil AddPlug (int powerID, int plugID, string name, MyState fallback) {
            pwrStrips [powerID].plugs [plugID].name = name;
            pwrStrips [powerID].plugs [plugID].fallback = fallback;
            pwrStrips [powerID].plugs [plugID].mode = Mode.Auto;

            //pwrStrips [powerID].plugs [plugID].plugControl.ChangeName (name);
            return pwrStrips [powerID].plugs [plugID].plugControl;
        }

        public static void SetManualPlugState (IndividualControl plug, MyState state) {
//            if (pwrStrips [plug.Group].plugs [plug.Individual].mode == Mode.Manual) {
//                pwrStrips [plug.Group].SetPlugState (plug.Individual, state, true);
//            }
            pwrStrips [plug.Group].plugs [plug.Individual].manualState = state;
        }

        public static MyState GetManualPlugState (IndividualControl plug) {
            return pwrStrips [plug.Group].plugs [plug.Individual].manualState;
        }

        public static void AlarmShutdownPlug (IndividualControl plug) {
            pwrStrips [plug.Group].SetPlugState (plug.Individual, MyState.Off, true);
        }

        protected static void SetPlugState (IndividualControl plug, MyState state) {
            pwrStrips [plug.Group].SetPlugState (plug.Individual, state, false);
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
            return pwrStrips [plug.Group].plugs [plug.Individual].mode;
        }

        public static Mode[] GetAllModes (int powerID) {
            Mode[] modes = new Mode[8];
            for (int i = 0; i < modes.Length; ++i)
                modes [i] = pwrStrips [powerID].plugs [i].mode;
            return modes;
        }

        public static string[] GetAllPlugNames (int powerID) {
            string[] names = new string[8];
            for (int i = 0; i < names.Length; ++i)
                names [i] = pwrStrips [powerID].plugs [i].name;
            return names;
        }

        public static string[] GetAllPowerStripNames () {
            string[] names = new string[pwrStrips.Count];
            for (int i = 0; i < pwrStrips.Count; ++i) {
                names [i] = pwrStrips [i].name;
            }
            return names;
        }

        public static int GetPowerStripIndex (string name) {
            for (int i = 0; i < pwrStrips.Count; ++i) {
                if (string.Compare (pwrStrips [i].name, name, StringComparison.CurrentCultureIgnoreCase) == 0)
                    return i;
            }
            return -1;
        }

        public static string GetApbStatus (int powerID) {
            return Utils.GetDescription (pwrStrips [powerID].slave.status);
        }

        public static int GetApbResponseTime (int powerID) {
            return pwrStrips [powerID].slave.responeTime;
        }

        public static int GetApbAddress (int powerID) {
            return pwrStrips [powerID].slave.address;
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

