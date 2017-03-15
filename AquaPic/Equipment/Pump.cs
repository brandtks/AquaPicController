using System;
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
            coil.ConditionChecker = Set;
        }

        public void Remove () {
            Power.RemoveOutlet (_outlet);
        }

        public virtual bool Set () {
            return false;
        }

        public void SetName (string name) {
            _name = name;
            Power.SetOutletName (_outlet, name);
        }

        public void SetConditionChecker (ConditionCheckHandler checker) {
            Power.SetOutletConditionCheck (outlet, checker);
        }
    }
}
