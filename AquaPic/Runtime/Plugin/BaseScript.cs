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
                if (Plugin.CompileCode (this)) {
                    StringBuilder sb = new StringBuilder ();
                    sb.Append (Environment.GetEnvironmentVariable ("AquaPic"));
                    sb.Append (@"\AquaPicRuntimeProject\Scripts\dll\");
                    sb.Append (name);
                    sb.Append (".dll");
                    pluginAssembly = Assembly.LoadFrom (sb.ToString ());
                    flags |= ScriptFlags.Compiled;
                }
            } catch (Exception ex) {
                flags &= ~ScriptFlags.Compiled;
                Console.WriteLine ("{0} compiled flag was just cleared at {1}", name, "constructor");
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
                        } catch (Exception ex) {
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
                } catch (Exception ex) {
                    flags &= ~ScriptFlags.Compiled;
                }
            }
        }

        public virtual void RunPlugin (ScriptFlags flag) {
            if (flags.HasFlag (ScriptFlags.Compiled | flag)) {
                try {
                    var i = instance as ICyclicScript;
                    i.RunScript ();
                } catch (Exception ex) {
                    flags &= ~ScriptFlags.Compiled;
                }
            }
        }
    }
}

