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

        public static void RemoveOutlet (IndividualControl outlet) {
            RemoveOutlet (outlet.Group, outlet.Individual);
        }

        public static void RemoveOutlet (int powerID, int outletID) {
            if (powerID == -1)
                throw new Exception ("Power strip ID does not exist");

            if ((outletID < 0) || (outletID >= pwrStrips [powerID].outlets.Length))
                throw new Exception ("Outlet ID out of range");

            string s = string.Format ("{0}.p{1}", pwrStrips [powerID].name, outletID);
            pwrStrips [powerID].outlets [outletID].name = s;
            pwrStrips [powerID].outlets [outletID].fallback = MyState.Off;
            pwrStrips [powerID].outlets [outletID].mode = Mode.Manual;

            pwrStrips [powerID].outlets [outletID].OutletControl.ConditionChecker = () => {
                return false;
            };

            pwrStrips [powerID].SetupOutlet (
                (byte)outletID,
                pwrStrips [powerID].outlets [outletID].fallback);
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

        public static string GetPowerStripName (int powerID) {
            if ((powerID >= 0) && (powerID < pwrStrips.Count))
                return pwrStrips [powerID].name;

            throw new ArgumentOutOfRangeException ("powerID is out of range");
        }

        public static int GetPowerStripIndex (string name) {
            for (int i = 0; i < pwrStrips.Count; ++i) {
                if (string.Equals (pwrStrips [i].name, name, StringComparison.CurrentCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException (name + " does not exists");
        }

        public static IndividualControl GetOutletIndividualControl (string name) {
            IndividualControl outlet;

            for (int i = 0; i < pwrStrips.Count; ++i) {
                for (int j = 0; j < pwrStrips [i].outlets.Length; ++j) {
                    if (string.Equals (pwrStrips [i].outlets [j].name, name, StringComparison.InvariantCultureIgnoreCase)) {
                        outlet.Group = (byte)i;
                        outlet.Individual = (byte)j;
                        return outlet;
                    }
                }
            }

            throw new ArgumentException (name + " does not exists");
        }

        public static string[] GetAllAvaiblableOutlets () {
            List<string> avail = new List<string> ();

            foreach (var ps in pwrStrips) {
                for (int i = 0; i < ps.outlets.Length; ++i) {
                    string s = string.Format ("{0}.p{1}", ps.name, i);
                    if (s == ps.outlets [i].name)
                        avail.Add (s);
                }
            }

            return avail.ToArray ();
        }

        public static void AddHandlerOnAuto (IndividualControl outlet, ModeChangedHandler handler) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].AutoEvent += handler;
        }

        public static void RemoveHandlerOnAuto (IndividualControl outlet, ModeChangedHandler handler) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].AutoEvent -= handler;
        }

        public static void AddHandlerOnManual (IndividualControl outlet, ModeChangedHandler handler) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].ManualEvent += handler;
        }

        public static void RemoveHandlerOnManual (IndividualControl outlet, ModeChangedHandler handler) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].ManualEvent -= handler;
        }

        public static void AddHandlerOnStateChange (IndividualControl outlet, StateChangeHandler handler) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].StateChangeEvent += handler;
        }

        public static void RemoveHandlerOnStateChange (IndividualControl outlet, StateChangeHandler handler) {
            pwrStrips [outlet.Group].outlets [outlet.Individual].StateChangeEvent -= handler;
        }
    }
}

