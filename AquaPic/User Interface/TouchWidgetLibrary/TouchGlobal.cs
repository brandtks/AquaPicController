using System;
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