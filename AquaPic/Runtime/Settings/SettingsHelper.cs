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
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Runtime
{
    public class SettingsHelper
    {
        public static JToken OpenSettingsFile (string fileName) {
            if (!fileName.EndsWith (".json")) {
                fileName = string.Format ("{0}.json", fileName);
            }
            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, fileName);

            string json = File.ReadAllText (path);
            return JToken.Parse (json);
        }

        public static void SaveSettingsFile (string fileName, JToken settings) {
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

        public static bool AddEntityToArray (string fileName, string arrayName, IGroupSettings settings) {
            var successful = false;
            var jo = OpenSettingsFile (fileName) as JObject;
            if (jo != null) {
                var ja = jo[arrayName] as JArray;
                if (ja != null) {
                    var jobj = new JObject ();
                    var reflectedSettings = GetReflectedSettings (settings);
                    foreach (var reflectedSetting in reflectedSettings) {
                        Console.WriteLine ("Setting: {0}", reflectedSetting);
                        var setting = reflectedSetting.setting;

                        if (!setting.optional || (setting.optional && reflectedSetting.value != null)) {
                            foreach (var key in setting.keys) {
                                jobj.Add (new JProperty (key, reflectedSetting.value.ToString ()));
                            }
                        }
                    }
                    ja.Add (jobj);
                    SaveSettingsFile (fileName, jo);
                    successful = true;
                }
            }
            return successful;
        }

        public static bool DeleteEntityInArray (string fileName, string arrayName, string entityName) {
            var successful = false;
            var jo = OpenSettingsFile (fileName) as JObject;
            if (jo != null) {
                var ja = jo[arrayName] as JArray;
                if (ja != null) {
                    var arrayIndex = FindSettingsInArray (ja, entityName);
                    if (arrayIndex != -1) {
                        ja.RemoveAt (arrayIndex);
                        SaveSettingsFile (fileName, jo);
                        successful = true;
                    }
                }
            }
            return successful;
        }

        public static bool UpdateEntityInArray (string fileName, string arrayName, string oldEntityName, IGroupSettings settings) {
            var successful = DeleteEntityInArray (fileName, arrayName, oldEntityName);
            if (successful) {
                successful = AddEntityToArray (fileName, arrayName, settings);
            }
            return successful;
        }

        public static List<ReflectedSetting> GetReflectedSettings (IGroupSettings settings) {
            var reflectedSettings = new List<ReflectedSetting> ();
            Type settingsType = settings.GetType ();
            PropertyInfo[] publicProperties = settingsType.GetProperties (BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in publicProperties) {
                Console.WriteLine ("property: {0}", property);
                var attribute = property.GetCustomAttributes (typeof (PropertySetting), false);
                if (attribute != null && attribute.Length > 0) {
                    var setting = (PropertySetting)attribute[0];
                    var value = property.GetValue (settings);
                    reflectedSettings.Add (new ReflectedSetting (setting, value));
                }
            }
            return reflectedSettings;
        }
    }
}
