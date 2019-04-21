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
using System.Collections.Generic;
using AquaPic.Service;

namespace AquaPic.UserInterface
{
    public class HomeWindowWidgets
    {
        public static Dictionary<string, LinePlotData> linePlots;
        public static Dictionary<string, BarPlotData> barPlots;
        public static Dictionary<string, CurvedBarPlotData> curvedBarPlots;

        static HomeWindowWidgets () {
            linePlots = new Dictionary<string, LinePlotData> {
                { "Temperature", new LinePlotData ((group, row, column) => {return new TemperatureLinePlot (group, row, column);}) },
                { "Water Level", new LinePlotData ((group, row, column) => {return new WaterLevelLinePlot (group, row, column);}) },
                { "pH Probe", new LinePlotData ((group, row, column) => {return new PhProbeLinePlot (group, row, column);}) }
            };

            barPlots = new Dictionary<string, BarPlotData> {
                { "Water Level", new BarPlotData ((group, row, column) => {return new WaterLevelBarPlotWidget (group, row, column);}) }
            };

            curvedBarPlots = new Dictionary<string, CurvedBarPlotData> {
                { "Lighting", new CurvedBarPlotData ((group, row, column) => {return new DimmingLightCurvedBarPlot (group, row, column);}) }
            };
        }

        public static HomeWidget GetNewHomeWidget (string type, string name, int row, int column) {
            return GetNewHomeWidget (type, name, string.Empty, row, column);
        }

        public static HomeWidget GetNewHomeWidget (string type, string name, string group, int row, int column) {
            HomeWidget widget = null;
            switch (type) {
            case "Timer": {
                    widget = new DeluxeTimerWidget (name, row, column);
                    break;
                }
            case "LinePlot": {
                    if (linePlots.ContainsKey (name)) {
                        widget = linePlots[name].CreateInstance (group, row, column);
                    } else {
                        Logger.AddWarning (string.Format ("Unknown line plot for main window: {0}", name));
                    }

                    break;
                }
            case "BarPlot": {
                    if (barPlots.ContainsKey (name)) {
                        widget = barPlots[name].CreateInstance (group, row, column);
                    } else {
                        Logger.AddWarning (string.Format ("Unknown bar plot for main window: {0}", name));
                    }

                    break;
                }
            case "CurvedBarPlot": {
                    if (curvedBarPlots.ContainsKey (name)) {
                        widget = curvedBarPlots[name].CreateInstance (group, row, column);
                    } else {
                        Logger.AddWarning (string.Format ("Unknown bar plot for main window: {0}", name));
                    }

                    break;
                }
            case "Button": {
                    widget = new ButtonWidget (name, row, column);
                    break;
                }
            default:
                Logger.AddWarning (string.Format ("Unknown widget for main window: {0}", type));
                break;
            }

            return widget;
        }
    }
}

