using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    public class MainWindow : WindowBase
    {
        List<LinePlotWidget> linePlots;
        List<BarPlotWidget> barPlots;

        uint timerId;

        public MainWindow (params object[] options) : base () {
            showTitle = false;

            var names = Lighting.GetAllFixtureNames ();
            foreach (var name in names) {
                int fixtureId = Lighting.GetFixtureIndex (name);
                if (Lighting.IsDimmingFixture (fixtureId)) {
                    if (!MainWindowWidgets.barPlots.ContainsKey (name))
                        MainWindowWidgets.barPlots.Add (
                            name, 
                            new BarPlotData (() => {
                                return new DimmingLightBarPlot (
                                    name, 
                                    () => {return Lighting.GetCurrentDimmingLevel (fixtureId);}
                                );}
                            )
                        );
                }
            }

            linePlots = new List<LinePlotWidget> ();
            barPlots = new List<BarPlotWidget> ();

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "mainScreen.json");

            using (StreamReader reader = File.OpenText (path)) {
                JArray ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                foreach (var jt in ja) {
                    var jo = jt as JObject;

                    int x = (Convert.ToInt32 (jo ["column"]) - 1) * 105 + 50;
                    int y = (Convert.ToInt32 (jo ["row"]) - 1) * 87 + 32;

                    string type = (string)jo ["type"];
                    switch (type) {
                    case "Timer": {
                            var t = new DeluxeTimerWidget ((string)jo ["name"]);
                            Put (t, x, y);
                            t.Show ();

                            break;
                        }
                    case "LinePlot": {
                            string name = (string)jo ["name"];

                            if (MainWindowWidgets.linePlots.ContainsKey (name)) {
                                var lp = MainWindowWidgets.linePlots [name].CreateInstance ();
                                Put (lp, x, y);
                                lp.Show ();

                                linePlots.Add (lp);

                            } else {
                                Logger.AddWarning (string.Format ("Unknown line plot for main window: {0}", name));
                            }

                            break;
                        }
                    case "BarPlot": {
                            string name = (string)jo ["name"];

                            if (MainWindowWidgets.barPlots.ContainsKey (name)) {
                                var bp = MainWindowWidgets.barPlots [name].CreateInstance ();
                                Put (bp, x, y);
                                bp.Show ();

                                barPlots.Add (bp);

                            } else {
                                Logger.AddWarning (string.Format ("Unknown bar plot for main window: {0}", name));
                            }

                            break;
                        }
                    case "Button": {
                            string name = (string)jo ["name"];

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

            return true;
        }
    }
}

