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
using AquaPic.Globals;

namespace AquaPic.Runtime
{
    public class IndividualControlMutator : ISettingMutator<IndividualControl>
    {
        public IndividualControl Read (JObject jobj, string[] keys) {
            if (keys.Length < 2) {
                throw new ArgumentException ("keys must include at least two keys", nameof(keys));
            }

            var value = Default ();
            var text = (string)jobj[keys[0]];
            if (text.IsNotEmpty ()) {
                try {
                    value.Group = text;
                } catch {
                    //
                }
            }

            text = (string)jobj[keys[1]];
            if (text.IsNotEmpty ()) {
                try {
                    value.Individual = Convert.ToInt32 (text);
                } catch {
                    value = Default ();
                }
            }

            return value;
        }

        public void Write (IndividualControl value, JObject jobj, string[] keys) {
            if (keys.Length < 2) {
                throw new ArgumentException ("keys must include at least two keys", nameof (keys));
            }

            jobj[keys[0]] = value.Group;
            jobj[keys[1]] = value.Individual.ToString ();
        }

        public bool Valid (IndividualControl value) {
            return value.IsNotEmpty ();
        }

        public IndividualControl Default () {
            return IndividualControl.Empty;
        }
    }
}
