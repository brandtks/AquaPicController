using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class MainWindow : WindowBase
    {
//        LinePlotWidget phPlot;
//        LinePlotWidget tempPlot;
//        BarPlotWidget waterLevel;
//        BarPlotWidget whiteLedDimming;
//        BarPlotWidget actinicLedDimming;
//        int testAlarmIndex;

        List<LinePlotWidget> linePlots;
        List<BarPlotWidget> barPlots;

        uint timerId;

        public MainWindow (params object[] options) : base () {
//            TouchButton b1 = new TouchButton ();
//            b1.ButtonReleaseEvent += OnButtonClick;
//            MyState s1 = Bit.Check ("Clean Skimmer");
//            if (s1 == MyState.Set)
//                b1.buttonColor = "pri";
//            else
//                b1.buttonColor = "seca";
//            b1.HeightRequest = 95;
//            b1.WidthRequest = 108;
//            b1.text = "Clean Skimmer";
//            Put (b1, 572, 230);
//
//            TouchButton b2 = new TouchButton ();
//            b2.ButtonReleaseEvent += OnButtonClick;
//            s1 = Bit.Check ("Water Change");
//            if (s1 == MyState.Set)
//                b2.buttonColor = "pri";
//            else
//                b2.buttonColor = "seca";
//            b2.HeightRequest = 95;
//            b2.WidthRequest = 108;
//            b2.text = "Water Change";
//            Put (b2, 685, 230);
//            phPlot = new LinePlotWidget ();
//            phPlot.text = "pH";
//            Put (phPlot, 7, 30);
//
//            tempPlot = new LinePlotWidget ();
//            tempPlot.text = "Temperature";
//            tempPlot.currentValue = Temperature.WaterTemperature;
//            Put (tempPlot, 7, 130);
//
//            var box3 = new LinePlotWidget ();
//            box3.text = "thingy 1";
//            Put (box3, 7, 230);
//
//            var box4 = new LinePlotWidget ();
//            box4.text = "thingy 2";
//            Put (box4, 7, 330);
//
//            waterLevel = new BarPlotWidget ();
//            waterLevel.text = "Water Level";
//            Put (waterLevel, 459, 30);
//
//            whiteLedDimming = new BarPlotWidget ();
//            whiteLedDimming.text = "White LED";
//            Put (whiteLedDimming, 572, 30);
//
//            actinicLedDimming = new BarPlotWidget ();
//            actinicLedDimming.text = "Actinic LED";
//            Put (actinicLedDimming, 685, 30);
//
//            var t = new DeluxeTimerWidget ("Main");
//            Put (t, 459, 330);

//            var b3 = new TouchButton ();
//            b3.ButtonReleaseEvent += OnTestButtonClick;
//            b3.HeightRequest = 95;
//            b3.WidthRequest = 108;
//            b3.text = "Test Alarm";
//            testAlarmIndex = Alarm.Subscribe ("Test Alarm", true);
//            Put (b3, 459, 330);

            linePlots = new List<LinePlotWidget> ();
            barPlots = new List<BarPlotWidget> ();

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "mainScreen.json");

            using (StreamReader reader = File.OpenText (path)) {
                JArray ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                foreach (var jt in ja) {
                    var jo = jt as JObject;

                    int x = (Convert.ToInt32 (jo ["column"]) - 1) * 113 + 7;
                    int y = (Convert.ToInt32 (jo ["row"]) - 1) * 100 + 30;

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

//                            if (MainWindowWidgets.buttons.ContainsKey (name)) {
//                                var b = MainWindowWidgets.buttons [name].CreateInstance ();
//                                Put (b, x, y);
//                                b.Show ();
//
//                            } else {
//                                Logger.AddWarning (string.Format ("Unknown button for main window: {0}", name));
//                            }

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

