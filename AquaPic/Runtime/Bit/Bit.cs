using System;
using System.Collections.Generic;
using AquaPic.Utilites;

namespace AquaPic.Runtime
{
    public partial class Bit
    {
        private static Dictionary<string, BitState> states = new Dictionary<string, BitState> ();

        public static void Set (string name) {
            if (states.ContainsKey (name))
                states [name].state = MyState.Set;
            else
                states.Add (name, new BitState (MyState.Set));
        }

        public static void Reset (string name) {
            if (states.ContainsKey (name))
                states [name].state = MyState.Reset;
            else
                states.Add (name, new BitState (MyState.Reset));
        }

        public static bool Toggle (string name) {
            if (states.ContainsKey (name)) {
                if (states [name].state == MyState.Set)
                    states [name].state = MyState.Reset;
                else
                    states [name].state = MyState.Set;
            } else
                states.Add (name, new BitState (MyState.Set)); // technically we started with the state reset

            return states [name].state == MyState.Set;
        }

        public static bool Check (string name) {
            if (states.ContainsKey (name))
                return states [name].state == MyState.Set;
            else
                return false;
        }
    }
}

