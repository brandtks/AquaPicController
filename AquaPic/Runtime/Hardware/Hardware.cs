#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Drivers;
using AquaPic.Utilites;
using AquaPic.SerialBus;

namespace AquaPic.Runtime
{
    public class Hardware
    {
        public static void AddFromJson () {
            var path = Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "equipment.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    JArray ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                    foreach (var jt in ja) {
                        var jo = jt as JObject;
                        string type = (string)jo["type"];
                        switch (type) {
                        case "power":
                            Logger.Add ("Adding power strip");
                            Power.AddPowerStrip (
                                Convert.ToInt32 ((string)jo["options"][0], 16),
                                (string)jo["options"][1],
                                Convert.ToBoolean (jo["options"][2]));
                            break;
                        case "analogInput":
                            Logger.Add ("Adding analog input card");
                            AquaPicDrivers.AnalogInput.AddCard (
                                Convert.ToInt32 ((string)jo["options"][0], 16),
                                (string)jo["options"][1]);
                            break;
                        case "analogOutput":
                            Logger.Add ("Adding analog output card");
                            AquaPicDrivers.AnalogOutput.AddCard (
                                Convert.ToInt32 ((string)jo["options"][0], 16),
                                (string)jo["options"][1]);
                            break;
                        case "digitalInput":
                            Logger.Add ("Adding digital input card");
                            AquaPicDrivers.DigitalInput.AddCard (
                                Convert.ToInt32 ((string)jo["options"][0], 16),
                                (string)jo["options"][1]);
                            break;
                        default:
                            Console.WriteLine ("Unknow equipment type: {0}", type);
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

            path = Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "generalProperties.json");

            if (File.Exists (path)) {
                string jstring = File.ReadAllText (path);
                JObject jobj = (JObject)JToken.Parse (jstring);

                bool autoConnect = Convert.ToBoolean (jobj["autoConnectAquaPicBus"]);
                if (autoConnect) {
                    string port = (string)jobj["aquaPicBusPort"];
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

