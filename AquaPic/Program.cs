#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿﻿using System;
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
            string aquaPicEnvironment = string.Empty;
            //Call the Gtk library hack because Windows sucks at everything
            if (Utils.ExecutingOperatingSystem == Platform.Windows) {
                CheckWindowsGtk ();
                
                //Get the AquaPic directory environment
                aquaPicEnvironment = Environment.GetEnvironmentVariable ("AquaPic");
                if (aquaPicEnvironment.IsEmpty ()) {
                    var path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    path = Path.Combine(path, "AquaPicEnvironment.txt");
                    if (File.Exists(path)) {
                        var lines = File.ReadAllLines(path);
                        aquaPicEnvironment = lines[0];
                    }
                }

                if (aquaPicEnvironment.IsNotEmpty ()) {
                    var path = Path.Combine (aquaPicEnvironment, "AquaPicRuntimeProject");
                    if (!Directory.Exists (path)) {
                        Console.WriteLine ("Path to AquaPic directory environment is incorrect");
                        Console.WriteLine("Incorrect path was {0}", path);
                        aquaPicEnvironment = string.Empty;
                    }
                }

                if (aquaPicEnvironment.IsEmpty ()) {
                    Console.WriteLine ("Please edit the AquaPicEnvironment.txt file to point to the path of the AquaPicRuntime directory,");
                    Console.WriteLine ("not to include AquaPicRuntime. For example if the AquaPicRuntime directory is located at");
                    Console.WriteLine ("/home/user/AquaPicRuntime/, then add \"/home/user\" to the first line of the file.");
                    return;
                }
            } else {
                aquaPicEnvironment = Environment.GetEnvironmentVariable("HOME");
                aquaPicEnvironment = Path.Combine(aquaPicEnvironment, ".aquapic");
                Console.WriteLine("AquaPic Environment {0}", aquaPicEnvironment);
                if (!Directory.Exists(aquaPicEnvironment)) {
                    Directory.CreateDirectory(aquaPicEnvironment);
                }
                var path = Path.Combine(aquaPicEnvironment, "AquaPicRuntimeProject");
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                    Directory.CreateDirectory(path + "/Settings");
                    Directory.CreateDirectory(path + "/DataLogging");
                    Directory.CreateDirectory(path + "/Logs");
                    Directory.CreateDirectory(path + "/TestProcedures");
                }
            }

            //Setup
            Utils.AquaPicEnvironment = aquaPicEnvironment;
            Application.Init ();
            Logger.Add ("Executing operating system is {0}", Utils.GetDescription (Utils.ExecutingOperatingSystem));

#if DEBUG
            try {
#endif
                Hardware.AddFromJson ();
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
            var win = AquaPicGui.CreateInstance ();
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
            groups = WaterLevel.GetAllWaterLevelGroupNames ();
            foreach (var group in groups) {
                WaterLevel.GetWaterLevelGroupDataLogger (group).DeleteAllLogFiles ();
            }
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
