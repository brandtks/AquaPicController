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
        static int powerStrip1 = -1;
        static int powerStrip2 = -1;
        static int analogInputCard1 = -1;
        static int analogOutputCard1 = -1;

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

            powerStrip1 = Power.AddPowerStrip (16, "PS1", false);
            powerStrip2 = Power.AddPowerStrip (17, "PS2", false);

            // Analog Input
            analogInputCard1 = AnalogInput.AddCard (20, "AI1");

            // Analog Output
            analogOutputCard1 = AnalogOutput.AddCard (30, "AQ1");

            // Digital Input
            DigitalInput.AddCard (40, "DI1");

            // Temperature
            Temperature.AddTemperatureProbe (analogInputCard1, 0, "Sump Temperature");
            Temperature.AddHeater (powerStrip1, 6, "Bottom Heater");
            Temperature.AddHeater (powerStrip1, 7, "Top Heater");

            // Lighting
            Lighting.AddLight (
                "White LED", 
                powerStrip1, 
                0,
                0,
                0,
                analogOutputCard1,
                0,
                AnalogType.ZeroTen,
                10.0f,
                100.0f
            );
            Lighting.AddLight (
                "Actinic LED", 
                powerStrip1, 
                1, 
                -15, 
                15,
                analogOutputCard1,
                1,
                AnalogType.ZeroTen,
                10.0f,
                100.0f
            );
            Lighting.AddLight (
                "Refugium",
                powerStrip1,
                2,
                0,
                0,
                LightingTime.Nighttime
            );

            /*
            Coil plugControl = Power.AddOutlet (powerStrip1, 5, "Test", MyState.On);
            OutletPlugin p = new OutletPlugin ("TestPlugControl", "ScriptTest.cs");
            plugControl.ConditionChecker = delegate() {
                bool b = p.RunOutletCondition ();
                return b;
            };*/

            Plugin.AddPlugins ();

            //uint timer = GLib.Timeout.Add (250, test);

            TaskManager.Start ();

            #if SIMULATION
            AquaPicGUI win = new AquaPicGUI (simulator);
            #else
            AquaPicGUI win = new AquaPicGUI ();
            #endif
            win.Show ();
			Application.Run ();
		}

        protected static bool test () {
            //Lighting.Run ();
            //Temperature.Run ();

            return true;
        }
	}
}
