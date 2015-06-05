using System;
using System.Reflection;
using System.Text;

namespace AquaPic.Runtime
{
    public class BaseScript
    {
        //protected Assembly pluginAssembly;
        protected IScript instance;
        public ScriptFlags flags;
        public string name;
        public string path;
        
        public BaseScript (string name, string path) {
            this.name = name;
            this.path = path;
            flags = ScriptFlags.None;
            instance = null;

            Assembly pluginAssembly = null;
            try {
                //Console.WriteLine ("Compiling " + this.name);
                if (Plugin.CompileCode (this.name, this.path)) {
                    StringBuilder sb = new StringBuilder ();
                    sb.Append (Environment.GetEnvironmentVariable ("AquaPic"));
                    sb.Append (@"\AquaPicRuntimeProject\Scripts\dll\");
                    sb.Append (name);
                    sb.Append (".dll");
                    //pluginAssembly = Assembly.LoadFrom (name + ".dll");
                    pluginAssembly = Assembly.LoadFrom (sb.ToString ());
                    flags |= ScriptFlags.Compiled;
                }
            } catch {
                //Console.WriteLine ("failed compiling " + this.name);
                flags &= ~ScriptFlags.Compiled;
            }
            if (pluginAssembly != null)
                CreateInstance (pluginAssembly);
        }

        protected virtual void CreateInstance (Assembly pluginAssembly) {
            if (flags.HasFlag (ScriptFlags.Compiled)) {
                foreach (var t in pluginAssembly.GetTypes ()) {
                    if (t.GetInterface ("IScript") != null) {
                        try {
                            instance = Activator.CreateInstance (t) as IScript;

                            if (instance == null)
                                flags &= ~ScriptFlags.Compiled;
                        } catch {
                            flags &= ~ScriptFlags.Compiled;
                        }
                    }
                }
            }
        }

        public void RunInitialize () {
            if (flags.HasFlag (ScriptFlags.Initializer | ScriptFlags.Compiled)) {
                try {
                    IStartupScript i = instance as IStartupScript;
                    i.Initialize ();
                } catch {
                    flags &= ~ScriptFlags.Compiled;
                }
            }
        }

        public virtual void RunPlugin (ScriptFlags flag) {
            if (flags.HasFlag (ScriptFlags.Compiled | flag)) {
                try {
                    var i = instance as ICyclicScript;
                    i.RunScript ();
                } catch {
//                    if (!flags.HasFlag (ScriptFlags.Outlet)) // if its not an outlet script mark plugin as bad
                        flags &= ~ScriptFlags.Compiled;
                }
            }
        }
    }
}

