using System;
using System.IO;
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Utilites;
using AquaPic.Drivers;

namespace AquaPic
{
    public class HeaterSettings : TouchSettingsDialog
    {
        public HeaterSettings (int heaterIdx) : base ("Heater") {
            var c = new SettingComboBox ();
            c.label.text = "Outlet";
            string name = Temperature.GetHeaterName (heaterIdx);
            IndividualControl ic = Power.GetOutletIndividualControl (name);
            string psName = Power.GetPowerStringName (ic.Group);
            c.combo.List.Add (string.Format ("Current: {0}.p{1}", psName, ic.Individual));
            c.combo.List.AddRange (Power.GetAllAvaiblableOutlets ());
            c.combo.Active = 0;
            AddSetting (c);

            DrawSettings ();
        }
    }
}

