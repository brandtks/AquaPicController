using System;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace AquaPic.PluginRuntime
{
    public class Plugin
    {
        private Assembly pluginAssembly;
        private string name;
        private string sourceCode;

        public string Name {
            get { return name; }
        }

        public string SourceCode {
            get { return sourceCode; }
            set {
                sourceCode = value;
            }
        }

        public Plugin (string sourceCode, string name) {
            this.sourceCode = sourceCode;
            this.name = name;

            try {
                this.pluginAssembly = Assembly.LoadFrom (this.name);
            } catch {
                this.pluginAssembly = CompileCode ();
            }
        }

        public bool RunPluginCoil () {
            try {
                Type type = pluginAssembly.GetType ("MyScript.ScriptCoil");
                MethodInfo method = type.GetMethod ("CoilCondition");
                object rtn = method.Invoke (null, null);
                return Convert.ToBoolean (rtn);
            } catch {
                return false;
            }
        }

        public Assembly CompileCode () {
            CSharpCodeProvider provider = new CSharpCodeProvider ();
            CompilerParameters options = new CompilerParameters();

            options.GenerateExecutable = false; // create dll
            options.OutputAssembly = name;
            options.GenerateInMemory = false;
            options.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
           
            CompilerResults result = provider.CompileAssemblyFromSource(options, sourceCode);

            if (result.Errors.HasErrors) {
                foreach (CompilerError error in result.Errors)
                    Console.WriteLine ("Error ({0}): {1}", error.ErrorNumber, error.ErrorText);

                return null;
            }

            return result.CompiledAssembly;
        }
    }
}

