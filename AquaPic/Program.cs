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

            int powerStrip1 = Power.AddPowerStrip (16, "PS1", false);
            int powerStrip2 = Power.AddPowerStrip (17, "PS2", false);

            // Analog Input
            int analogInputCard1 = AnalogInput.AddCard (20, "AI1");

            // Analog Output
            int analogOutputCard1 = AnalogOutput.AddCard (30, "AQ1");

            // Digital Input
            DigitalInput.AddCard (40, "DI1");

            // Temperature
            Temperature.AddTemperatureProbe (analogInputCard1, 0, "Sump Temperature");
            Temperature.AddHeater (powerStrip1, 6, "Bottom Heater");
            Temperature.AddHeater (powerStrip1, 7, "Top Heater");

            // Lighting
            int lightingID = Lighting.AddLight (
                "White LED", 
                powerStrip1, 
                0,
                analogOutputCard1,
                0,
                10.0f,
                75.0f
            );
            Lighting.SetupAutoOnOffTime (lightingID);

            lightingID = Lighting.AddLight (
                "Actinic LED", 
                powerStrip1, 
                1,
                analogOutputCard1,
                1,
                10.0f,
                75.0f
            );
            Lighting.SetupAutoOnOffTime (lightingID, -15, 15);

            lightingID = Lighting.AddLight (
                "Refugium",
                powerStrip1,
                2,
                LightingTime.Nighttime
            );
            Lighting.SetupAutoOnOffTime (lightingID);

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
