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

namespace AquaPic.Operands
{
    public delegate bool StateGetterHandler ();
    public delegate void StateSetterHandler (bool condition);

    public class Coil : IOperand
    {
        public StateGetterHandler StateGetter;
        // This is protected to minimize the risk changing the setter after initialization
        protected StateSetterHandler StateSetter;
        public bool state;

        public Coil (StateSetterHandler ConditionSetter) {
            state = false;
            StateSetter = ConditionSetter;
        }

        public void Execute () {
            state = (bool)StateGetter?.Invoke ();  // do we have a condition check method, if yes, lets run it to find out the new state
            StateSetter?.Invoke (state);
        }
    }
}

