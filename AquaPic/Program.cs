using System;
#if SIMULATION
using System.Diagnostics;
using System.Threading;
#endif
using Gtk;
using AquaPic.Drivers;
using AquaPic.Modules;
using AquaPic.Runtime;
using AquaPic.SerialBus;
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

            TaskManager.Start ();

//            string Resource_File = @"C:\Program Files\GTK2-Runtime\share\themes\Unity\gtk-2.0\gtkrc";
//            Gtk.Rc.AddDefaultFile (Resource_File);
//            Gtk.Rc.Parse (Resource_File);

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
