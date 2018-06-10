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
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Globals
{
    // This is a struct to force passing by value instead of by reference
    public struct IndividualControl
    {
        public static IndividualControl Empty {
            get {
                var ic = new IndividualControl ();
                ic.Individual = -1;
                ic.Group = string.Empty;
                return ic;
            }
        }

        public string Group;
        public int Individual;

        public bool IsNotEmpty () {
            bool check = Group.IsNotEmpty ();
            check &= Individual != -1;
            return check;
        }

        public bool Equals (IndividualControl ic) {
            bool check = true;
            check &= (Individual == ic.Individual);
            check &= (Group == ic.Group);
            return check;
        }
    }
}

