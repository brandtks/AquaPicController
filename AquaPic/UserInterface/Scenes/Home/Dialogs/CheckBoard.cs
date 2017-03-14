using System;
using Cairo;
using Gtk;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    partial class HomeSettings
    {
        private class CheckerBoard : EventBox
        {
            Tile[,] tiles;

            public bool containsNoConflictTiles {
                get {
                    foreach (var tile in tiles) {
                        if (tile.status == TileStatus.Conflict) {
                            return false;
                        }
                    }
                    return true;
                }
            }

            public CheckerBoard () {
                VisibleWindow = false;
                Visible = true;

                SetSizeRequest (222, 158);
                tiles = new Tile[5, 7];
                for (int row = 0; row < 5; ++row) {
                    for (int column = 0; column < 7; ++column) {
                        tiles[row, column] = new Tile ();
                    }
                }
                ExposeEvent += OnExpose;
            }

            public void OccupyTile (HomeSettingsWidget widget) {
                RowColumnPair[] pairs = widget.pairs;
                foreach (var pair in pairs) {
                    if (tiles[pair.row, pair.column].status == TileStatus.Free) {
                        tiles[pair.row, pair.column].status = TileStatus.Occupied;
                    } else if (tiles[pair.row, pair.column].status == TileStatus.Occupied) {
                        tiles[pair.row, pair.column].status = TileStatus.Conflict;
                    } else {
                        throw new Exception ("Something happened freeing tiles on home screen settings");
                    }
                }
            }

            public void FreeTile (HomeSettingsWidget widget) {
                RowColumnPair[] pairs = widget.pairs;
                foreach (var pair in pairs) {
                    if (tiles[pair.row, pair.column].status == TileStatus.Conflict) {
                        tiles[pair.row, pair.column].status = TileStatus.Occupied;
                    } else {
                        tiles[pair.row, pair.column].status = TileStatus.Free;
                    }
                }
            }

            public void HighlightTile (HomeSettingsWidget widget) {
                UnhighlightTile ();

                RowColumnPair[] pairs = widget.pairs;
                foreach (var pair in pairs) {
                    if (tiles[pair.row, pair.column].status == TileStatus.Occupied) {
                        tiles[pair.row, pair.column].status = TileStatus.Highlighted;
                    }
                }
            }

            public void UnhighlightTile () {
                foreach (var tile in tiles) {
                    if (tile.status == TileStatus.Highlighted) {
                        tile.status = TileStatus.Occupied;
                    }
                }
            }

            protected void OnExpose (object sender, ExposeEventArgs args) {
                using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                    for (int row = 0; row < 5; ++row) {
                        for (int column = 0; column < 7; ++column) {
                            cr.Rectangle (column * 32 + Allocation.Left, row * 32 + Allocation.Top, 30, 30);

                            if (tiles[row, column].status == TileStatus.Occupied) {
                                TouchColor.SetSource (cr, "pri");
                            } else if (tiles[row, column].status == TileStatus.Highlighted) {
                                TouchColor.SetSource (cr, "secb");
                            } else if (tiles[row, column].status == TileStatus.Conflict) {
                                TouchColor.SetSource (cr, "compl");
                            } else {
                                TouchColor.SetSource (cr, "grey4");
                            }

                            cr.Fill ();
                        }
                    }
                }
            }
        }

        enum TileStatus
        {
            Free,
            Occupied,
            Highlighted,
            Conflict
        }

        private class Tile
        {
            public TileStatus status;

            public Tile () {
                status = TileStatus.Free;
            }
        }

        private class RowColumnPair
        {
            public int row;
            public int column;

            public RowColumnPair () {
                row = 0;
                column = 0;
            }
        }
    }
}
