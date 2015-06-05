using System;
using System.IO;
using System.Reflection;

namespace AquaPic.Runtime
{
    public class StartupScript : BaseScript
    {
        public StartupScript (string name, string path) : base (name, path) {
            flags |= ScriptFlags.Initializer | ScriptFlags.Cyclic;
        }

        protected override void CreateInstance (Assembly pluginAssembly) {
            if (flags.HasFlag (ScriptFlags.Compiled)) {
                foreach (var t in pluginAssembly.GetTypes ()) {
                    if (t.GetInterface ("IStartupScript") != null) {
                        try {
                            instance = Activator.CreateInstance (t) as IStartupScript;

                            if (instance != null) {
                                IStartupScript i = instance as IStartupScript;
                                i.Initialize ();
                            } else
                                flags &= ~ScriptFlags.Compiled;
                        } catch (Exception ex) {
                            flags &= ~ScriptFlags.Compiled;
                            Console.WriteLine (ex.Message);
                            Console.WriteLine (ex.ToString ());
                        }
                    }
                }
                //<TODO> figure out how to delete the assembly file
                //int idx = path.LastIndexOf ('\\') + 1;
                //string newPath = path.Substring (0, idx) + name + ".dll";
                //File.Delete (newPath);
            }
        }
    }
}

