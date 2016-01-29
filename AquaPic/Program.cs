using System;
using System.IO;
using Gtk;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.UserInterface;
using AquaPic.Drivers;
using AquaPic.Modules;

namespace AquaPic
{
	class MainClass
	{
        public static void Main (string[] args) {
            //Call the Gtk library hack because Windows sucks at everything
            if (Utils.RunningPlatform == Platform.Windows)
                CheckWindowsGtk ();

            Application.Init ();

            Logger.Add ("Executing operating system is {0}", Utils.GetDescription (Utils.RunningPlatform));

            try {
                Equipment.AddFromJson ();

                Temperature.Init ();
                Lighting.Init ();
                WaterLevel.Init ();
                Power.Init ();
            } catch (Exception ex) {
                Logger.AddError (ex.ToString ());
            }

            AquaPicGUI win = new AquaPicGUI ();
            win.Show ();

            win.DestroyEvent += (o, a) => {
                if (AquaPic.SerialBus.AquaPicBus.isOpen) {
                    AquaPic.SerialBus.AquaPicBus.Close ();
                }
            };

            Application.Run ();
		}

        //Gtk library hack because Windows
        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool SetDllDirectory (string lpPathName);

        static bool CheckWindowsGtk () {
            string location = null;
            Version version = null;
            Version minVersion = new Version (2, 12, 30 );

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Xamarin\GtkSharp\InstallFolder")) {
                if (key != null)
                    location = key.GetValue (null) as string;
            }

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey (@"SOFTWARE\Xamarin\GtkSharp\Version")) {
                if (key != null)
                    Version.TryParse (key.GetValue (null) as string, out version);
            }

            if (version == null || version < minVersion || location == null || !File.Exists (Path.Combine (location, "bin", "libgtk-win32-2.0-0.dll"))) {
                Console.WriteLine ("Did not find required GTK# installation");
                return false;
            }

            Console.WriteLine ("Found GTK# version " + version);
            var path = Path.Combine (location, @"bin");
            Console.WriteLine ("SetDllDirectory(\"{0}\") ", path);

            try {
                if (SetDllDirectory (path)) {
                    return true;
                }
            } catch (EntryPointNotFoundException) { 
                //
            }

            // this shouldn't happen unless something is weird in Windows
            Console.WriteLine ("Unable to set GTK# dll directory");
            return true;
        }
    }
}
