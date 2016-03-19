using System;
using System.Collections.Generic;

namespace AquaPic.UserInterface
{
    public class HomeWindowWidgets
    {
        public static Dictionary<string, LinePlotData> linePlots;
        public static Dictionary<string, BarPlotData> barPlots;

        static HomeWindowWidgets () {
            linePlots = new Dictionary<string, LinePlotData> () {
                { "Temperature", new LinePlotData (() => {return new TemperatureLinePlot ();}) },
                { "Water Level", new LinePlotData (() => {return new WaterLevelLinePlot ();}) }
            };

            barPlots = new Dictionary<string, BarPlotData> () {
                { "Water Level", new BarPlotData (() => {return new WaterLevelWidget ();}) }
            };
        }
    }
}

