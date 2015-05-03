using System;
using System.Reflection;

namespace AquaPic.PluginRuntime
{
    public class PluginType
    {
        protected Assembly pluginAssembly;
        protected string name;
        protected string path;
        protected bool compiled;

        public string Name {
            get { return name; }
        }

        public string SourcePath {
            get { return path; }
            set {
                compiled = false;
                path = value;
            }
        }

        public PluginType (string name, string path) {
            this.name = name;
            this.path = path;

            try {
                //Console.WriteLine ("Compiling " + this.name);
                pluginAssembly = Plugin.CompileCode (ref compiled, this.name, this.path);
            } catch {
                //Console.WriteLine ("failed compiling " + this.name);
                compiled = false;
            }
        }

        public void RunInitialize (string typeName) {
            if (compiled) {
                Type type = pluginAssembly.GetType (typeName);

                MethodInfo method = null;
                if (type != null)
                    method = type.GetMethod ("Initialize");

                if (method != null)
                    method.Invoke (null, null);
            }
        }

        public void RunPlugin () {
            if (compiled) {
                Type type = pluginAssembly.GetType ("ScriptingInterface.PluginScript");

                MethodInfo method = null;
                if (type != null)
                    method = type.GetMethod ("RuntimePlugin");

                if (method != null)
                    method.Invoke (null, null);
            }
        }
    }
}

