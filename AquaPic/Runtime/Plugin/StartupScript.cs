/***************************************************************************************************/
/*        NOT COMPILED                                                                             */
/***************************************************************************************************/

using System;
using System.IO;
using System.Reflection;

namespace AquaPic.Runtime
{
    public class StartupScript : BaseScript
    {
        public StartupScript (string name, string path) : base (name, path) {
            flags |= ScriptFlags.Initializer;
        }

        protected override void CreateInstance (Assembly pluginAssembly) {
            if (flags.HasFlag (ScriptFlags.Compiled)) {
                foreach (var t in pluginAssembly.GetTypes ()) {
                    if (t.GetInterface ("IStartupScript") != null) {
                        try {
                            instance = Activator.CreateInstance (t) as IStartupScript;

                            if (instance == null)
                                flags &= ~ScriptFlags.Compiled;
                        } catch (Exception ex) {
                            flags &= ~ScriptFlags.Compiled;
                            errors.Add (new ScriptMessage ("StartupScript Constructor", "  " + ex.ToString ()));
                        }
                    }
                }
            }
        }
    }
}

