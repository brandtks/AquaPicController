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
                    if (line.Contains ("ICyclicScript")) {    
                        AllPlugins.Add (name, new CyclicScript (name, path));
                        AllPlugins [name].RunInitialize ();
                    } else if (line.Contains ("IStartupScript")) {
                        AllPlugins.Add (name, new StartupScript (name, path));
                        AllPlugins [name].RunInitialize ();
                    } else if (line.Contains ("IEventScript")) {
                        AllPlugins.Add (name, new EventScript (name, path));
                    }
                }
            }
        }

        public static void Run () {
            foreach (var p in AllPlugins.Values) {
                if (p.flags.HasFlag (ScriptFlags.Cyclic)) {
                    p.RunPlugin (ScriptFlags.Cyclic);
                }
            }
        }

        //public static bool CompileCode (string scriptName, string filePath) {
        public static bool CompileCode (BaseScript script) {
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

            CompilerResults result = provider.CompileAssemblyFromFile (options, script.path);

            if (result.Errors.HasErrors) {
                foreach (CompilerError error in result.Errors) {
                    Console.WriteLine ("Error ({0}): {1}", error.ErrorNumber, error.ErrorText);
                    Console.WriteLine ("Line {0}, Column {1}", error.Line, error.Column);
                }
                
                return false;
            }

            return true;
        }
    }
}

