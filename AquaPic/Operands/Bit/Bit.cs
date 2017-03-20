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
using System.Collections.Generic;
using AquaPic.Utilites;

namespace AquaPic.Operands
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

