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

        static Plugin () {
            TaskManager.AddTask ("Plugin", 1000, Run);
        }

        public static void AddPlugins () {
            StringBuilder sb = new StringBuilder ();
            sb.Append (Environment.GetEnvironmentVariable ("AquaPic"));
            sb.Append (@"\AquaPicRuntimeProject\Scripts\");
            var topPath = sb.ToString ();
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
            foreach (var p in AllPlugins.Values) {
                if (p.flags.HasFlag (ScriptFlags.Cyclic)) {
                    p.CyclicRun ();
                }
            }
        }

        public static bool CompileCode (BaseScript script) {
            script.errors.Clear ();

            // <WINDOWS> CSharpCodeProvider does not work with Mono
            CSharpCodeProvider provider = new CSharpCodeProvider ();
            CompilerParameters options = new CompilerParameters();

            options.GenerateExecutable = false; // create dll
            StringBuilder sb = new StringBuilder ();
            sb.Append (Environment.GetEnvironmentVariable ("AquaPic"));
            sb.Append (@"\AquaPicRuntimeProject\Scripts\dll\");
            sb.Append (script.name);
            sb.Append (".dll");
            options.OutputAssembly = sb.ToString ();

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

