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
using Cairo;
using Gtk;

namespace TouchWidgetLibrary
{
    public enum TouchOrientation : byte {
        Vertical = 1,
        Horizontal
    }

    public enum TouchAlignment : byte {
        Right = 1,
        Left,
        Center
    }

    public enum TouchTextWrap : byte {
        None = 1,
        WordWrap,
        Shrink
    }

    public enum UnitsOfMeasurement {
        None,
        Degrees,
        Percentage,
        Inches,
        Amperage
    }

    public static class TouchGlobal
    {
        public static void DrawRoundedRectangle (Cairo.Context cr, double x, double y, double width, double height, double radius) {
            cr.Save ();

            if ((radius > height / 2) || (radius > width / 2))
                radius = Math.Min (height / 2, width / 2);

            cr.MoveTo (x, y + radius);
            cr.Arc (x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
            cr.LineTo (x + width - radius, y);
            cr.Arc (x + width - radius, y + radius, radius, -Math.PI / 2, 0);
            cr.LineTo (x + width, y + height - radius);
            cr.Arc (x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
            cr.LineTo (x + radius, y + height);
            cr.Arc (x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);

            cr.ClosePath ();
            cr.Restore ();
        }

        public static double CalcX (double originX, double radius, double radians) {
            return originX + radius * Math.Cos (radians);
        }

        public static double CalcY (double originY, double radius, double radians) {
            return originY + radius * Math.Sin (radians);
        }
    }
}