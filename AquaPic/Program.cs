using System;
using Gtk;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.UserInterface;

namespace AquaPic
{
	class MainClass
	{
        public static void Main (string[] args) {
            Application.Init ();

            Equipment.AddFromJson ();

            string resourceFile;
            string proc = Environment.GetEnvironmentVariable ("PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine);
            if (proc == "x86")
                resourceFile = @"C:\Program Files\Mono\share\themes\Nodoka-Midnight\gtk-2.0\gtkrc";
            else
                resourceFile = @"C:\Program Files (x86)\Mono\share\themes\Nodoka-Midnight\gtk-2.0\gtkrc";
            
            Gtk.Rc.AddDefaultFile (resourceFile);
            Gtk.Rc.Parse (resourceFile);

            AquaPicGUI win = new AquaPicGUI ();

            win.Show ();
            Application.Run ();
		}
    }
}
