using System;
using System.Collections.Generic;

namespace AquaPic.UserInterface
{
    public class HomeWindowWidgets
    {
        public static Dictionary<string, LinePlotData> linePlots;
        public static Dictionary<string, BarPlotData> barPlots;
        public static Dictionary<string, CurvedBarPlotData> curvedBarPlots;

        static HomeWindowWidgets () {
            linePlots = new Dictionary<string, LinePlotData> () {
                { "Temperature", new LinePlotData ((options) => {return new TemperatureLinePlot (options);}) },
                { "Water Level", new LinePlotData ((options) => {return new WaterLevelLinePlot (options);}) }
            };

            barPlots = new Dictionary<string, BarPlotData> () {
                { "Water Level", new BarPlotData (() => {return new WaterLevelWidget ();}) },
                { "ATO Reservoir Level", new BarPlotData (() => {return new AtoReservoirLevelWidget ();}) }
            };

            curvedBarPlots = new Dictionary<string, CurvedBarPlotData> ();
        }
    }
}

