#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2019 Goodtime Development

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
using AquaPic.Globals;

namespace AquaPic.Gadgets
{
    public class GenericGadget : GadgetPublisher, IDisposable
    {
        public string name { get; protected set; }
        public IndividualControl channel { get; protected set; }
        protected ValueType _value;
        public ValueType value {
            get {
                return GetValue ();
            }
            protected set {
                _value = value;
            }
        }

        public GenericGadget (GenericGadgetSettings settings) {
            name = settings.name;
            channel = settings.channel;
        }

        public virtual void Dispose () => throw new NotImplementedException ();

        public virtual GenericGadgetSettings OnUpdate (GenericGadgetSettings settings) {
            return settings;
        }

        public virtual GenericGadget Clone () {
            return (GenericGadget)MemberwiseClone ();
        }

        protected virtual ValueType GetValue () {
            return _value;
        }
    }
}
