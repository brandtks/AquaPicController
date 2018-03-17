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
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Modules
{
    public enum AutoTopOffState
    {
        Off,
        Standby,
        Filling,
        Cooldown,
        Error
    }

    public partial class AutoTopOff
    {
        private static Dictionary<string, AutoTopOffGroup> atoGroups;

        public static int atoGroupCount {
            get {
                return atoGroups.Count;
            }
        }

        public static string firstAtoGroup {
            get {
                if (atoGroups.Count > 0) {
                    var first = atoGroups.First ();
                    return first.Key;
                }

                return string.Empty;
            }
        }

        static AutoTopOff () {
            Logger.Add ("Initializing Auto Top Off");

            atoGroups = new Dictionary<string, AutoTopOffGroup> ();

            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, "autoTopOffProperties.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    JObject jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));

                    var ja = (JArray)jo["atoGroups"];
                    foreach (var jt in ja) {
                        var name = (string)jt["name"];

                        bool enable;
                        try {
                            enable = Convert.ToBoolean (jt["enable"]);
                        } catch {
                            enable = false;
                        }

                        var requestBitName = (string)jt["requestBitName"];
                        var waterLevelGroupName = (string)jt["waterLevelGroupName"];

                        uint maximumRuntime;
                        var text = (string)jt["maximumRuntime"];
                        if (text.IsNotEmpty ()) {
                            maximumRuntime = 0U;
                            enable = false;
                        } else {
                            try {
                                maximumRuntime = Timer.ParseTime (text) / 1000;
                            } catch {
                                maximumRuntime = 0U;
                                enable = false;
                            }
                        }

                        uint minimumCooldown;
                        text = (string)jt["minimumCooldown"];
                        if (string.IsNullOrWhiteSpace (text)) {
                            minimumCooldown = uint.MaxValue;
                            enable = false;
                        } else {
                            try {
                                minimumCooldown = Timer.ParseTime (text) / 1000;
                            } catch {
                                minimumCooldown = uint.MaxValue;
                                enable = false;
                            }
                        }

                        bool useAnalogSensors;
                        try {
                            useAnalogSensors = Convert.ToBoolean (jt["useAnalogSensors"]);
                        } catch {
                            useAnalogSensors = false;
                        }


                        float analogOnSetpoint;
                        text = (string)jt["analogOnSetpoint"];
                        if (text.IsEmpty ()) {
                            analogOnSetpoint = 0f;
                            useAnalogSensors = false;
                        } else {
                            try {
                                analogOnSetpoint = Convert.ToSingle (text);
                            } catch {
                                analogOnSetpoint = 0f;
                                useAnalogSensors = false;
                            }
                        }

                        float analogOffSetpoint;
                        text = (string)jt["analogOffSetpoint"];
                        if (string.IsNullOrWhiteSpace (text)) {
                            analogOffSetpoint = 0.0f;
                            useAnalogSensors = false;
                        } else {
                            try {
                                analogOffSetpoint = Convert.ToSingle (text);
                            } catch {
                                analogOffSetpoint = 0.0f;
                                useAnalogSensors = false;
                            }
                        }

                        bool useFloatSwitches;
                        try {
                            useFloatSwitches = Convert.ToBoolean (jt["useFloatSwitches"]);
                        } catch {
                            useFloatSwitches = false;
                        }

                        if (!useFloatSwitches && !useAnalogSensors)
                            enable = false;

                        AddAtoGroup (
                            name,
                            enable,
                            requestBitName,
                            waterLevelGroupName,
                            maximumRuntime,
                            minimumCooldown,
                            useAnalogSensors,
                            analogOnSetpoint,
                            analogOffSetpoint,
                            useFloatSwitches);
                    }
                }
            } else {
                Logger.Add ("ATO settings file did not exist, created new ATO settings");
                var file = File.Create (path);
                file.Close ();

                var jo = new JObject ();
                jo.Add (new JProperty ("atoGroups", new JArray ()));

                File.WriteAllText (path, jo.ToString ());
            }
        }

        public static void Run () {
            foreach (var group in atoGroups.Values) {
                group.GroupRun ();
            }
        }

        /**************************************************************************************************************/
        /* Auto Top Off Groups                                                                                        */
        /**************************************************************************************************************/
        public static void AddAtoGroup (
            string name,
            bool enable,
            string requestBitName,
            string waterLevelGroupName,
            uint maximumRuntime,
            uint minimumCooldown,
            bool useAnalogSensors,
            float analogOnSetpoint,
            float analogOffSetpoint,
            bool useFloatSwitches
        ) {
            if (!AtoGroupNameOk (name)) {
                throw new Exception (string.Format ("ATO Group: {0} already exists", name));
            }

            var atoGroup = new AutoTopOffGroup (
                name,
                enable,
                requestBitName,
                waterLevelGroupName,
                maximumRuntime,
                minimumCooldown,
                useAnalogSensors,
                analogOnSetpoint,
                analogOffSetpoint,
                useFloatSwitches);

            atoGroups.Add (name, atoGroup);
        }

        public static void RemoveAtoGroup (string name) {
            CheckAtoGroupKey (name);
            atoGroups.Remove (name);
        }

        public static void CheckAtoGroupKey (string name) {
            if (!atoGroups.ContainsKey (name)) {
                throw new ArgumentException ("name");
            }
        }

        public static bool CheckAtoGroupKeyNoThrow (string name) {
            try {
                CheckAtoGroupKey (name);
                return true;
            } catch {
                return false;
            }
        }

        public static bool AtoGroupNameOk (string name) {
            return !CheckAtoGroupKeyNoThrow (name);
        }

        public static bool ClearAtoAlarm (string name) {
            CheckAtoGroupKey (name);
            return atoGroups[name].ClearAtoAlarm ();
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllAtoGroupNames () {
            List<string> names = new List<string> ();
            foreach (var waterLevelGroup in atoGroups.Values) {
                names.Add (waterLevelGroup.name);
            }
            return names.ToArray ();
        }


    }
}

