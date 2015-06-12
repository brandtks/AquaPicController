using System;
using System.Reflection;

namespace AquaPic.Runtime
{
    public class ModuleScript : BaseScript
    {
        public ModuleScript (string name, string path) : base (name, path) {
            flags |= (ScriptFlags.Initializer | ScriptFlags.Cyclic | ScriptFlags.Event);
        }

        protected override void CreateInstance (Assembly pluginAssembly) {
            if (flags.HasFlag (ScriptFlags.Compiled)) {
                foreach (var t in pluginAssembly.GetTypes ()) {
                    if (t.GetInterface ("IModuleScript") != null) {
                        try {
                            instance = Activator.CreateInstance (t) as IModuleScript;

                            if (instance == null)
                                flags &= ~ScriptFlags.Compiled;
                        } catch (Exception ex) {
                            flags &= ~ScriptFlags.Compiled;
                            errors.Add (new ScriptMessage ("ModuleScript Constructor", "  " + ex.ToString ()));
                        }
                    }
                }
            }
        }
    }
}

