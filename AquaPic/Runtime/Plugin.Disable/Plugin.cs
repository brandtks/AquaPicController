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

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace AquaPic.Runtime
{
    public class Plugin
    {
        public static Dictionary<string, BaseScript> AllPlugins = new Dictionary<string, BaseScript> ();
        private static int alarm;

        static Plugin () {
            TaskManager.AddCyclicInterrupt ("Plugin", 1000, Run);
            alarm = Alarm.Subscribe ("Plugin Failed");
        }

        public static void AddPlugins () {
            var topPath = Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            topPath = Path.Combine (topPath, "Scripts");

            var files = Directory.GetFiles (topPath, "*.cs");

            foreach (var path in files) {
                string name = string.Empty;
                var idxBackslash = path.LastIndexOf ('\\') + 1;
                var idxPeriod = path.LastIndexOf ('.');
                if ((idxBackslash != -1) && (idxPeriod != -1))
                    name = path.Substring (idxBackslash, (idxPeriod - idxBackslash));

                foreach (var line in File.ReadLines (path)) {
                    if (line.Contains ("[AquaPicScript (\"ICyclicScript\")]")) {
                        AllPlugins.Add (name, new CyclicScript (name, path));
                        AllPlugins [name].RunInitialize ();
                    } else if (line.Contains ("[AquaPicScript (\"IStartupScript\")]")) {
                        AllPlugins.Add (name, new StartupScript (name, path));
                        AllPlugins [name].RunInitialize ();
                    } else if (line.Contains ("[AquaPicScript (\"IModuleScript\")]")) {
                        AllPlugins.Add (name, new ModuleScript (name, path));
                        AllPlugins [name].RunInitialize ();
                    } else if (line.Contains ("[AquaPicScript (\"IEventScript\")]")) {
                        AllPlugins.Add (name, new EventScript (name, path));
                    }
                }
            }
        }

        public static void Run () {
            bool atleastOnePluginFailed = false;

            foreach (var p in AllPlugins.Values) {
                if (!p.flags.HasFlag (ScriptFlags.Compiled))
                    atleastOnePluginFailed = true;

                if (p.flags.HasFlag (ScriptFlags.Cyclic)) {
                    p.CyclicRun ();
                }
            }

            if (atleastOnePluginFailed)
                Alarm.Post (alarm);
            else if (Alarm.CheckAlarming (alarm))
                Alarm.Clear (alarm); 
        }

        public static bool CompileCode (BaseScript script) {
            script.errors.Clear ();

            // <WINDOWS> CSharpCodeProvider does not work with Mono, does work in the mono framework on windows, linux not tested
            CSharpCodeProvider provider = new CSharpCodeProvider ();
            CompilerParameters options = new CompilerParameters();

            options.GenerateExecutable = false; // create dll

            var dllPath = Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            dllPath = Path.Combine (dllPath, "Scripts");
            dllPath = Path.Combine (dllPath, "dll");
            dllPath = Path.Combine (dllPath, script.name + ".dll");

            options.OutputAssembly = dllPath;

            options.ReferencedAssemblies.Add (Assembly.GetExecutingAssembly ().Location);

            AssemblyName[] aNames = Assembly.GetExecutingAssembly ().GetReferencedAssemblies ();
            foreach (var aName in aNames) {
                if (aName.Name != "Mono.Cairo") { // for whatever reason Mono.Cairo done screws up ReflectionOnlyLoad
                    string path = Assembly.ReflectionOnlyLoad (aName.FullName).Location;
                    options.ReferencedAssemblies.Add (path);
                }
            }

            // for whatever reason Mono.Cairo done screws up ReflectionOnlyLoad and 
            // atk-sharp isn't in the Referenced Assemblies
            options.ReferencedAssemblies.Add (@"C:\Windows\Microsoft.Net\assembly\GAC_MSIL\Mono.Cairo\v4.0_4.0.0.0__0738eb9f132ed756\Mono.Cairo.dll");
            options.ReferencedAssemblies.Add (@"C:\Windows\Microsoft.Net\assembly\GAC_MSIL\atk-sharp\v4.0_2.12.0.0__35e10195dab3c99f\atk-sharp.dll");

            CompilerResults result = provider.CompileAssemblyFromFile (options, script.path);

            if (result.Errors.HasErrors) {
                foreach (CompilerError error in result.Errors) {
                    //Console.WriteLine ("Error ({0}): {1}", error.ErrorNumber, error.ErrorText);
                    //Console.WriteLine ("At Line {0}, Column {1}", error.Line, error.Column);

                    StringBuilder e = new StringBuilder ();
                    e.AppendLine (string.Format ("  Error ({0}): {1} ", error.ErrorNumber, error.ErrorText));
                    e.Append (string.Format ("  File {0}, Line {1} ", error.FileName, error.Line));

                    script.errors.Add (new ScriptMessage ("CompileCode", e.ToString ()));
                }
                
                return false;
            }

            return true;
        }
    }
}

