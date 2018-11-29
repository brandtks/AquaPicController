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
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Runtime
{
    public class BoolMutator : ISettingMutator<bool>
    {
        public bool Read (JObject jobj, string[] keys) {
            if (keys.Length < 1) {
                throw new ArgumentException ("keys can not be empty", nameof(keys));
            }

            var value = Default ();
            var text = (string)jobj[keys[0]];
            if (text.IsNotEmpty ()) {
                try {
                    value = Convert.ToBoolean (text);
                } catch {
                    //
                }
            }
            return value;
        }

        public void Write (bool value, JObject jobj, string[] keys) {
            if (keys.Length < 1) {
                throw new ArgumentException ("keys can not be empty", nameof (keys));
            }
            jobj[keys[0]] = value.ToString ();
        }

        public bool Valid (bool value) {
            return true;
        }

        public virtual bool Default () {
            return false;
        }
    }

    public class BoolMutatorDefaultTrue : BoolMutator
    {
        public override bool Default () {
            return true;
        }
    }
}
