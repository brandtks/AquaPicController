using System;
using System.Threading;
#if SIMULATION
using System.Diagnostics;
#endif
using Gtk;
using AquaPic.AlarmRuntime;
using AquaPic.AnalogInputDriver;
using AquaPic.AnalogOutputDriver;
using AquaPic.DigitalInputDriver;
using AquaPic.EquipmentRuntime;
using AquaPic.Globals;
using AquaPic.LightingModule;
using AquaPic.PowerDriver;
using AquaPic.SerialBus;
using AquaPic.TaskManagerRuntime;
using AquaPic.CoilRuntime;
using AquaPic.PluginRuntime;
using AquaPic.TemperatureModule;
using AquaPic.Utilites;

namespace AquaPic
{
	class MainClass
	{
        public static void Main (string[] args)
		{
            Application.Init ();

            #if SIMULATION
            AquaPicBus.Bus1.Start ();
            const string FILENAME = @"\VisualStudio\AquaPicSimulator\AquaPicSimulator\bin\Release\AquaPicSimulator.exe";
            string path = string.Format ("{0}{1}", Environment.GetEnvironmentVariable ("AquaPic"), FILENAME);
            Process simulator = Process.Start (path);
            Thread.Sleep (2000);
            #endif

            Equipment.AddFromJson ();

            // Temperature
            Temperature.AddTemperatureProbe (AnalogInput.GetCardIndex ("AI1"), 0, "Sump Temperature");
            Temperature.AddHeater (Power.GetPowerStripIndex("PS1"), 6, "Bottom Heater");
            Temperature.AddHeater (Power.GetPowerStripIndex("PS1"), 7, "Top Heater");

            // Lighting
            int lightingID = Lighting.AddLight (
                "White LED", 
                Power.GetPowerStripIndex("PS1"), 
                0,
                AnalogOutput.GetCardIndex ("AQ1"),
                0,
                10.0f,
                75.0f
            );
            Lighting.SetupAutoOnOffTime (lightingID);

            lightingID = Lighting.AddLight (
                "Actinic LED", 
                Power.GetPowerStripIndex("PS1"), 
                1,
                AnalogOutput.GetCardIndex ("AQ1"),
                1,
                10.0f,
                75.0f
            );
            Lighting.SetupAutoOnOffTime (lightingID, -15, 15);

            lightingID = Lighting.AddLight (
                "Refugium",
                Power.GetPowerStripIndex("PS1"),
                2,
                LightingTime.Nighttime
            );
            Lighting.SetupAutoOnOffTime (lightingID);

            Plugin.AddPlugins ();

            TaskManager.Start ();

            #if SIMULATION
            AquaPicGUI win = new AquaPicGUI (simulator);
            #else
            AquaPicGUI win = new AquaPicGUI ();
            #endif
            win.Show ();
			Application.Run ();
		}
	}
}
