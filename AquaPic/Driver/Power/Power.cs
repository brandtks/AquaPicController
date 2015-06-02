using System;
using System.Collections.Generic; // for List
using AquaPic.Utilites;
using AquaPic.SerialBus;
using AquaPic.CoilRuntime;
using AquaPic.PluginRuntime;

namespace AquaPic.PowerDriver
{
    public partial class Power
    {
        private static List<PowerStrip> pwrStrips = new List<PowerStrip> ();

        static Power () {
            TaskManagerRuntime.TaskManager.AddTask ("Power", 250, Run);
        }

        public static void Run () {
            foreach (var strip in pwrStrips) {
                #if !SIMULATION
                strip.GetStatus ();
                #endif

                int i = 0;
                foreach (var outlet in strip.Outlets) { // could, probably should use a for loop but its just extra words
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
                throw new Exception ("Card does not exist");
            pwrStrips [powerID].Outlets [outletID].name = name;
            pwrStrips [powerID].Outlets [outletID].fallback = fallback;
            pwrStrips [powerID].Outlets [outletID].mode = Mode.Auto;
            pwrStrips [powerID].SetupOutlet (
                (byte)outletID,
                pwrStrips [powerID].Outlets [outletID].fallback);

            return pwrStrips [powerID].Outlets [outletID].OutletControl;
        }

//        public static void AddPluginOutlet (
//            string pluginName, 
//            int powerID, 
//            int outletID, 
//            string name, 
//            MyState fallback
//        ) {
//            Coil c = AddOutlet (powerID, outletID, name, fallback);
//            c.ConditionChecker = () => {
//                OutletScript p = Plugin.AllPlugins [pluginName] as OutletScript;
//                return p.RunOutletCondition ();
//            };
//        }

        public static void SetManualOutletState (IndividualControl outlet, MyState state) {
            pwrStrips [outlet.Group].Outlets [outlet.Individual].manualState = state;
        }

        public static void AlarmShutdownOutlet (IndividualControl outlet) {
            pwrStrips [outlet.Group].SetOutletState (outlet.Individual, MyState.Off, true);
        }

        public static void SetOutletMode (IndividualControl outlet, Mode mode) {
            pwrStrips [outlet.Group].SetPlugMode (outlet.Individual, mode);
        }

        public static MyState GetOutletState (IndividualControl outlet) {
            return pwrStrips [outlet.Group].Outlets [outlet.Individual].currentState;
        }

        public static MyState[] GetAllStates (int powerID) {
            MyState[] states = new MyState[8];
            for (int i = 0; i < states.Length; ++i)
                states [i] = pwrStrips [powerID].Outlets [i].currentState;
            return states;
        }

        public static Mode GetOutletMode (IndividualControl outlet) {
            return pwrStrips [outlet.Group].Outlets [outlet.Individual].mode;
        }

        public static Mode[] GetAllModes (int powerID) {
            Mode[] modes = new Mode[8];
            for (int i = 0; i < modes.Length; ++i)
                modes [i] = pwrStrips [powerID].Outlets [i].mode;
            return modes;
        }

        public static string[] GetAllOutletNames (int powerID) {
            string[] names = new string[8];
            for (int i = 0; i < names.Length; ++i)
                names [i] = pwrStrips [powerID].Outlets [i].name;
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
                for (int j = 0; j < pwrStrips [i].Outlets.Length; ++j) {
                    if (string.Compare (pwrStrips [i].Outlets [j].name, name, StringComparison.InvariantCultureIgnoreCase) == 0) {
                        outlet.Group = (byte)i;
                        outlet.Individual = (byte)j;
                        return true;
                    }
                }
            }

            return false;
        }

        public static void AddHandlerOnAuto (IndividualControl outlet, ModeChangedHandler handler) {
            pwrStrips [outlet.Group].Outlets [outlet.Individual].onAuto += handler;
        }

        public static void AddHandlerOnManual (IndividualControl outlet, ModeChangedHandler handler) {
            pwrStrips [outlet.Group].Outlets [outlet.Individual].onManual += handler;
        }

        public static void AddHandlerOnStateChange (IndividualControl outlet, StateChangeHandler handler) {
            pwrStrips [outlet.Group].Outlets [outlet.Individual].onStateChange += handler;
        }

        public static void RemoveHandlerOnStateChange (IndividualControl outlet, StateChangeHandler handler) {
            pwrStrips [outlet.Group].Outlets [outlet.Individual].onStateChange -= handler;
        }
    }
}

