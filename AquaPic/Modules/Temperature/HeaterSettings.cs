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
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using AquaPic.Globals;
using AquaPic.Gadgets;

namespace AquaPic.Modules.Temperature
{
    public class HeaterSettings : GenericEquipmentSettings
    {

    }

    public class HeaterMutator : ISettingMutator<IEnumerable<HeaterSettings>>
    {
        public IEnumerable<HeaterSettings> Read (JObject jobj, string[] keys) {
            var heaters = new List<HeaterSettings> ();
            var ja = jobj["heaters"] as JArray;
            foreach (var jt in ja) {
                var jo = jt as JObject;

                var settings = new HeaterSettings ();
                settings.name = (string)jo["name"];
                var plug = IndividualControl.Empty;
                plug.Group = (string)jo["card"];
                var text = (string)jo["channel"];
                try {
                    plug.Individual = Convert.ToInt32 (text);
                } catch {
                    plug = IndividualControl.Empty;
                }
                settings.channel = plug;
                heaters.Add (settings);
            }

            return heaters;
        }

        public void Write (IEnumerable<HeaterSettings> value, JObject jobj, string[] keys) {
            var ja = new JArray ();
            foreach (var heater in value) {
                JObject jo = new JObject ();
                jo.Add (new JProperty ("name", heater.name));
                jo.Add (new JProperty ("card", heater.channel.Group));
                jo.Add (new JProperty ("channel", heater.channel.Individual.ToString ()));
                ja.Add (jo);
            }
            jobj["heaters"] = ja;
        }

        public bool Valid (IEnumerable<HeaterSettings> heaters) {
            return true;
        }

        public IEnumerable<HeaterSettings> Default () {
            return new HeaterSettings[0];
        }
    }
}
