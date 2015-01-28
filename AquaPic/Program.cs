using System;
using Gtk;
using AquaPic.Alarm;
using AquaPic.AnalogInput;
using AquaPic.Lighting;
using AquaPic.Power;
using AquaPic.SerialBus;
using AquaPic.Temp;
using AquaPic.Utilites;


namespace AquaPic
{
	class MainClass
	{
        /*
        static int powerStrip1 = -1;
        static int powerStrip2 = -1;
        static int analogInputCard1 = -1;
        */

        public static void Main (string[] args)
		{
            Application.Init ();

            /*
            // AquaPic Bus
            AquaPicBus.Bus1.Open ("Comm3", 9600);

            // Power
            powerStrip1 = power.addPowerStrip (16, "Left Strip");
            powerStrip2 = power.addPowerStrip (17, "Right Strip");

            // Analog Input
            analogInputCard1 = analogInput.addCard (20);

            // Temperature
            temperature.init (analogInputCard1, 0, "Sump Temperature", powerStrip1, 7, "Top Heater");
            temperature.addHeater (powerStrip1, 6, "Bottom Heater");

            // Lighting
            lighting.init (powerStrip1, 0, "White LED", 0, 0, new Time (7, 30, 0), new Time (8, 30, 0));
            lighting.addLight (powerStrip1, 1, "Actinic LED", -15, 15, new Time (7, 30, 0), new Time (8, 30, 0));
            */

            mainWindow mainScreen = new mainWindow ();
            mainScreen.Show ();
			Application.Run ();
		}
	}
}
