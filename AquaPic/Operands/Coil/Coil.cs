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

namespace AquaPic.Operands
{
    public delegate bool ConditionCheckHandler ();
    public delegate void OutputHandler ();

    public class Coil
    {
        public ConditionCheckHandler ConditionChecker;
        public OutputHandler OutputTrue;
        public OutputHandler OutputFalse;
        public bool state;

        public Coil () {
            state = false;
        }

        public void Execute () {  
            state = (bool)ConditionChecker?.Invoke ();  // do we have a condition check method, if yes, lets run it to find out the new state

            if (state) {                        // if state is true
                OutputTrue?.Invoke ();          // do we have a method to run if the state is true, if yes, run it
            } else {
                OutputFalse?.Invoke ();         // do we have a method to run if the state is false, if yes, run it
            }
        }
    }
}

