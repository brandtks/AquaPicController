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
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Globals
{
    public class StringArrayMutator : ISettingMutator<IEnumerable<string>>
    {
        public IEnumerable<string> Read (JObject jobj, string[] keys) {
            if (keys.Length < 1) {
                throw new ArgumentException ("keys can not be empty", nameof (keys));
            }

            var values = new List<string> ();
            var ja = jobj[keys[0]] as JArray;
            foreach (var t in ja) {
                values.Add ((string)t);
            }

            return values;
        }

        public void Write (IEnumerable<string> values, JObject jobj, string[] keys) {
            if (keys.Length < 1) {
                throw new ArgumentException ("keys can not be empty", nameof (keys));
            }
            var ja = new JArray ();
            foreach (var value in values) {
                ja.Add (value);
            }
            jobj[keys[0]] = ja;
        }

        public bool Valid (IEnumerable<string> value) {
            return true;
        }

        public IEnumerable<string> Default () {
            return new string[0];
        }
    }
}
