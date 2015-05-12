using System;
using System.Reflection;

namespace AquaPic.PluginRuntime
{
    public class PluginScript
    {
        protected Assembly pluginAssembly;
        protected IPluginScript instance;

        public PluginFlags flags;
        public string name;
        public string path;

        public PluginScript (string name, string path) {
            this.name = name;
            this.path = path;

            flags = PluginFlags.Initializer | PluginFlags.Cyclic;

            try {
                //Console.WriteLine ("Compiling " + this.name);
                if (Plugin.CompileCode (out pluginAssembly, this.name, this.path))
                    flags |= PluginFlags.Compiled;
            } catch {
                //Console.WriteLine ("failed compiling " + this.name);
                flags &= ~PluginFlags.Compiled;
            }

            CreateInstance ();
        }

        protected virtual void CreateInstance () {
            foreach (var t in pluginAssembly.GetTypes ()) {
                if (t.GetInterface ("IPluginScript") != null) {
                    try {
                        instance = Activator.CreateInstance (t) as IPluginScript;

                        if (instance == null)
                            flags &= ~PluginFlags.Compiled;
                    } catch {
                        flags &= ~PluginFlags.Compiled;
                    }
                }
            }
        }

        public void RunInitialize () {
            if (flags.HasFlag (PluginFlags.Initializer | PluginFlags.Compiled)) {
                try {
                    instance.Initialize ();
                } catch {
                    flags &= ~PluginFlags.Compiled;
                }
            }
        }

        public void RunPlugin () {
            if (flags.HasFlag (PluginFlags.Compiled)) {
                try {
                    instance.RunScript ();
                } catch {
                    if (!flags.HasFlag (PluginFlags.Outlet)) // if its not an outlet script mark plugin as bad
                        flags &= ~PluginFlags.Compiled;
                }
            }
        }
    }
}

