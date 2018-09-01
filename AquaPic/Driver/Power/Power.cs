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
using System.Collections.Generic; // for List
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using AquaPic.Globals;
using AquaPic.Runtime;
using AquaPic.Operands;

namespace AquaPic.Drivers
{
    public partial class Power
    {
        static Dictionary<string, PowerStrip> powerStrips = new Dictionary<string, PowerStrip> ();
        static Dictionary<string, ModeChangedObj> modeChangedHandlers = new Dictionary<string, ModeChangedObj> ();
        static Dictionary<string, StateChangedObj> stateChangedHandlers = new Dictionary<string, StateChangedObj> ();

        public static int powerStripCount {
            get {
                return powerStrips.Count;
            }
        }

        public static string firstPowerStrip {
            get {
                if (powerStrips.Count > 0) {
                    var first = powerStrips.First ();
                    return first.Key;
                }

                return string.Empty;
            }
        }

        public static void Init () {
            string path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, "powerProperties.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    JArray ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                    foreach (var jt in ja) {
                        var jo = jt as JObject;

                        string name = (string)jo["name"];
                        string powerStripName = (string)jo["powerStrip"];
                        int outletId = Convert.ToInt32 (jo["outlet"]);

                        MyState fallback = (MyState)Enum.Parse (typeof (MyState), (string)jo["fallback"]);

                        List<string> conditions = new List<string> ();
                        JArray cja = (JArray)jo["conditions"];
                        foreach (var cjt in cja) {
                            conditions.Add ((string)cjt);
                        }

                        try {
                            var script = Script.CompileOutletCoilStateGetter (conditions.ToArray ());
                            var c = AddOutlet (powerStripName, outletId, name, fallback);
                            c.StateGetter = () => {
                                return script.OutletCoilStateGetter ();
                            };
                        } catch {
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
            foreach (var strip in powerStrips.Values) {
                strip.GetStatus ();

                int i = 0;
                foreach (var outlet in strip.outlets) { // could, probably should use a for loop but its just extra words
                    if (outlet.mode == Mode.Manual) {
                        if (outlet.manualState != outlet.currentState)
                            strip.SetOutletState ((byte)i, outlet.manualState);

                    } else {
                        outlet.OutletControl.Execute ();
                    }

                    ++i;
                }
            }
        }

        public static void AddPowerStrip (string name, int address, bool alarmOnLossOfPower) {
            int powerLossAlarmIndex = -1;

            if (alarmOnLossOfPower) {
                foreach (var strip in powerStrips.Values) {
                    if (strip.powerLossAlarmIndex != -1)
                        powerLossAlarmIndex = strip.powerLossAlarmIndex;
                }

                // No other power strips are subscride to a loss of power
                if (powerLossAlarmIndex == -1) {
                    powerLossAlarmIndex = Alarm.Subscribe ("Loss of AC Power");
                }
            }

            powerStrips.Add (name, new PowerStrip (name, (byte)address, powerLossAlarmIndex));
        }

        public static void RemovePowerStrip (string powerStripName) {
            CheckPowerStripKey (powerStripName);
            if (!CheckPowerStipEmpty (powerStripName)) {
                throw new Exception ("At least one outlet is occupied");
            }
            powerStrips[powerStripName].RemoveSlave ();
            powerStrips.Remove (powerStripName);
        }

        public static void CheckPowerStripKey (string powerStripName) {
            if (!powerStrips.ContainsKey (powerStripName))
                throw new ArgumentOutOfRangeException (nameof (powerStripName));
        }

        public static bool CheckPowerStripKeyNoThrow (string powerStripName) {
            try {
                CheckPowerStripKey (powerStripName);
                return true;
            } catch {
                return false;
            }
        }

        public static bool PowerStripNameOk (string powerStripName) {
            return !CheckPowerStripKeyNoThrow (powerStripName);
        }

        public static bool CheckPowerStipEmpty (string powerStripName) {
            CheckPowerStripKey (powerStripName);
            if (GetAllAvailableOutlets (powerStripName).Length == powerStrips[powerStripName].outlets.Length)
                return true;
            return false;
        }

        public static string[] GetAllPowerStripNames () {
            List<string> names = new List<string> ();
            foreach (var strip in powerStrips.Values) {
                names.Add (strip.name);
            }
            return names.ToArray ();
        }

        public static int GetLowestPowerStripNameIndex () {
            var nameIndexes = new List<int> ();
            var lowestNameIndex = 1;
            foreach (var strip in powerStrips.Values) {
                // All names start with PS, so everything after that is the name index
                nameIndexes.Add (Convert.ToInt32 (strip.name.Substring (2)));
            }

            bool lowestFound = false;
            while (!lowestFound) {
                if (nameIndexes.Contains (lowestNameIndex)) {
                    ++lowestNameIndex;
                } else {
                    lowestFound = true;
                }
            }

            return lowestNameIndex;
        }

        public static bool GetPowerStripAlarmOnPowerLoss (string powerStripName) {
            CheckPowerStripKey (powerStripName);
            return powerStrips[powerStripName].powerLossAlarmIndex != -1;
        }

        public static void SetPowerStripAlarmOnPowerLoss (string powerStripName, bool alarmOnLossOfPower) {
            CheckPowerStripKey (powerStripName);
            if (alarmOnLossOfPower) {
                foreach (var strip in powerStrips.Values) {
                    if (strip.powerLossAlarmIndex != -1)
                        powerStrips[powerStripName].powerLossAlarmIndex = strip.powerLossAlarmIndex;
                }

                if (powerStrips[powerStripName].powerLossAlarmIndex == -1) {
                    powerStrips[powerStripName].powerLossAlarmIndex = Alarm.Subscribe ("Loss of AC Power");
                }
            } else {
                powerStrips[powerStripName].powerLossAlarmIndex = -1;
            }
        }

        public static Coil AddOutlet (IndividualControl outlet, string name, MyState fallback, string owner = "Power") {
            return AddOutlet (outlet.Group, outlet.Individual, name, fallback, owner);
        }

        public static Coil AddOutlet (string powerStripName, int outletId, string name, MyState fallback, string owner = "Power") {
            CheckPowerStripKey (powerStripName);
            CheckOutletId (powerStripName, outletId);

            string s = string.Format ("{0}.p{1}", powerStripName, outletId);
            if (powerStrips[powerStripName].outlets[outletId].name != s)
                throw new Exception (string.Format ("Outlet already taken by {0}", powerStrips[powerStripName].outlets[outletId].name));

            powerStrips[powerStripName].outlets[outletId].name = name;
            powerStrips[powerStripName].outlets[outletId].fallback = fallback;
            powerStrips[powerStripName].outlets[outletId].mode = Mode.Auto;
            powerStrips[powerStripName].outlets[outletId].owner = owner;
            powerStrips[powerStripName].SetupOutlet (
                (byte)outletId,
                powerStrips[powerStripName].outlets[outletId].fallback);

            return powerStrips[powerStripName].outlets[outletId].OutletControl;
        }

        public static void RemoveOutlet (IndividualControl outlet) {
            RemoveOutlet (outlet.Group, outlet.Individual);
        }

        public static void RemoveOutlet (string powerStripName, int outletId) {
            CheckPowerStripKey (powerStripName);
            CheckOutletId (powerStripName, outletId);

            string s = string.Format ("{0}.p{1}", powerStripName, outletId);
            powerStrips[powerStripName].outlets[outletId].name = s;
            powerStrips[powerStripName].outlets[outletId].fallback = MyState.Off;
            powerStrips[powerStripName].outlets[outletId].mode = Mode.Manual;
            powerStrips[powerStripName].outlets[outletId].owner = "Power";
            powerStrips[powerStripName].outlets[outletId].manualState = MyState.Off;

            powerStrips[powerStripName].outlets[outletId].OutletControl.StateGetter = () => {
                return false;
            };

            powerStrips[powerStripName].SetupOutlet (
                (byte)outletId,
                powerStrips[powerStripName].outlets[outletId].fallback);
            powerStrips[powerStripName].SetOutletState ((byte)outletId, MyState.Off);
        }

        public static void CheckOutletId (string powerStripName, int outletId) {
            if ((outletId < 0) || (outletId >= powerStrips[powerStripName].outlets.Length))
                throw new ArgumentOutOfRangeException (nameof (outletId));
        }

        public static void SetOutletManualState (IndividualControl outlet, MyState state) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            powerStrips[outlet.Group].outlets[outlet.Individual].manualState = state;
        }

        public static MyState GetOutletManualState (IndividualControl outlet) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            return powerStrips[outlet.Group].outlets[outlet.Individual].manualState;
        }

        public static MyState GetOutletState (IndividualControl outlet) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            return powerStrips[outlet.Group].outlets[outlet.Individual].currentState;
        }

        public static MyState[] GetAllStates (string powerStripName) {
            CheckPowerStripKey (powerStripName);

            MyState[] states = new MyState[8];
            for (int i = 0; i < states.Length; ++i)
                states[i] = powerStrips[powerStripName].outlets[i].currentState;
            return states;
        }

        public static void SetOutletMode (IndividualControl outlet, Mode mode) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            powerStrips[outlet.Group].SetPlugMode ((byte)outlet.Individual, mode);
        }

        public static Mode GetOutletMode (IndividualControl outlet) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            return powerStrips[outlet.Group].outlets[outlet.Individual].mode;
        }

        public static Mode[] GetAllModes (string powerStripName) {
            CheckPowerStripKey (powerStripName);

            Mode[] modes = new Mode[8];
            for (int i = 0; i < modes.Length; ++i)
                modes[i] = powerStrips[powerStripName].outlets[i].mode;
            return modes;
        }

        public static string[] GetAllOutletNames (string powerStripName) {
            CheckPowerStripKey (powerStripName);

            string[] names = new string[8];
            for (int i = 0; i < names.Length; ++i)
                names[i] = powerStrips[powerStripName].outlets[i].name;
            return names;
        }

        public static string GetOutletName (IndividualControl outlet) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            return powerStrips[outlet.Group].outlets[outlet.Individual].name;
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
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            if (OutletNameOk (name))
                powerStrips[outlet.Group].outlets[outlet.Individual].name = name;
            else
                throw new Exception (string.Format ("Outlet: {0} already exists", name));
        }

        public static MyState GetOutletFallback (IndividualControl outlet) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            return powerStrips[outlet.Group].outlets[outlet.Individual].fallback;
        }

        public static void SetOutletFallback (IndividualControl outlet, MyState fallback) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            powerStrips[outlet.Group].outlets[outlet.Individual].fallback = fallback;
            powerStrips[outlet.Group].SetupOutlet (
                (byte)outlet.Individual,
                powerStrips[outlet.Group].outlets[outlet.Individual].fallback);
        }


        public static void SetOutletCoilStateGetter (IndividualControl outlet, StateGetterHandler setter) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            powerStrips[outlet.Group].outlets[outlet.Individual].OutletControl.StateGetter = setter;
        }

        public static void SetOutletCoilStateGetter (IndividualControl outlet, IOutletScript script) {
            SetOutletCoilStateGetter (outlet, script.OutletCoilStateGetter);
        }

        public static IndividualControl GetOutletIndividualControl (string name) {
            var outlet = IndividualControl.Empty;

            foreach (var strip in powerStrips.Values) {
                for (int j = 0; j < strip.outlets.Length; ++j) {
                    if (string.Equals (strip.outlets[j].name, name, StringComparison.InvariantCultureIgnoreCase)) {
                        outlet.Group = strip.name;
                        outlet.Individual = j;
                        return outlet;
                    }
                }
            }

            throw new ArgumentException (name + " does not exists");
        }

        public static string[] GetAllAvailableOutlets () {
            var available = new List<string> ();

            foreach (var strip in powerStrips.Values) {
                for (int i = 0; i < strip.outlets.Length; ++i) {
                    string s = string.Format ("{0}.p{1}", strip.name, i);
                    if (s == strip.outlets[i].name)
                        available.Add (s);
                }
            }

            return available.ToArray ();
        }

        public static string[] GetAllAvailableOutlets (string powerStripName) {
            CheckPowerStripKey (powerStripName);

            var available = new List<string> ();
            var ps = powerStrips[powerStripName];
            for (int i = 0; i < ps.outlets.Length; ++i) {
                string s = string.Format ("{0}.p{1}", ps.name, i);
                if (s == ps.outlets[i].name)
                    available.Add (s);
            }

            return available.ToArray ();
        }

        public static string GetOutletOwner (IndividualControl outlet) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            return powerStrips[outlet.Group].outlets[outlet.Individual].owner;
        }

        public static string[] GetAllOutletOwners (string powerStripName) {
            CheckPowerStripKey (powerStripName);

            string[] owners = new string[powerStrips[powerStripName].outlets.Length];
            for (int i = 0; i < owners.Length; ++i)
                owners[i] = powerStrips[powerStripName].outlets[i].owner;

            return owners;
        }

        public static void AddHandlerOnModeChange (IndividualControl outlet, ModeChangedHandler handler) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            string name = GetOutletName (outlet);

            if (!modeChangedHandlers.ContainsKey (name))
                modeChangedHandlers.Add (name, new ModeChangedObj ());

            modeChangedHandlers[name].ModeChangedEvent += handler;
        }

        public static void RemoveHandlerOnModeChange (IndividualControl outlet, ModeChangedHandler handler) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            string name = GetOutletName (outlet);

            if (modeChangedHandlers.ContainsKey (name))
                modeChangedHandlers[name].ModeChangedEvent -= handler;
        }

        private static void OnModeChange (OutletData outlet, ModeChangeEventArgs args) {
            if (modeChangedHandlers.ContainsKey (outlet.name)) {
                modeChangedHandlers[outlet.name].CallEvent (outlet, args);
            }
        }

        public static void AddHandlerOnStateChange (IndividualControl outlet, StateChangeHandler handler) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            string name = GetOutletName (outlet);

            if (!stateChangedHandlers.ContainsKey (name))
                stateChangedHandlers.Add (name, new StateChangedObj ());

            stateChangedHandlers[name].StateChangedEvent += handler;
        }

        public static void RemoveHandlerOnStateChange (IndividualControl outlet, StateChangeHandler handler) {
            CheckPowerStripKey (outlet.Group);
            CheckOutletId (outlet.Group, outlet.Individual);

            string name = GetOutletName (outlet);

            if (stateChangedHandlers.ContainsKey (name))
                stateChangedHandlers[name].StateChangedEvent -= handler;
        }

        private static void OnStateChange (OutletData outlet, StateChangeEventArgs args) {
            if (stateChangedHandlers.ContainsKey (outlet.name)) {
                stateChangedHandlers[outlet.name].CallEvent (outlet, args);
            }
        }

        public static bool AquaPicBusCommunicationOk (string powerStripName) {
            CheckPowerStripKey (powerStripName);
            return powerStrips[powerStripName].AquaPicBusCommunicationOk;
        }

        public static bool AquaPicBusCommunicationOk (IndividualControl outlet) {
            CheckPowerStripKey (outlet.Group);
            return powerStrips[outlet.Group].AquaPicBusCommunicationOk;
        }
    }
}

