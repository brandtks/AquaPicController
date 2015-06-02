using System;
using System.Collections.Generic;
using AquaPic.Utilites;

namespace AquaPic.StateRuntime
{
    //<TODO> find a new name, I don't like ControllerState
    public partial class ControllerState
    {
        private static Dictionary<string, IState> states = new Dictionary<string, IState> ();

        //public ControllerState () { }

        public static void Set (string name) {
            if (states.ContainsKey (name))
                states [name].state = MyState.Set;
            else
                states.Add (name, new IState (MyState.Set));
        }

        public static void Reset (string name) {
            if (states.ContainsKey (name))
                states [name].state = MyState.Reset;
            else
                states.Add (name, new IState (MyState.Reset));
        }

        public static void Toggle (string name) {
            if (states.ContainsKey (name)) {
                if (states [name].state == MyState.Set)
                    states [name].state = MyState.Reset;
                else
                    states [name].state = MyState.Set;
            } else
                states.Add (name, new IState (MyState.Set)); // technically we started with the state reset
        }

        public static MyState Check (string name) {
            if (states.ContainsKey (name))
                return states [name].state;
            else
                return MyState.Invalid;
        }
    }
}

