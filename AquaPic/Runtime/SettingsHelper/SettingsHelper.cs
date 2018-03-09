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
using System.IO;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Runtime
{
    public class SettingsHelper
    {
        public static JObject OpenSettingsFile (string fileName) {
            if (!fileName.EndsWith (".json")) {
                fileName = string.Format ("{0}.json", fileName);
            }
            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, fileName);

            string json = File.ReadAllText (path);
            return (JObject)JToken.Parse (json);
        }

        public static void SaveSettingsFile (string fileName, JObject settings) {
            if (!fileName.EndsWith (".json")) {
                fileName = string.Format ("{0}.json", fileName);
            }
            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, fileName);

            File.WriteAllText (path, settings.ToString ());
        }

        public static int FindSettingsInArray (JArray settings, string entityName) {
            int arrayIndex = -1;
            for (int i = 0; i < settings.Count; ++i) {
                string n = (string)settings[i]["name"];
                if (entityName == n) {
                    arrayIndex = i;
                    break;
                }
            }
            return arrayIndex;
        }
    }
}
