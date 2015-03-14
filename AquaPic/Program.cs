using System;
using Gtk;
using AquaPic.AlarmRuntime;
using AquaPic.AnalogInputDriver;
using AquaPic.AnalogOutputDriver;
using AquaPic.Globals;
using AquaPic.LightingModule;
using AquaPic.PowerDriver;
using AquaPic.SerialBus;
using AquaPic.TaskManagerRuntime;
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

            powerStrip1 = Power.AddPowerStrip (16, "Left Power Strip", false);
            powerStrip2 = Power.AddPowerStrip (17, "Right Power Strip", false);

            // Analog Input
            analogInputCard1 = AnalogInput.AddCard (20, "Analog Input 1");

            // Analog Output
            analogOutputCard1 = AnalogOutput.AddCard (30, "Analog Output 1");

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
                0.0f,
                75.0f
            );
            Lighting.AddLight (
                "Actinic LED", 
                powerStrip1, 
                1, 
                -15, 
                15,
                analogOutputCard1,
                0,
                AnalogType.ZeroTen,
                0.0f,
                75.0f
            );
            Lighting.AddLight (
                "Refugium",
                powerStrip1,
                2,
                0,
                0,
                LightingTime.Nighttime
            );

            //uint timer = GLib.Timeout.Add (250, test);

            TaskManager.Start ();
             
            AquaPicGUI win = new AquaPicGUI ();
            win.Show ();
			Application.Run ();
		}

        protected static bool test () {
            Lighting.Run ();
            Temperature.Run ();

            return true;
        }
	}
}
