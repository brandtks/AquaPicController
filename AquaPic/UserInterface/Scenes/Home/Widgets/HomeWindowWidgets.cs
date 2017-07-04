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
                { "Water Level", new BarPlotData ((options) => {return new WaterLevelWidget (options);}) }
            };

            curvedBarPlots = new Dictionary<string, CurvedBarPlotData> ();
        }
    }
}

