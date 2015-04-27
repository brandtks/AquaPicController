using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace AquaPic.PluginRuntime
{
    public class Plugin
    {
        public static Dictionary<string, PluginType> AllPlugins = new Dictionary<string, PluginType> ();

        public static void AddPlugins () {
            StringBuilder sb = new StringBuilder ();
            sb.Append (Environment.GetEnvironmentVariable ("AquaPic"));
            sb.Append (@"\AquaPicRuntimeProject\");
            var topPath = sb.ToString ();
            var files = Directory.GetFiles (topPath, "*.cs");

            foreach (var path in files) {
                string name = string.Empty;
                var idxBackslash = path.LastIndexOf ('\\') + 1;
                var idxPeriod = path.LastIndexOf ('.');
                if ((idxBackslash != -1) && (idxPeriod != -1))
                    name = path.Substring (idxBackslash, (idxPeriod - idxBackslash));

                //Console.WriteLine ("{0} at file path {1}", name, path);

                foreach (var line in File.ReadLines (path)) {
                    if (line.Contains ("OutletPluginScript")) {
                        AllPlugins.Add (name, new OutletPlugin (name, path));
                        AllPlugins [name].RunInitialize ("ScriptingInterface.OutletPluginScript");
                        break;
                    } else if (line.Contains ("PluginScript")) {
                        AllPlugins.Add (name, new PluginType (name, path));
                        AllPlugins [name].RunInitialize ("ScriptingInterface.PluginScript");
                    }
                }
            }
        }

        public static void Run () {
            foreach (var p in AllPlugins.Values) {
                if (p is OutletPlugin) {

                } else {
                    p.RunPlugin ();
                }
            }
        }

        public static Assembly CompileCode (ref bool compiledOk, string name, string sourceFileLocation) {
            CSharpCodeProvider provider = new CSharpCodeProvider ();
            CompilerParameters options = new CompilerParameters();

            options.GenerateExecutable = false; // create dll
            options.OutputAssembly = name + ".dll";
            options.GenerateInMemory = false;
            options.ReferencedAssemblies.Add (Assembly.GetExecutingAssembly ().Location);

            CompilerResults result = provider.CompileAssemblyFromFile(options, sourceFileLocation);

            if (result.Errors.HasErrors) {
                foreach (CompilerError error in result.Errors)
                    Console.WriteLine ("Error ({0}): {1}", error.ErrorNumber, error.ErrorText);

                compiledOk = false;
                return null;
            }

            compiledOk = true;
            return Assembly.LoadFrom (name + ".dll");
        }
    }
}

