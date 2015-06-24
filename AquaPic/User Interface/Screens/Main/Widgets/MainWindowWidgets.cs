using System;
using System.Collections.Generic;

namespace AquaPic
{
    public class MainWindowWidgets
    {
        public static Dictionary<string, LinePlotData> linePlots;
        public static Dictionary<string, BarPlotData> barPlots;
        public static Dictionary<string, ButtonData> buttons;

        static MainWindowWidgets () {
            linePlots = new Dictionary<string, LinePlotData> ();
            barPlots = new Dictionary<string, BarPlotData> ();
            buttons = new Dictionary<string, ButtonData> ();
        }
    }
}

