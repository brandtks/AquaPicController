using System;
using System.Collections.Generic;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public class CalibrationDialog : Gtk.Dialog
    {
        public Fixed fix;

        public CalibrationDialog (string name) {
            Name = "AquaPic.Calibration." + name;
            Title = name + " Calibration";
            WindowPosition = (Gtk.WindowPosition)4;
            SetSizeRequest (600, 400);

            #if RPI_BUILD
            Decorated = false;
            #endif

            ModifyBg (StateType.Normal, MyColor.NewGtkColor ("grey0"));

            foreach (Widget w in this.Children) {
                Remove (w);
                w.Dispose ();
            }

            fix = new Fixed ();
            fix.SetSizeRequest (600, 400);



            Show ();
        }
    }
}

