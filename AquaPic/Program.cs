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
