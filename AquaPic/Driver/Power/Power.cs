using System;
using System.Collections.Generic; // for List
using AquaPic.Utilites;
using AquaPic.SerialBus;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
    public partial class Power
    {
        private static List<PowerStrip> pwrStrips = new List<PowerStrip> ();

        static Power () {
            //<TEST> this doesn't need to be this fast for now
            //TaskManager.AddCyclicInterrupt ("Power", 250, Run);
            TaskManager.AddCyclicInterrupt ("Power", 1000, Run);
        }

        public static void Run () {
            foreach (var strip in pwrStrips) {
                #if !SIMULATION
                strip.GetStatus ();
                #endif

                int i = 0;
                foreach (var outlet in strip.outlets) { // could, probably should use a for loop but its just extra words
                    if (outlet.mode == Mode.Manual) {
                        if (outlet.manualState != outlet.currentState)
                            strip.SetOutletState ((byte)i, outlet.manualState, false);
                        
                    } else
                        outlet.OutletControl.Execute ();

                    ++i;
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

        public static Coil AddOutlet (IndividualControl outlet, string name, MyState fallback) {
            return AddOutlet (outlet.Group, outlet.Individual, name, fallback);
        }

        public static Coil AddOutlet (int powerID, int outletID, string name, MyState fallback) {
            if (powerID == -1)
                throw new Exception ("Power strip ID does not exist");

            if ((outletID < 0) || (outletID >= pwrStrips [powerID].outlets.Length))
                throw new Exception ("Outlet ID out of range");

            string s = string.Format ("{0}.p{1}", pwrStrips [powerID].name, outletID);
            if (pwrStrips [powerID].outlets [outletID].name != s)
                throw new Exception (string.Format ("Outlet already taken by {0}", pwrStrips [powerID].outlets [outletID].name));

            pwrStrips [powerID].outlets [outletID].name = name;
            pwrStrips [powerID].outlets [outletID].fallback = fallback;
            pwrStrips [powerID].outlets [outletID].mode = Mode.Auto;
            pwrStrips [powerID].SetupOutlet (
                (byte)outletID,
                pwrStrips [powerID].outlets [outletID].fallback);

            return pwrStrips [powerID].outlets [outletID].OutletControl;
        }

        public static void SetManualOutletState (IndividualControl outlet, MyState state) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].manualState = state;
        }

        public static void AlarmShutdownOutlet (IndividualControl outlet) {
            pwrStrips [outlet.Group].SetOutletState ((byte)outlet.Individual, MyState.Off, true);
        }

        public static void SetOutletMode (IndividualControl outlet, Mode mode) {
            pwrStrips [outlet.Group].SetPlugMode ((byte)outlet.Individual, mode);
        }

        public static MyState GetOutletState (IndividualControl outlet) {
            return pwrStrips [outlet.Group].outlets [outlet.Individual].currentState;
        }

        public static MyState[] GetAllStates (int powerID) {
            MyState[] states = new MyState[8];
            for (int i = 0; i < states.Length; ++i)
                states [i] = pwrStrips [powerID].outlets [i].currentState;
            return states;
        }

        public static Mode GetOutletMode (IndividualControl outlet) {
            return pwrStrips [outlet.Group].outlets [outlet.Individual].mode;
        }

        public static Mode[] GetAllModes (int powerID) {
            Mode[] modes = new Mode[8];
            for (int i = 0; i < modes.Length; ++i)
                modes [i] = pwrStrips [powerID].outlets [i].mode;
            return modes;
        }

        public static string[] GetAllOutletNames (int powerID) {
            string[] names = new string[8];
            for (int i = 0; i < names.Length; ++i)
                names [i] = pwrStrips [powerID].outlets [i].name;
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

        public static bool GetOutletIndividualControl (string name, ref IndividualControl outlet) {
            for (int i = 0; i < pwrStrips.Count; ++i) {
                for (int j = 0; j < pwrStrips [i].outlets.Length; ++j) {
                    if (string.Compare (pwrStrips [i].outlets [j].name, name, StringComparison.InvariantCultureIgnoreCase) == 0) {
                        outlet.Group = (byte)i;
                        outlet.Individual = (byte)j;
                        return true;
                    }
                }
            }

            return false;
        }

        public static void AddHandlerOnAuto (IndividualControl outlet, ModeChangedHandler handler) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].onAuto += handler;
        }

        public static void AddHandlerOnManual (IndividualControl outlet, ModeChangedHandler handler) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].onManual += handler;
        }

        public static void AddHandlerOnStateChange (IndividualControl outlet, StateChangeHandler handler) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].onStateChange += handler;
        }

        public static void RemoveHandlerOnStateChange (IndividualControl outlet, StateChangeHandler handler) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].onStateChange -= handler;
        }
    }
}

