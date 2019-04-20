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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using AquaPic.SerialBus;
using AquaPic.Service;

namespace AquaPic.Drivers
{
    public class Driver
    {
        public static AnalogInputBase AnalogInput = AnalogInputBase.SharedAnalogInputInstance;
        public static AnalogOutputBase AnalogOutput = AnalogOutputBase.SharedAnalogOutputInstance;
        public static DigitalInputBase DigitalInput = DigitalInputBase.SharedDigitalInputInstance;
        public static PhOrpBase PhOrp = PhOrpBase.SharedPhOrpInstance;
        public static PowerBase Power = PowerBase.SharedPowerInstance;

        public static void AddFromJson () {
            var path = Path.Combine (Utils.AquaPicSettings, "hardware.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    var ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                    foreach (var jt in ja) {
                        var jo = jt as JObject;
                        var type = (string)jo["type"];
                        var address = Convert.ToInt32 ((string)jo["address"], 16);
                        var name = (string)jo["name"];
                        switch (type) {
                        case "power":
                            Logger.Add ("Adding power strip");
                            Power.AddCard (name, address);
                            break;
                        case "analogInput":
                            Logger.Add ("Adding analog input card");
                            AnalogInput.AddCard (name, address);
                            break;
                        case "analogOutput":
                            Logger.Add ("Adding analog output card");
                            AnalogOutput.AddCard (name, address);
                            break;
                        case "digitalInput":
                            Logger.Add ("Adding digital input card");
                            DigitalInput.AddCard (name, address);
                            break;
                        case "phOrp":
                            Logger.Add ("Adding pH/ORP card");
                            PhOrp.AddCard (name, address);
                            break;
                        default:
                            Console.WriteLine ("Unknown equipment type: {0}", type);
                            break;
                        }
                    }
                }
            } else {
                Logger.Add ("Equipment file did not exist, created new equipment file");
                var file = File.Create (path);
                file.Close ();

                var ja = new JArray ();
                File.WriteAllText (path, ja.ToString ());
            }

            path = Path.Combine (Utils.AquaPicSettings, "generalProperties.json");

            if (File.Exists (path)) {
                var jstring = File.ReadAllText (path);
                var jobj = (JObject)JToken.Parse (jstring);

                bool autoConnect;
                try {
                    autoConnect = Convert.ToBoolean (jobj["autoConnectAquaPicBus"]);
                } catch {
                    autoConnect = false;
                }

                if (autoConnect) {
                    var port = (string)jobj["aquaPicBusPort"];
                    if (!string.IsNullOrWhiteSpace (port)) {
                        Logger.Add ("Starting AquaPicBus on port " + port);
                        AquaPicBus.Open (port);
                    }
                }
            } else {
                Logger.Add ("General settings file did not exist, created new general settings");
                var file = File.Create (path);
                file.Close ();

                var jo = new JObject ();
                jo.Add (new JProperty ("autoConnectAquaPicBus", "false"));
                jo.Add (new JProperty ("aquaPicBusPort", string.Empty));
                File.WriteAllText (path, jo.ToString ());
            }
        }
    }
}

