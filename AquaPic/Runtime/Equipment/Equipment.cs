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
                        AnalogInput.AddCard (
                            Convert.ToInt32 ((string)jo ["options"] [0], 16),
                            (string)jo ["options"] [1]);
                        break;
                    case "analogOutput":
                        Logger.Add ("Adding analog output card");
                        AnalogOutput.AddCard (
                            Convert.ToInt32 ((string)jo ["options"] [0], 16),
                            (string)jo ["options"] [1]);
                        break;
                    case "digitalInput":
                        Logger.Add ("Adding digital input card");
                        DigitalInput.AddCard (
                            Convert.ToInt32 ((string)jo ["options"] [0], 16),
                            (string)jo ["options"] [1]);
                        break;
                    default:
                        Console.WriteLine ("Unknow equipment type: {0}", type);
                        break;
                    }
                }
            }

            Temperature.Init ();
            Lighting.Init ();
            WaterLevel.Init ();
            try {
                Power.Init ();
            } catch (Exception ex) {
                Console.WriteLine (ex.ToString ());
            }
        }
    }
}

