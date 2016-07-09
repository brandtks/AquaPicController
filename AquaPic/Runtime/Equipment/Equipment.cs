using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Drivers;
using AquaPic.Modules;

namespace AquaPic.Runtime
{
    public class Equipment
    {
        public static void AddFromJson () {
            string path = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "equipment.json");

            using (StreamReader reader = File.OpenText (path)) {
                JArray ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                foreach (var jt in ja) {
                    var jo = jt as JObject;
                    string type = (string)jo ["type"];
                    switch (type) {
                    case "power":
                        Logger.Add ("Adding power strip");
                        Power.AddPowerStrip (
                            Convert.ToInt32 ((string)jo ["options"] [0], 16),
                            (string)jo ["options"] [1],
                            Convert.ToBoolean (jo ["options"] [2]));
                        break;
                    case "analogInput":
                        Logger.Add ("Adding analog input card");
                        AquaPicDrivers.AnalogInput.AddCard (
                            Convert.ToInt32 ((string)jo ["options"] [0], 16),
                            (string)jo ["options"] [1]);
                        break;
                    case "analogOutput":
                        Logger.Add ("Adding analog output card");
                        AquaPicDrivers.AnalogOutput.AddCard (
                            Convert.ToInt32 ((string)jo ["options"] [0], 16),
                            (string)jo ["options"] [1]);
                        break;
                    case "digitalInput":
                        Logger.Add ("Adding digital input card");
                        AquaPicDrivers.DigitalInput.AddCard (
                            Convert.ToInt32 ((string)jo ["options"] [0], 16),
                            (string)jo ["options"] [1]);
                        break;
                    default:
                        Console.WriteLine ("Unknow equipment type: {0}", type);
                        break;
                    }
                }
            }

            path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "generalProperties.json");

            string jstring = File.ReadAllText (path);
            JObject jobj = (JObject)JToken.Parse (jstring);

            bool autoConnect = Convert.ToBoolean (jobj ["autoConnectAquaPicBus"]);
            if (autoConnect) {
                string port = (string)jobj ["aquaPicBusPort"];
                if (!string.IsNullOrWhiteSpace (port)) {
                    Logger.Add ("Starting AquaPicBus on port " + port);
                    AquaPic.SerialBus.AquaPicBus.Open (port);
                }
            }
        }
    }
}

