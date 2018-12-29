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
using AquaPic.Globals;

namespace AquaPic.Runtime
{
    public class SettingsHelper
    {
        public static bool SettingsFileExists (string fileName) {
            if (!fileName.EndsWith (".json", StringComparison.InvariantCultureIgnoreCase)) {
                fileName = string.Format ("{0}.json", fileName);
            }
            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, fileName);
            return File.Exists (path);
        }

        public static JToken OpenSettingsFile (string fileName) {
            if (!fileName.EndsWith (".json", StringComparison.InvariantCultureIgnoreCase)) {
                fileName = string.Format ("{0}.json", fileName);
            }
            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, fileName);

            string json = File.ReadAllText (path);
            return JToken.Parse (json);
        }

        public static void WriteSettingsFile (string fileName, JToken settings) {
            if (!fileName.EndsWith (".json", StringComparison.InvariantCultureIgnoreCase)) {
                fileName = string.Format ("{0}.json", fileName);
            }
            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, fileName);

            if (!File.Exists (path)) {
                var file = File.Create (path);
                file.Close ();
            }

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

        public static EntitySettings[] ReadAllSettingsInArray<EntitySettings> (string fileName, string arrayName) where EntitySettings : IEntitySettings {
            // Create a list to store all the entity settings
            var allEntitySettings = new List<EntitySettings> ();

            // Open the settings file and read in the JSON object
            var jo = OpenSettingsFile (fileName) as JObject;
            // If the JSON object exists
            if (jo != null) {
                // Get the JSON array
                var ja = jo[arrayName] as JArray;
                // If the JSON array exists
                if (ja != null) {
                    // Get the type of the entity settings
                    var entitySettingsType = typeof (EntitySettings);
                    // For each of the JSON tokens in the array
                    foreach (var jt in ja) {
                        // Convert the JSCON token to a JSON object
                        var jobj = jt as JObject;
                        // Create a new instance of the entity settings
                        var setting = (EntitySettings)Activator.CreateInstance (entitySettingsType);
                        // Use reflected to find all the properties
                        var reflectedSettings = GetReflectedSettings (setting);
                        // For each of the properties from the settings reflection
                        foreach (var reflectedSetting in reflectedSettings) {
                            // Get the custom entity setting attribute
                            var entitySettingAttribute = reflectedSetting.entitySettingAttribute;
                            // Get the mutator type
                            var mutatorType = entitySettingAttribute.mutatorType;
                            // Get the read method from the mutator type
                            var readMethod = mutatorType.GetMethod ("Read");
                            // Create a new instance of the mutator type
                            var mutator = Activator.CreateInstance (mutatorType);
                            // Invoke the read method, Read (JObject jobj, string[] keys)
                            var value = readMethod.Invoke (mutator, new object[] { jobj, entitySettingAttribute.keys });
                            // Find the property that is currently being read
                            var propertyInfo = entitySettingsType.GetProperty (reflectedSetting.propertyName);
                            // Set that property to the read value
                            propertyInfo.SetValue (setting, value);
                        }
                        // Add the new settings to the list
                        allEntitySettings.Add (setting);
                    }
                }
            }
            // convert the list to an array and return
            return allEntitySettings.ToArray ();
        }

        public static bool AddSettingsToArray (string fileName, string arrayName, IEntitySettings settings, int index = -1) {
            var successful = false;
            var jo = OpenSettingsFile (fileName) as JObject;
            if (jo != null) {
                var ja = jo[arrayName] as JArray;
                if (ja != null) {
                    var jobj = new JObject ();
                    var reflectedSettings = GetReflectedSettings (settings);
                    foreach (var reflectedSetting in reflectedSettings) {
                        var entitySettingAttribute = reflectedSetting.entitySettingAttribute;
                        var mutatorType = entitySettingAttribute.mutatorType;
                        var writeMethod = mutatorType.GetMethod ("Write");
                        var validMethod = mutatorType.GetMethod ("Valid");
                        var mutator = Activator.CreateInstance (mutatorType);
                        var valid = (bool)validMethod.Invoke (mutator, new object[] { reflectedSetting.propertyValue });
                        if (!entitySettingAttribute.optional || valid) {
                            writeMethod.Invoke (mutator, new object[] { reflectedSetting.propertyValue, jobj, entitySettingAttribute.keys });
                        }
                    }
                    if (index == -1) {
                        ja.Add (jobj);
                    } else {
                        ja.Insert (index, jobj);
                    }
                    WriteSettingsFile (fileName, jo);
                    successful = true;
                }
            }
            return successful;
        }

        public static int DeleteSettingsFromArray (string fileName, string arrayName, string entityName) {
            var arrayIndex = -1;
            var jo = OpenSettingsFile (fileName) as JObject;
            if (jo != null) {
                var ja = jo[arrayName] as JArray;
                if (ja != null) {
                    arrayIndex = FindSettingsInArray (ja, entityName);
                    if (arrayIndex != -1) {
                        ja.RemoveAt (arrayIndex);
                        WriteSettingsFile (fileName, jo);
                    }
                }
            }
            return arrayIndex;
        }

        public static bool UpdateSettingsInArray (string fileName, string arrayName, string oldEntityName, IEntitySettings settings) {
            var index = DeleteSettingsFromArray (fileName, arrayName, oldEntityName);
            var successful = AddSettingsToArray (fileName, arrayName, settings, index);
            return successful & (index != -1);
        }

        public static List<ReflectedSetting> GetReflectedSettings (IEntitySettings settings) {
            var reflectedSettings = new List<ReflectedSetting> ();
            Type settingsType = settings.GetType ();
            PropertyInfo[] publicProperties = settingsType.GetProperties (BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in publicProperties) {
                var attribute = property.GetCustomAttributes (typeof (EntitySettingAttribute), false);
                if (attribute != null && attribute.Length > 0) {
                    var entitySettingAttribute = (EntitySettingAttribute)attribute[0];
                    var propertyName = property.Name;
                    var propertyValue = property.GetValue (settings);
                    reflectedSettings.Add (new ReflectedSetting (entitySettingAttribute, propertyName, propertyValue));
                }
            }
            return reflectedSettings;
        }
    }

    public class ReflectedSetting
    {
        public EntitySettingAttribute entitySettingAttribute;
        public string propertyName;
        public object propertyValue;

        public ReflectedSetting (EntitySettingAttribute entitySettingAttribute, string propertyName, object propertyValue) {
            this.entitySettingAttribute = entitySettingAttribute;
            this.propertyName = propertyName;
            this.propertyValue = propertyValue;
        }
    }
}
