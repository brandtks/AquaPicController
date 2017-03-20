#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using System.Collections.Generic; // for List
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Utilites;
using AquaPic.SerialBus;
using AquaPic.Runtime;
using AquaPic.Operands;

namespace AquaPic.Drivers
{
    public partial class Power
    {
        private static List<PowerStrip> pwrStrips = new List<PowerStrip> ();
        private static Dictionary<string, ModeChangedObj> modeChangedHandlers = new Dictionary<string, ModeChangedObj> ();
        private static Dictionary<string, StateChangedObj> stateChangedHandlers = new Dictionary<string, StateChangedObj> ();

        public static int powerStripCount {
            get {
                return pwrStrips.Count;
            }
        }

        public static void Init () {
            string path = Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "powerProperties.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    JArray ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                    foreach (var jt in ja) {
                        var jo = jt as JObject;

                        string name = (string)jo["name"];
                        int powerStripId = Power.GetPowerStripIndex ((string)jo["powerStrip"]);
                        int outletId = Convert.ToInt32 (jo["outlet"]);

                        MyState fallback = (MyState)Enum.Parse (typeof (MyState), (string)jo["fallback"]);

                        List<string> conditions = new List<string> ();
                        JArray cja = (JArray)jo["conditions"];
                        foreach (var cjt in cja) {
                            conditions.Add ((string)cjt);
                        }

                        var script = Script.CompileOutletConditionCheck (conditions.ToArray ());
                        if (script != null) {
                            var c = AddOutlet (powerStripId, outletId, name, fallback);
                            c.ConditionChecker = () => {
                                return script.OutletConditionCheck ();
                            };
                        } else {
                            Logger.AddInfo ("Error while adding outlet");
                        }
                    }
                }
            } else {
                Logger.Add ("Power settings file did not exist, created new power settings");
                var file = File.Create (path);
                file.Close ();

                var ja = new JArray ();
                File.WriteAllText (path, ja.ToString ());
            }

            TaskManager.AddCyclicInterrupt ("Power", 1000, Run);
        }

        protected static void Run () {
            foreach (var strip in pwrStrips) {
                strip.GetStatus ();

                int i = 0;
                foreach (var outlet in strip.outlets) { // could, probably should use a for loop but its just extra words
                    if (outlet.mode == Mode.Manual) {
                        if (outlet.manualState != outlet.currentState)
                            strip.SetOutletState ((byte)i, outlet.manualState);

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
            return AddOutlet (outlet.Group, outlet.Individual, name, fallback, "Power");
        }

        public static Coil AddOutlet (IndividualControl outlet, string name, MyState fallback, string owner) {
            return AddOutlet (outlet.Group, outlet.Individual, name, fallback, owner);
        }

        public static Coil AddOutlet (int powerID, int outletID, string name, MyState fallback) {
            return AddOutlet (powerID, outletID, name, fallback, "Power");
        }

        public static Coil AddOutlet (int powerID, int outletID, string name, MyState fallback, string owner) {
            if ((powerID < 0) && (powerID >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("powerId");

            if ((outletID < 0) || (outletID >= pwrStrips [powerID].outlets.Length))
                throw new ArgumentOutOfRangeException ("outletID");

            string s = string.Format ("{0}.p{1}", pwrStrips [powerID].name, outletID);
            if (pwrStrips [powerID].outlets [outletID].name != s)
                throw new Exception (string.Format ("Outlet already taken by {0}", pwrStrips [powerID].outlets [outletID].name));

            pwrStrips [powerID].outlets [outletID].name = name;
            pwrStrips [powerID].outlets [outletID].fallback = fallback;
            pwrStrips [powerID].outlets [outletID].mode = Mode.Auto;
            pwrStrips [powerID].outlets [outletID].owner = owner;
            pwrStrips [powerID].SetupOutlet (
                (byte)outletID,
                pwrStrips [powerID].outlets [outletID].fallback);

            return pwrStrips [powerID].outlets [outletID].OutletControl;
        }

        public static void RemoveOutlet (IndividualControl outlet) {
            RemoveOutlet (outlet.Group, outlet.Individual);
        }

        public static void RemoveOutlet (int powerID, int outletID) {
            if ((powerID < 0) && (powerID >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("powerId");

            if ((outletID < 0) || (outletID >= pwrStrips [powerID].outlets.Length))
                throw new ArgumentOutOfRangeException ("outletID");

            string s = string.Format ("{0}.p{1}", pwrStrips [powerID].name, outletID);
            pwrStrips [powerID].outlets [outletID].name = s;
            pwrStrips [powerID].outlets [outletID].fallback = MyState.Off;
            pwrStrips [powerID].outlets [outletID].mode = Mode.Manual;
            pwrStrips [powerID].outlets [outletID].owner = "Power";
            pwrStrips [powerID].outlets [outletID].manualState = MyState.Off;

            pwrStrips [powerID].outlets [outletID].OutletControl.ConditionChecker = () => {
                return false;
            };

            pwrStrips [powerID].SetupOutlet (
                (byte)outletID,
                pwrStrips [powerID].outlets [outletID].fallback);
            pwrStrips [powerID].SetOutletState ((byte)outletID, MyState.Off);
        }

        public static void SetOutletManualState (IndividualControl outlet, MyState state) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips[outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            pwrStrips [outlet.Group].outlets [outlet.Individual].manualState = state;
        }

        public static MyState GetOutletManualState (IndividualControl outlet) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips[outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            return pwrStrips [outlet.Group].outlets [outlet.Individual].manualState;
        }

        public static MyState GetOutletState (IndividualControl outlet) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips[outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            return pwrStrips [outlet.Group].outlets [outlet.Individual].currentState;
        }

        public static MyState[] GetAllStates (int powerID) {
            if ((powerID < 0) && (powerID >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("powerId");

            MyState[] states = new MyState[8];
            for (int i = 0; i < states.Length; ++i)
                states [i] = pwrStrips [powerID].outlets [i].currentState;
            return states;
        }

        public static void SetOutletMode (IndividualControl outlet, Mode mode) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips[outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            pwrStrips [outlet.Group].SetPlugMode ((byte)outlet.Individual, mode);
        }

        public static Mode GetOutletMode (IndividualControl outlet) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips[outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            return pwrStrips [outlet.Group].outlets [outlet.Individual].mode;
        }
        
        public static Mode[] GetAllModes (int powerID) {
            if ((powerID < 0) && (powerID >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("powerId");

            Mode[] modes = new Mode[8];
            for (int i = 0; i < modes.Length; ++i)
                modes [i] = pwrStrips [powerID].outlets [i].mode;
            return modes;
        }

        public static string[] GetAllOutletNames (int powerID) {
            if ((powerID < 0) && (powerID >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("powerId");

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
            if ((powerID < 0) && (powerID >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("powerId");

            return pwrStrips [powerID].name;
        }

        public static int GetPowerStripIndex (string name) {
            for (int i = 0; i < pwrStrips.Count; ++i) {
                if (string.Equals (pwrStrips [i].name, name, StringComparison.CurrentCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException (name + " does not exists");
        }

        public static string GetOutletName (IndividualControl outlet) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips[outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            return pwrStrips[outlet.Group].outlets[outlet.Individual].name;
        }

        public static bool OutletNameOk (string name) {
            try {
                GetOutletIndividualControl (name);
                return false;
            } catch {
                return true;
            }
        }

        public static void SetOutletName (IndividualControl outlet, string name) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips[outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            if (OutletNameOk (name))
                pwrStrips [outlet.Group].outlets [outlet.Individual].name = name;
            else
                throw new Exception (string.Format ("Outlet: {0} already exists", name));
        }

        public static MyState GetOutletFallback (IndividualControl outlet) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips[outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            return pwrStrips [outlet.Group].outlets [outlet.Individual].fallback;
        }

        public static void SetOutletFallback (IndividualControl outlet, MyState fallback) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips[outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            pwrStrips [outlet.Group].outlets [outlet.Individual].fallback = fallback;
            pwrStrips [outlet.Group].SetupOutlet (
                (byte)outlet.Individual,
                pwrStrips [outlet.Group].outlets [outlet.Individual].fallback);
        }


        public static void SetOutletConditionCheck (IndividualControl outlet, ConditionCheckHandler checker) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips[outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            pwrStrips[outlet.Group].outlets[outlet.Individual].OutletControl.ConditionChecker = checker;
        }

        public static void SetOutletConditionCheck (IndividualControl outlet, IOutletScript script) {
            SetOutletConditionCheck (outlet, script.OutletConditionCheck);
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

        public static string GetOutletOwner (IndividualControl outlet) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips [outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            return pwrStrips [outlet.Group].outlets [outlet.Individual].owner;
        }

        public static string[] GetAllOutletOwners (int powerId) {
            if ((powerId < 0) && (powerId >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("powerId");

            string[] owners = new string[pwrStrips [powerId].outlets.Length];
            for (int i = 0; i < owners.Length; ++i)
                owners [i] = pwrStrips [powerId].outlets [i].owner;

            return owners;
        }

        public static void AddHandlerOnModeChange (IndividualControl outlet, ModeChangedHandler handler) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips [outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            string name = GetOutletName (outlet);

            if (!modeChangedHandlers.ContainsKey (name))
                modeChangedHandlers.Add (name, new ModeChangedObj ());

            modeChangedHandlers [name].ModeChangedEvent += handler;
        }

        public static void RemoveHandlerOnModeChange (IndividualControl outlet, ModeChangedHandler handler) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips [outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            string name = GetOutletName (outlet);

            if (modeChangedHandlers.ContainsKey (name))
                modeChangedHandlers [name].ModeChangedEvent -= handler;
        }

        private static void OnModeChange (OutletData outlet, ModeChangeEventArgs args) {
            if (modeChangedHandlers.ContainsKey (outlet.name)) {
                modeChangedHandlers [outlet.name].CallEvent (outlet, args);
            }
        }

        public static void AddHandlerOnStateChange (IndividualControl outlet, StateChangeHandler handler) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips [outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            string name = GetOutletName (outlet);

            if (!stateChangedHandlers.ContainsKey (name))
                stateChangedHandlers.Add (name, new StateChangedObj ());

            stateChangedHandlers [name].StateChangedEvent += handler;
        }

        public static void RemoveHandlerOnStateChange (IndividualControl outlet, StateChangeHandler handler) {
            if ((outlet.Group < 0) && (outlet.Group >= pwrStrips.Count))
                throw new ArgumentOutOfRangeException ("outlet.Group");

            if ((outlet.Individual < 0) && (outlet.Individual >= pwrStrips [outlet.Group].outlets.Length))
                throw new ArgumentOutOfRangeException ("outlet.Individual");

            string name = GetOutletName (outlet);

            if (stateChangedHandlers.ContainsKey (name))
                stateChangedHandlers [name].StateChangedEvent -= handler;
        }

        private static void OnStateChange (OutletData outlet, StateChangeEventArgs args) {
            if (stateChangedHandlers.ContainsKey (outlet.name)) {
                stateChangedHandlers [outlet.name].CallEvent (outlet, args);
            }
        }

        public static bool AquaPicBusCommunicationOk (int powerId) {
            return pwrStrips [powerId].AquaPicBusCommunicationOk;
        }

        public static bool AquaPicBusCommunicationOk (IndividualControl ic) {
            return pwrStrips [ic.Group].AquaPicBusCommunicationOk;
        }

        public static bool AquaPicBusCommunicationOk (string name) {
            int powerId = GetPowerStripIndex (name);
            return pwrStrips [powerId].AquaPicBusCommunicationOk;
        }
    }
}

