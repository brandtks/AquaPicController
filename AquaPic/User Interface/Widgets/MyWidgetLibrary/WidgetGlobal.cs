using System;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public enum MyOrientation : byte {
        Vertical = 1,
        Horizontal
    }

    public static class WidgetGlobal
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
    }

    public class MessageBox {
        public static void Show (string msg) {
            var ms = new MessageDialog (
                null,
                DialogFlags.DestroyWithParent,
                MessageType.Other,
                ButtonsType.Ok,
                msg);
            ms.Run ();
            ms.Destroy ();
        }
    }
}