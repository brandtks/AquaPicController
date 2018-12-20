#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2018 Goodtime Development

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
    public class EntitySettingAttribute : Attribute
    {
        public Type mutatorType;
        public string[] keys;
        public bool optional;

        public EntitySettingAttribute (Type mutatorType, string[] keys, bool optional) {
            if (!mutatorType.TypeIs (typeof (ISettingMutator<>))) {
                throw new ArgumentException ("The mutator type must derive ISettingMutator<T>", nameof (mutatorType));
            }
            this.mutatorType = mutatorType;
            this.keys = keys;
            this.optional = optional;
        }

        public EntitySettingAttribute (Type mutatorType, string key, bool optional)
            : this (mutatorType, new string[] { key }, optional) { }

        public EntitySettingAttribute (Type mutatorType, string[] keys)
            : this (mutatorType, keys, false) { }

        public EntitySettingAttribute (Type mutatorType, string key)
            : this (mutatorType, new string[] { key }, false) { }

        public EntitySettingAttribute (Type mutatorType)
            : this (mutatorType, new string[] { }, false) { }
    }
}
