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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cairo;
using Gtk;
using AquaPic.Runtime;
using AquaPic.Globals;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    partial class HomeSettings
    {
        private class HomeSettingsWidget
        {
            public string name;
            public string group;
            public string type;
            HomeWidgetPlacement placement;

            public RowColumnPair[] pairs {
                get {
                    return placement.ToRowColumnPairs ();
                }
            }

            public int rowOrigin {
                get {
                    return placement.rowOrigin;
                }
                set {
                    placement.rowOrigin = value;
                }
            }

            public int columnOrigin {
                get {
                    return placement.columnOrigin;
                }
                set {
                    placement.columnOrigin = value;
                }
            }

            public int width {
                get {
                    return placement.width;
                }
            }

            public int height {
                get {
                    return placement.height;
                }
            }

            public string fullName {
                get {
                    return string.Format ("{0} {1} ({2})", name, group, type);
                }
            }

            public HomeSettingsWidget (string name, string group, string type, int row, int column) {
                this.name = name;
                this.group = group;
                this.type = type;
                placement = new HomeWidgetPlacement (row, column);

                switch (type) {
                case "Timer": {
                        placement.width = 3;
                        placement.height = 2;
                        break;
                    }
                case "LinePlot": {
                        placement.width = 3;
                        placement.height = 1;
                        break;
                    }
                case "BarPlot": {
                        placement.width = 1;
                        placement.height = 2;
                        break;
                    }
                case "CurvedBarPlot": {
                        placement.width = 2;
                        placement.height = 2;
                        break;
                    }
                case "Button": {
                        placement.width = 1;
                        placement.height = 1;
                        break;
                    }
                default:
                    break;
                }
            }
        }

        private class HomeWidgetPlacement
        {
            public int rowOrigin;
            public int columnOrigin;
            public int width;
            public int height;

            public HomeWidgetPlacement (int rowOrigin, int columnOrigin) : this (rowOrigin, columnOrigin, 1, 1) { }

            public HomeWidgetPlacement (int rowOrigin, int columnOrigin, int width, int height) {
                this.rowOrigin = rowOrigin;
                this.columnOrigin = columnOrigin;
                this.width = width;
                this.height = height;
            }

            public RowColumnPair[] ToRowColumnPairs () {
                var pairs = new RowColumnPair[width * height];
                for (int i = 0; i < pairs.Length; ++i) {
                    pairs[i] = new RowColumnPair ();
                    if (height != 1) {
                        pairs[i].row = rowOrigin + (i / (pairs.Length / height));
                    } else {
                        pairs[i].row = rowOrigin;
                    }
                    pairs[i].column = columnOrigin + (i % width);
                }
                return pairs;
            }
        }
    }
}
