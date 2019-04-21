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
using System.Collections.Generic;

namespace AquaPic.Service
{
    public class Bit
    {
        public static Bit Instance { get; } = new Bit ();

        private Dictionary<string, bool> states;

        protected Bit () {
            states = new Dictionary<string, bool> ();
        }

        public void Set (string name, bool value) {
            if (states.ContainsKey (name)) {
                states[name] = value;
            } else {
                states.Add (name, value);
            }
        }

        public void Set (string name) {
            if (states.ContainsKey (name)) {
                states[name] = true;
            } else {
                states.Add (name, true);
            }
        }

        public void Reset (string name) {
            if (states.ContainsKey (name)) {
                states[name] = false;
            } else {
                states.Add (name, false);
            }
        }

        public void Toggle (string name) {
            if (states.ContainsKey (name)) {
                states[name] = !states[name];
            } else {
                states.Add (name, true); // technically we started with the state low
            }
        }

        public bool Check (string name) {
            if (states.ContainsKey (name)) {
                return states[name];
            }

            return false;
        }

        public void Remove (string name) {
            if (states.ContainsKey (name)) {
                states.Remove (name);
            }
        }
    }
}

