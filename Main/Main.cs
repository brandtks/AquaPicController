#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

#define DELETE_DATA_FILES

using System;
using System.IO;
using Gtk;
using GoodtimeDevelopment.Utilites;
using AquaPic.DataLogging;
using AquaPic.Service;
using AquaPic.UserInterface;
using AquaPic.Drivers;
using AquaPic.Modules;
using AquaPic.Modules.Temperature;
using AquaPic.SerialBus;
using AquaPic.Globals;
using AquaPic.Gadgets.Sensor;
using AquaPic.Gadgets.Device;

namespace AquaPic
{
    class MainClass
    {
        public static void Main (string[] args) {
            var aquaPicEnvironment = string.Empty;
            var aquaPicSettings = string.Empty;
            // Call the Gtk library hack because Windows sucks at everything
            if (Utils.ExecutingOperatingSystem == Platform.Windows) {
                CheckWindowsGtk ();

                //Get the AquaPic directory environment
                aquaPicEnvironment = Environment.GetEnvironmentVariable ("AquaPic");
                if (aquaPicEnvironment.IsEmpty ()) {
                    var path = Path.GetDirectoryName (System.Reflection.Assembly.GetEntryAssembly ().Location);
                    path = Path.Combine (path, "AquaPicEnvironment.txt");
                    if (File.Exists (path)) {
                        var lines = File.ReadAllLines (path);
                        aquaPicEnvironment = lines[0];
                    }
                }

                if (aquaPicEnvironment.IsNotEmpty ()) {
                    var path = aquaPicEnvironment;
                    if (!Directory.Exists (path)) {
                        Console.WriteLine ("Path to AquaPic directory environment is incorrect");
                        Console.WriteLine ("Incorrect path was {0}", path);
                        aquaPicEnvironment = string.Empty;
                    }
                }

                if (aquaPicEnvironment.IsEmpty ()) {
                    Console.WriteLine ("Please edit the AquaPicEnvironment.txt file to point to the path of the AquaPicRuntime directory.");
                    Console.WriteLine ("For example if the AquaPicRuntime directory is located at /home/user/AquaPicRuntimeProject/,");
                    Console.WriteLine ("then add \"/home/user/AquaPicRuntimeProject/\" to the first line of the file.");
                    return;
                }
            } else {
                aquaPicEnvironment = Environment.GetEnvironmentVariable ("HOME");
                aquaPicEnvironment = Path.Combine (aquaPicEnvironment, ".config");
                aquaPicEnvironment = Path.Combine (aquaPicEnvironment, "AquaPic");

                if (!Directory.Exists (aquaPicEnvironment)) {
                    Directory.CreateDirectory (aquaPicEnvironment);
                    Directory.CreateDirectory (Path.Combine (aquaPicEnvironment, "DataLogging"));
                    Directory.CreateDirectory (Path.Combine (aquaPicEnvironment, "Logs"));
                    Directory.CreateDirectory (Path.Combine (aquaPicEnvironment, "TestProcedures"));
                }
            }

            if (args.Length > 0) {
                Console.WriteLine ("Arguments {0}", args[0]);
                if (args[0].Contains ("-empty")) {
                    aquaPicSettings = Path.Combine (aquaPicEnvironment, "Settings.Empty");
                } else if (args[0].Contains ("-full")) {
                    aquaPicSettings = Path.Combine (aquaPicEnvironment, "Settings.Full");
                } else {
                    aquaPicSettings = Path.Combine (aquaPicEnvironment, "Settings");
                }
            } else {
                aquaPicSettings = Path.Combine (aquaPicEnvironment, "Settings");
            }

            if (!Directory.Exists (aquaPicSettings)) {
                Directory.CreateDirectory (aquaPicSettings);
            }

            //Setup
            Utils.AquaPicEnvironment = aquaPicEnvironment;
            Utils.AquaPicSettings = aquaPicSettings;
            Application.Init ();
            Logger.Add ("Executing operating system is {0}", Utils.GetDescription (Utils.ExecutingOperatingSystem));

#if DEBUG
            try {
#endif
                Driver.AddFromJson ();
                Sensors.AddSensors ();
                Devices.AddDevices ();
                Temperature.Init ();
                WaterLevel.Init ();
                AutoTopOff.Init ();
#if DEBUG
            } catch (Exception ex) {
                Logger.AddError (ex.ToString ());
                return;
            }
#endif

#if DEBUG
            Driver.AnalogInput.SetChannelMode ("Right Overflow, Temperature Probe", Mode.Manual);
            Driver.AnalogInput.SetChannelValue ("Right Overflow, Temperature Probe", 2796.5);
            Driver.AnalogInput.SetChannelMode ("Left Overflow, Temperature Probe", Mode.Manual);
            Driver.AnalogInput.SetChannelValue ("Left Overflow, Temperature Probe", 2796.5);
            Driver.AnalogInput.SetChannelMode ("Sump, Temperature Probe", Mode.Manual);
            Driver.AnalogInput.SetChannelValue ("Sump, Temperature Probe", 2796.5);
            Driver.AnalogInput.SetChannelMode ("Salt Mixing, Temperature Probe", Mode.Manual);
            Driver.AnalogInput.SetChannelValue ("Salt Mixing, Temperature Probe", 2796.5);

            Driver.AnalogInput.SetChannelMode ("Return Chamber, Water Level Sensor", Mode.Manual);
            Driver.AnalogInput.SetChannelValue ("Return Chamber, Water Level Sensor", 1803);
            Driver.AnalogInput.SetChannelMode ("ATO Reservoir, Water Level Sensor", Mode.Manual);
            Driver.AnalogInput.SetChannelValue ("ATO Reservoir, Water Level Sensor", 3878);
            Driver.AnalogInput.SetChannelMode ("Salt Reservoir, Water Level Sensor", Mode.Manual);
            Driver.AnalogInput.SetChannelValue ("Salt Reservoir, Water Level Sensor", 2048);

            Driver.DigitalInput.SetChannelMode ("Sump High, Float Switch", Mode.Manual);
            Driver.DigitalInput.SetChannelValue ("Sump High, Float Switch", true);
            Driver.DigitalInput.SetChannelMode ("Sump Low, Float Switch", Mode.Manual);
            Driver.DigitalInput.SetChannelValue ("Sump Low, Float Switch", true);
            Driver.DigitalInput.SetChannelMode ("Sump ATO, Float Switch", Mode.Manual);

            Driver.PhOrp.SetChannelMode ("Sump, pH Probe", Mode.Manual);
            Driver.PhOrp.SetChannelValue ("Sump, pH Probe", 2282);
#endif

            // Run the control
            var win = AquaPicGui.CreateInstance ();
            win.Show ();

            Application.Run ();

            // Cleanup
            if (AquaPicBus.isOpen) {
                AquaPicBus.Close ();
            }

#if DELETE_DATA_FILES
            // For some reason this doesn't like to be in the destroyed event
            var groups = Temperature.GetAllTemperatureGroupNames ();
            foreach (var group in groups) {
                var dataLogger = (DataLoggerIoImplementation)Temperature.GetTemperatureGroupDataLogger (group);
                if (dataLogger != null) {
                    dataLogger.DeleteAllLogFiles ();
                }
            }
            groups = WaterLevel.GetAllWaterLevelGroupNames ();
            foreach (var group in groups) {
                var dataLogger = (DataLoggerIoImplementation)WaterLevel.GetWaterLevelGroupDataLogger (group);
                if (dataLogger != null) {
                    dataLogger.DeleteAllLogFiles ();
                }
            }
            var phProbes = Sensors.PhProbes.GetAllGadgetNames ();
            foreach (var probe in phProbes) {
                var dataLogger = (DataLoggerIoImplementation)Sensors.PhProbes.GetDataLogger (probe);
                if (dataLogger != null) {
                    dataLogger.DeleteAllLogFiles ();
                }
            }
#endif
        }

        // Gtk library hack because Windows
        [System.Runtime.InteropServices.DllImport ("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs (System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool SetDllDirectory (string lpPathName);

        static bool CheckWindowsGtk () {
            string location = null;
            Version version = null;
            Version minVersion = new Version (2, 12, 30);

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey (@"SOFTWARE\Xamarin\GtkSharp\InstallFolder")) {
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

