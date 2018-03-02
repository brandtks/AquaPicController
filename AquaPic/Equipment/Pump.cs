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
using AquaPic.Drivers;
using AquaPic.Globals;
using AquaPic.Operands;

namespace AquaPic.Equipment
{
    public class Pump : IEquipment<ConditionGetterHandler>
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

        public Pump (IndividualControl outlet, string name, MyState fallback, string owner) {
            _name = name;
            _fallback = fallback;
            _owner = owner;
            _outlet = IndividualControl.Empty;
            Add (outlet);
        }

        public void Add (IndividualControl outlet) {
            if (!_outlet.Equals(outlet)) {
                Remove ();
            }

            _outlet = outlet;

            if (_outlet.IsNotEmpty ()) {
                var coil = Power.AddOutlet (_outlet, _name, _fallback, _owner);
                coil.ConditionGetter = OnConditionGetter;
            }
        }

        public void Remove () {
            if (_outlet.IsNotEmpty ()) {
                Power.RemoveOutlet (_outlet);
            }
        }

        public void SetName (string name) {
            _name = name;
            Power.SetOutletName (_outlet, name);
        }

        public void SetGetter (ConditionGetterHandler OnGetter) {
            if (_outlet.IsNotEmpty ()) {
                Power.SetOutletConditionCheck (_outlet, OnGetter);
            }
        }

        protected virtual bool OnConditionGetter () {
            return false;
        }
    }
}

