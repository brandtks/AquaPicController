using System;
using Gtk;
using AquaPic.Alarm;
using AquaPic.AInput;
using AquaPic.Lights;
using AquaPic.Pwr;
using AquaPic.SerialBus;
using AquaPic.Temp;
using AquaPic.Utilites;


namespace AquaPic
{
	class MainClass
	{
        static int powerStrip1 = -1;
        static int powerStrip2 = -1;
        static int analogInputCard1 = -1;

        public static void Main (string[] args)
		{
            Application.Init ();

            powerStrip1 = Power.Main.addPowerStrip (16, "Left Power Strip");
            powerStrip2 = Power.Main.addPowerStrip (17, "Right Power Strip");

            // Analog Input
            analogInputCard1 = AnalogInput.Main.addCard (20, "Analog Input 1");

            // Temperature
            Temperature.Main.addHeater (powerStrip1, 6, "Bottom Heater");
            Temperature.Main.addHeater (powerStrip1, 7, "Top Heater");
            Temperature.Main.init (analogInputCard1, 0, "Sump Temperature");

            // Lighting
            Lighting.Main.addLight (powerStrip1, 0, "White LED", 0, 0, new Time (7, 30, 0), new Time (8, 30, 0));
            Lighting.Main.addLight (powerStrip1, 1, "Actinic LED", -15, 15, new Time (7, 30, 0), new Time (8, 30, 0));
            Lighting.Main.init ();

            mainWindow mainScreen = new mainWindow ();
            mainScreen.Show ();
			Application.Run ();
		}
	}
}
