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
using Gtk;
using Cairo;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.UserInterface
{
    public class HomeWidgetPlacement
    {
        public int row;
        public int column;
        public int columnWidth;
        public int rowHeight;

        public int x {
            get {
                return column * 105 + 50;
            }
        }

        public int y {
            get {
                return row * 87 + 32;
            }
        }

        public int width {
            get {
                return (columnWidth * 100) + ((columnWidth - 1) * 5);
            }
        }

        public int height {
            get {
                return (rowHeight * 82) + ((rowHeight - 1) * 5);
            }
        }

        public HomeWidgetPlacement (int row, int column) : this (row, column, 1, 1) { }

        public HomeWidgetPlacement (int row, int column, int columnWidth, int rowHeight) {
            this.row = row;
            this.column = column;
            this.columnWidth = columnWidth;
            this.rowHeight = rowHeight;
        }

        public Tuple<int, int>[] ToRowColumnPairs () {
            var pairs = new Tuple<int, int>[columnWidth * rowHeight];
            for (int i = 0; i < pairs.Length; ++i) {
                int interRow;
                if (rowHeight != 1) {
                    interRow = row + (i / (pairs.Length / rowHeight));
                } else {
                    interRow = row;
                }
                var interColumn = column + (i % columnWidth);
                pairs[i] = new Tuple<int, int> (interRow, interColumn);
            }
            return pairs;
        }

        public static Tuple<int, int> GetRowColumn (int x, int y) {
            int column = (x - 50) / 105;
            int row = (y - 32) / 87;
            return new Tuple<int, int> (row, column);
        }
    }
}
