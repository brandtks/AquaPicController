using System;
using System.Reflection;

namespace AquaPic.Runtime
{
    public class CyclicScript : BaseScript
    {
        public CyclicScript (string name, string path) : base (name, path) {
            flags |= ScriptFlags.Initializer | ScriptFlags.Cyclic;
        }

        protected override void CreateInstance (Assembly pluginAssembly) {
            if (flags.HasFlag (ScriptFlags.Compiled)) {
                foreach (var t in pluginAssembly.GetTypes ()) {
                    if (t.GetInterface ("ICyclicScript") != null) {
                        try {
                            instance = Activator.CreateInstance (t) as ICyclicScript;

                            if (instance == null)
                                flags &= ~ScriptFlags.Compiled;
                        } catch (Exception ex) {
                            flags &= ~ScriptFlags.Compiled;
                            errors.Add (new ScriptMessage ("CyclicScript Constructor", "  " + ex.ToString ()));
                        }
                    }
                }
            }
        }
    }
}

