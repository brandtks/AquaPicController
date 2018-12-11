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
using Gtk;
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
        EventBox emptySpaceEventBox;
        bool hoveredOverTrash;
        uint emptySpacePressTime;

        public HomeWindow (params object[] options) {
            showTitle = false;

            widgets = new List<HomeWidget> ();
            tileBoard = new TileBoard (5, 7);

            emptySpaceEventBox = new EventBox ();
            emptySpaceEventBox.SetSizeRequest (800, 480);
            emptySpaceEventBox.VisibleWindow = false;
            emptySpaceEventBox.ButtonPressEvent += OnEmptySpaceEventBoxButtonPressed;
            emptySpaceEventBox.ButtonReleaseEvent += OnEmptySpaceEventBoxButtonReleased;
            Put (emptySpaceEventBox, 0, 0);
            emptySpaceEventBox.Show ();

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "mainScreen.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    var ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                    foreach (var jt in ja) {
                        var jo = jt as JObject;

                        var name = (string)jo["name"];
                        var type = (string)jo["type"];

                        var group = string.Empty;
                        if (jo.ContainsKey ("group")) {
                            group = (string)jo["group"];
                        }

                        var column = -1;
                        var row = -1;
                        try {
                            column = Convert.ToInt32 (jo["column"]);
                            row = Convert.ToInt32 (jo["row"]);
                        } catch {
                            Logger.AddWarning (string.Format ("Invalid row or column for {0}", name));
                            continue;
                        }

                        var widget = HomeWindowWidgets.GetNewHomeWidget (type, name, group, row, column);

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
            trashButton.SetSizeRequest (40, 40);
            trashButton.text = "Trash";
            trashButton.buttonColor = "compl";
            trashButton.buttonColor.ModifyColor (0.75);
            trashButton.Visible = false;
            Put (trashButton, 755, 435);

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
            if (x.WithinRange (755, 795) && y.WithinRange (435, 475)) {
                hoveredOverTrash = true;
                trashButton.buttonColor = "compl";
                return;
            }

            if (hoveredOverTrash) {
                hoveredOverTrash = false;
                trashButton.buttonColor.ModifyColor (0.75);
            }

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
            Put (trashButton, 755, 435);
            trashButton.Visible = true;
            hoveredOverTrash = false;
        }

        protected void OnWidgetUnselected (HomeWidget widget) {
            if (hoveredOverTrash) {
                Remove (widget);
                widgets.Remove (widget);
            } else {
                if (!tileBoard.containsConflictTiles) {
                    widget.row = newWidgetLocation.placement.row;
                    widget.column = newWidgetLocation.placement.column;
                    Remove (widget);
                    Put (widget, widget.x, widget.y);
                }
            }
            newWidgetLocation.Visible = false;
            trashButton.Visible = false;
        }

        protected void OnEmptySpaceEventBoxButtonPressed (object sender, ButtonPressEventArgs args) {
            emptySpacePressTime = args.Event.Time;
        }

        protected void OnEmptySpaceEventBoxButtonReleased (object sender, ButtonReleaseEventArgs args) {
            if (args.Event.Time - emptySpacePressTime < 1000) {
                return; 
            }

            var parent = Toplevel as Window;
            var addHomeWidgetDialog = new AddHomeWidgetDialog (parent);
            addHomeWidgetDialog.Run ();
            var newWidgetSettings = addHomeWidgetDialog.newWidget;
            addHomeWidgetDialog.Destroy ();
            addHomeWidgetDialog.Dispose ();

            if (newWidgetSettings == null) {
                return;
            }

            var pair = HomeWidgetPlacement.GetRowColumn (args.Event.X.ToInt (), args.Event.Y.ToInt ());

            var row = pair.Item1;
            var column = pair.Item2;

            var widget = HomeWindowWidgets.GetNewHomeWidget (
                newWidgetSettings.type, 
                newWidgetSettings.name, 
                newWidgetSettings.group, 
                row, 
                column);

            if (widget != null) {
                if (row < 0 && (row + widget.rowHeight > 5) && column < 0 && (column + widget.columnWidth > 7)) {
                    MessageBox.Show ("Not enough room on the screen for widget");
                } else {
                    tileBoard.OccupyTiles (widget);

                    if (tileBoard.containsConflictTiles) {
                        tileBoard.FreeTiles (widget);
                        MessageBox.Show ("Widget conflicts with other widgets");
                    } else {
                        Put (widget, widget.x, widget.y);
                        widget.Show ();
                        widgets.Add (widget);
                        widget.WidgetSelectedEvent += OnWidgetSelected;
                        widget.WidgetUnselectedEvent += OnWidgetUnselected;
                        widget.RequestNewTileLocationEvent += OnRequestNewTileLocation;
                    }
                }
            }
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

