using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Runtime;
using AquaPic.Modules;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class HomeWindow : SceneBase
    {
        List<LinePlotWidget> linePlots;
        List<BarPlotWidget> barPlots;
        List<CurvedBarPlotWidget> curvedBarPlots;

        uint timerId;

        public HomeWindow (params object[] options) : base () {
            showTitle = false;

            if (Lighting.fixtureCount > 0) {
                var names = Lighting.GetAllFixtureNames ();
                foreach (var name in names) {
                    if (Lighting.IsDimmingFixture (name)) {
                        if (!HomeWindowWidgets.curvedBarPlots.ContainsKey (name)) {
                            HomeWindowWidgets.curvedBarPlots.Add (
                                name,
                                new CurvedBarPlotData (() => {
                                    return new DimmingLightBarPlot (
                                        name,
                                        () => {
                                            return Lighting.GetCurrentDimmingLevel (name);
                                        }
                                    );
                                }
                                )
                            );
                        }
                    }
                }
            }

            linePlots = new List<LinePlotWidget> ();
            barPlots = new List<BarPlotWidget> ();
            curvedBarPlots = new List<CurvedBarPlotWidget> ();

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "mainScreen.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    JArray ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                    foreach (var jt in ja) {
                        var jo = jt as JObject;

                        int x = (Convert.ToInt32 (jo["column"])) * 105 + 50;
                        int y = (Convert.ToInt32 (jo["row"])) * 87 + 32;

                        string type = (string)jo["type"];
                        switch (type) {
                        case "Timer": {
                                var t = new DeluxeTimerWidget ((string)jo["name"]);
                                Put (t, x, y);
                                t.Show ();

                                break;
                            }
                        case "LinePlot": {
                                string name = (string)jo["name"];
                                var group = (string)jo["group"];

                                if (HomeWindowWidgets.linePlots.ContainsKey (name)) {
                                    var lp = HomeWindowWidgets.linePlots[name].CreateInstance (group);
                                    Put (lp, x, y);
                                    lp.Show ();

                                    linePlots.Add (lp);

                                } else {
                                    Logger.AddWarning (string.Format ("Unknown line plot for main window: {0}", name));
                                }

                                break;
                            }
                        case "BarPlot": {
                                string name = (string)jo["name"];

                                if (HomeWindowWidgets.barPlots.ContainsKey (name)) {
                                    var bp = HomeWindowWidgets.barPlots[name].CreateInstance ();
                                    Put (bp, x, y);
                                    bp.Show ();

                                    barPlots.Add (bp);

                                } else {
                                    Logger.AddWarning (string.Format ("Unknown bar plot for main window: {0}", name));
                                }

                                break;
                            }
                        case "CurvedBarPlot": {
                                string name = (string)jo["name"];

                                if (HomeWindowWidgets.curvedBarPlots.ContainsKey (name)) {
                                    var bp = HomeWindowWidgets.curvedBarPlots[name].CreateInstance ();
                                    Put (bp, x, y);
                                    bp.Show ();

                                    curvedBarPlots.Add (bp);

                                } else {
                                    Logger.AddWarning (string.Format ("Unknown bar plot for main window: {0}", name));
                                }

                                break;
                            }
                        case "Button": {
                                string name = (string)jo["name"];

                                var b = new ButtonWidget (name);
                                Put (b, x, y);
                                b.Show ();

                                break;
                            }
                        default:
                            Logger.AddWarning (string.Format ("Unknown widget for main window: {0}", type));
                            break;
                        }
                    }
                }
            } else {
                Logger.Add ("Home screen file did not exist, created new home screen file");
                var file = File.Create (path);
                file.Close ();

                var ja = new JArray ();
                File.WriteAllText (path, ja.ToString ());
            }

            OnUpdateTimer ();

            timerId = GLib.Timeout.Add (1000, OnUpdateTimer);

            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        protected bool OnUpdateTimer () {
            foreach (var lp in linePlots) {
                lp.OnUpdate ();
                lp.QueueDraw ();
            }

            foreach (var bp in barPlots) {
                bp.OnUpdate ();
                bp.QueueDraw ();
            }

            foreach (var curvedBarPlot in curvedBarPlots) {
                curvedBarPlot.OnUpdate ();
                curvedBarPlot.QueueDraw ();
            }

            return true;
        }
    }
}

