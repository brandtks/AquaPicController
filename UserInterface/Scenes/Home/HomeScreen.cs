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
using System.IO;
using System.Text;
using AquaPic.Runtime;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AquaPic.UserInterface
{
    public class HomeWindow : SceneBase
    {
        List<HomeWidget> widgets;
        TileBoard tileBoard;
        NewWidgetLocation newWidgetLocation;
        TouchButton trashButton;

        public HomeWindow (params object[] options) {
            showTitle = false;

            widgets = new List<HomeWidget> ();
            tileBoard = new TileBoard (5, 7);

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "mainScreen.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    var ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                    foreach (var jt in ja) {
                        var jo = jt as JObject;

                        var name = (string)jo["name"];
                        var type = (string)jo["type"];

                        var column = -1;
                        var row = -1;
                        try {
                            column = Convert.ToInt32 (jo["column"]);
                            row = Convert.ToInt32 (jo["row"]);
                        } catch {
                            Logger.AddWarning (string.Format ("Invalid row or column for {0}", name));
                            continue;
                        }

                        HomeWidget widget = null;
                        switch (type) {
                        case "Timer": {
                                widget = new DeluxeTimerWidget (name, row, column);
                                break;
                            }
                        case "LinePlot": {
                                var group = (string)jo["group"];

                                if (HomeWindowWidgets.linePlots.ContainsKey (name)) {
                                    widget = HomeWindowWidgets.linePlots[name].CreateInstance (group, row, column);
                                } else {
                                    Logger.AddWarning (string.Format ("Unknown line plot for main window: {0}", name));
                                }

                                break;
                            }
                        case "BarPlot": {
                                var group = (string)jo["group"];

                                if (HomeWindowWidgets.barPlots.ContainsKey (name)) {
                                    widget = HomeWindowWidgets.barPlots[name].CreateInstance (group, row, column);
                                } else {
                                    Logger.AddWarning (string.Format ("Unknown bar plot for main window: {0}", name));
                                }

                                break;
                            }
                        case "CurvedBarPlot": {
                                var group = (string)jo["group"];

                                if (HomeWindowWidgets.curvedBarPlots.ContainsKey (name)) {
                                    widget = HomeWindowWidgets.curvedBarPlots[name].CreateInstance (group, row, column);
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

                        if (widget != null) {
                            Put (widget, widget.x, widget.y);
                            widget.Show ();
                            widgets.Add (widget);
                            tileBoard.OccupyTiles (widget);
                            widget.WidgetSelectedEvent += OnWidgetSelected;
                            widget.WidgetUnselectedEvent += OnWidgetUnselected;
                            widget.RequestNewTileLocationEvent += OnRequestNewTileLocation;
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

            trashButton = new TouchButton ();
            trashButton.SetSizeRequest (60, 60);
            trashButton.text = "Trash";
            trashButton.buttonColor = "compl";
            trashButton.Visible = false;
            Put (trashButton, 20, 410); 

            Update ();
            Show ();
        }

        protected override bool OnUpdateTimer () {
            Update ();
            return true;
        }

        protected void Update () {
            foreach (var widget in widgets) {
                if (widget is IHomeWidgetUpdatable updateWidget) {
                    updateWidget.Update ();
                }
            }
            QueueDraw ();
        }

        protected void OnRequestNewTileLocation (int x, int y) {
            var pair = HomeWidgetPlacement.GetRowColumn (x, y);

            var newRow = pair.Item1;
            var newColumn = pair.Item2;

            if (newRow == newWidgetLocation.placement.row && newColumn == newWidgetLocation.placement.column) {
                return;
            }

            if (newRow >= 0 && (newRow + newWidgetLocation.placement.rowHeight <= 5) &&
                newColumn >= 0 && (newColumn + newWidgetLocation.placement.columnWidth <= 7)) {
                if (!newWidgetLocation.Visible) {
                    newWidgetLocation.Visible = true;
                }

                tileBoard.FreeTiles (newWidgetLocation);
                newWidgetLocation.placement.row = newRow;
                newWidgetLocation.placement.column = newColumn;
                tileBoard.OccupyTiles (newWidgetLocation);

                if (tileBoard.containsConflictTiles) {
                    newWidgetLocation.color = "compl";
                } else {
                    newWidgetLocation.color = "grey2";
                }
                Remove (newWidgetLocation);
                Put (newWidgetLocation, newWidgetLocation.placement.x, newWidgetLocation.placement.y);
                newWidgetLocation.Show ();
            }
        }

        protected void OnWidgetSelected (HomeWidget widget) {
            newWidgetLocation = new NewWidgetLocation (widget);
            newWidgetLocation.color = "grey2";
            Put (newWidgetLocation, widget.x, widget.y);
            newWidgetLocation.Visible = false;

            Remove (trashButton);
            Put (trashButton, 40, 400);
            trashButton.Visible = true;
        }

        protected void OnWidgetUnselected (HomeWidget widget) {
            if (!tileBoard.containsConflictTiles) {
                widget.row = newWidgetLocation.placement.row;
                widget.column = newWidgetLocation.placement.column;
                Remove (widget);
                Put (widget, widget.x, widget.y);
            }
            newWidgetLocation.Visible = false;
            trashButton.Visible = false;
        }

        private class TileBoard
        {
            Tile[,] tiles;

            public bool containsConflictTiles {
                get {
                    foreach (var tile in tiles) {
                        if (tile.status == TileStatus.Conflict) {
                            return true;
                        }
                    }
                    return false;
                }
            }

            public TileBoard (int rows, int columns) {
                tiles = new Tile[rows, columns];
                for (var row = 0; row < rows; ++row) {
                    for (var column = 0; column < columns; ++column) {
                        tiles[row, column] = new Tile ();
                    }
                }
            }

            public void OccupyTiles (HomeWidget widget) {
                OccupyTiles (widget.pairs);
            }

            public void OccupyTiles (NewWidgetLocation newWidgetLocation) {
                OccupyTiles (newWidgetLocation.placement.ToRowColumnPairs ());
            }

            public void OccupyTiles (Tuple<int, int>[] pairs) {
                foreach (var pair in pairs) {
                    if (tiles[pair.Item1, pair.Item2].status == TileStatus.Free) {
                        tiles[pair.Item1, pair.Item2].status = TileStatus.Occupied;
                    } else if (tiles[pair.Item1, pair.Item2].status == TileStatus.Occupied) {
                        tiles[pair.Item1, pair.Item2].status = TileStatus.Conflict;
                    } else {
                        throw new Exception (string.Format ("The status was {0}", tiles[pair.Item1, pair.Item2].status));
                    }
                }
            }

            public void FreeTiles (HomeWidget widget) {
                FreeTiles (widget.pairs);
            }

            public void FreeTiles (NewWidgetLocation newWidgetLocation) {
                FreeTiles (newWidgetLocation.placement.ToRowColumnPairs ());
            }

            public void FreeTiles (Tuple<int, int>[] pairs) {
                foreach (var pair in pairs) {
                    if (tiles[pair.Item1, pair.Item2].status == TileStatus.Conflict) {
                        tiles[pair.Item1, pair.Item2].status = TileStatus.Occupied;
                    } else {
                        tiles[pair.Item1, pair.Item2].status = TileStatus.Free;
                    }
                }
            }

            enum TileStatus
            {
                Free,
                Occupied,
                Conflict
            }

            private class Tile
            {
                public TileStatus status;

                public Tile () {
                    status = TileStatus.Free;
                }
            }
        }

        private class NewWidgetLocation : TouchGraphicalBox
        {
            public HomeWidgetPlacement placement;

            public NewWidgetLocation (HomeWidget widget) : base (widget.width, widget.height) {
                placement = new HomeWidgetPlacement (widget.row, widget.column, widget.columnWidth, widget.rowHeight);
            }
        }
    }
}

