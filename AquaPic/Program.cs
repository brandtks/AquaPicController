using System;
using System.IO;
using Gtk;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.UserInterface;
using AquaPic.Drivers;
using AquaPic.Modules;
using AquaPic.SerialBus;

namespace AquaPic
{
	class MainClass
	{
        public static void Main (string[] args) {
            //Call the Gtk library hack because Windows sucks at everything
            if (Utils.ExecutingOperatingSystem == Platform.Windows)
                CheckWindowsGtk ();

            //Get the AquaPic directory environment
            string aquaPicEnvironment = string.Empty;
            aquaPicEnvironment = Environment.GetEnvironmentVariable ("AquaPic");
            if (aquaPicEnvironment.IsEmpty ()) {
                if (File.Exists ("AquaPicEnvironment.txt")) {
                    var lines = File.ReadAllLines ("AquaPicEnvironment.txt");
                    aquaPicEnvironment = lines[0];
                }
            }

            if (aquaPicEnvironment.IsNotEmpty ()) {
                string path = Path.Combine (aquaPicEnvironment, "AquaPicRuntimeProject");
                if (!Directory.Exists (path)) {
                    Console.WriteLine ("Path to AquaPic directory environment is incorrect");
                    aquaPicEnvironment = string.Empty;
                }
            }

            if (aquaPicEnvironment.IsEmpty ()) {
                Console.WriteLine ("Please add an environment variable or file in execution path");
                Console.WriteLine ("with the path to the AquaPic directory environment");
                Application.Quit ();
                return;
            }

            Utils.AquaPicEnvironment = aquaPicEnvironment;

            //Setup
            Application.Init ();

            Logger.Add ("Executing operating system is {0}", Utils.GetDescription (Utils.ExecutingOperatingSystem));

#if DEBUG
            try {
#endif
                Equipment.AddFromJson ();
                Temperature.Init ();
                Lighting.Init ();
                WaterLevel.Init ();
                Power.Init ();
#if DEBUG
            } catch (Exception ex) {
                Logger.AddError (ex.ToString ());
                return;
            }
#endif

            //Run the control
            AquaPicGUI win = new AquaPicGUI ();
            win.Show ();

            Application.Run ();

            //Cleanup
            if (AquaPicBus.isOpen) {
                AquaPicBus.Close ();
            }

#if DEBUG
            //for some reason this doesn't like to be in the destroyed event
            var groups = Temperature.GetAllTemperatureGroupNames ();
            foreach (var group in groups) {
                Temperature.GetTemperatureGroupDataLogger (group).DeleteAllLogFiles ();
            }
            WaterLevel.dataLogger.DeleteAllLogFiles ();
#endif
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
