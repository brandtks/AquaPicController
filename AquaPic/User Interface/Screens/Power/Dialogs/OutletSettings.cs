using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class OutletSettings : Gtk.Dialog
    {
        public Fixed fix;

        public OutletSettings () {
            Name = "Power.Outlet.Settings";
            Title = "Outlet Settings";
            WindowPosition = (Gtk.WindowPosition)4;
            SetSizeRequest (600, 320);

            ModifyBg (StateType.Normal, MyColor.NewGtkColor ("grey0"));

            foreach (Widget w in this.Children) {
                Remove (w);
                w.Dispose ();
            }

            fix = new Fixed ();
            fix.SetSizeRequest (600, 320);
        }
    }
}

