using System;
using System.Reflection;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace AquaPic.PluginRuntime
{
    public class Plugin
    {
        private Assembly pluginAssembly;
        private string name;
        private string sourceCodeFile;
        private bool compiled;

        public string Name {
            get { return name; }
        }

        public string SourceCodeFile {
            get { return sourceCodeFile; }
            set {
                compiled = false;
                sourceCodeFile = value;
            }
        }

        public bool DefaultReturn;

        public Plugin ( string name, string sourceCodeFile) {
            this.name = name;
            this.sourceCodeFile = sourceCodeFile;
            this.DefaultReturn = false;

            try {
                if (Directory.Exists (this.name + ".dll")) {
                    this.pluginAssembly = Assembly.LoadFrom (this.name + ".dll");
                    compiled = true;
                    Console.WriteLine ("Loaded from file");
                } else {
                    Console.WriteLine ("Compiling");
                    compiled = CompileCode ();
                }
            } catch {
                Console.WriteLine ("failed compiling");
                compiled = false;
            }
        }

        public bool RunPluginCoil () {
            if (compiled) {
                //try {
                    Type type = pluginAssembly.GetType ("MyScript.ScriptCoil");
                    MethodInfo method = type.GetMethod ("CoilCondition");
                    object rtn = method.Invoke (null, null);
                    bool b = Convert.ToBoolean (rtn);
                    return b;
                //} catch {
                //    return false;
                //}
            } else
                return DefaultReturn;
        }

        protected bool CompileCode () {
            CSharpCodeProvider provider = new CSharpCodeProvider ();
            CompilerParameters options = new CompilerParameters();

            options.GenerateExecutable = false; // create dll
            options.OutputAssembly = name + ".dll";
            options.GenerateInMemory = false;
            options.ReferencedAssemblies.Add (Assembly.GetExecutingAssembly ().Location);
           
            CompilerResults result = provider.CompileAssemblyFromFile(options, sourceCodeFile);

            if (result.Errors.HasErrors) {
                foreach (CompilerError error in result.Errors)
                    Console.WriteLine ("Error ({0}): {1}", error.ErrorNumber, error.ErrorText);

                pluginAssembly =  null;
                return false;
            }

            pluginAssembly = Assembly.LoadFrom (name + ".dll");
            return true;
        }
    }
}

