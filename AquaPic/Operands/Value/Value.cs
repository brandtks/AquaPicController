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
    public delegate float ValueGetterHandler ();
    public delegate void ValueSetterHandler (float value);

    public class Value
    {
        public ValueGetterHandler ValueGetter;
        public ValueSetterHandler ValueSetter;
        public float value;

        public Value () {
            value = 0.0f;
        }

        public void Execute () {
            value = (float)ValueGetter?.Invoke();
            ValueSetter?.Invoke (value);
        }
    }
}

