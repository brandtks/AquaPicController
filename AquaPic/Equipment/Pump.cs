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
using AquaPic.Drivers;
using AquaPic.Utilites;
using AquaPic.Operands;

namespace AquaPic.Equipment
{
    public class Pump : IEquipment<bool>
    {
        protected IndividualControl _outlet;
        public IndividualControl outlet {
            get {
                return _outlet;
            }
        }

        protected string _name;
        public string name {
            get {
                return _name;
            }
        }

        protected MyState _fallback;
        public MyState fallback {
            get {
                return _fallback;
            }
        }

        protected string _owner;
        public string owner {
            get {
                return _owner;
            }
        }

        protected bool _requestedState;
        public bool requestedState {
            get {
                return _requestedState;
            }
        }

        public Pump (IndividualControl outlet, string name, MyState fallback, string owner) {
            Add (outlet, name, fallback, owner);
        }

        public void Add (IndividualControl outlet, string name, MyState fallback, string owner) {
            _name = name;
            _fallback = fallback;
            _owner = owner;
            Add (outlet);
        }

        public void Add (IndividualControl outlet) {
            _outlet = outlet;
            var coil = Power.AddOutlet (_outlet, _name, _fallback, _owner);
            coil.ConditionChecker = PumpConditionChecker;
        }

        public void Remove () {
            Power.RemoveOutlet (_outlet);
        }

        public void Set (bool state) {
            _requestedState = state;
        }

        public void Set (MyState state) {
            _requestedState = state.ToBool ();
        }

        public void SetName (string name) {
            _name = name;
            Power.SetOutletName (_outlet, name);
        }

        protected virtual bool PumpConditionChecker () {
            return _requestedState;
        }
    }
}
