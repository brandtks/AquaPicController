using System;
#if SIMULATION
using System.Diagnostics;
using System.Threading;
#endif
using Gtk;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic
{
	class MainClass
	{
        public static void Main (string[] args) {
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

            //string RESOURCE_FILE = @"C:\Program Files (x86)\Mono\share\themes\Nodoka-Midnight\gtk-2.0\gtkrc";
            string RESOURCE_FILE = @"C:\Program Files\Mono\share\themes\Nodoka-Midnight\gtk-2.0\gtkrc";
            Gtk.Rc.AddDefaultFile (RESOURCE_FILE);
            Gtk.Rc.Parse (RESOURCE_FILE);

            //<Test> here to test time of day interrupts
//            Time now = new Time (); // sets the instanse with DateTime.Now
//            now.AddMinutes (1);
//            TaskManager.AddTimeOfDayInterrupt ("test1", new Time (now), () => Console.WriteLine ("Test 1 time of day run"));
//            now.AddMinutes (1);
//            TaskManager.AddTimeOfDayInterrupt ("test2", new Time (now), () => Console.WriteLine ("Test 2 time of day run"));

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
