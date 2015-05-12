using System;
using System.Reflection;

namespace AquaPic.PluginRuntime
{
    public class OutletScript : PluginScript
    {
        public bool defaultReturn;

        public OutletScript (string name, string path) : base (name, path) {
            defaultReturn = false;
            flags |= PluginFlags.Outlet;
            flags &= ~PluginFlags.Cyclic;
        }

        protected override void CreateInstance ()
        {
            foreach (var t in pluginAssembly.GetTypes ()) {
                if (t.GetInterface ("IOutletScript") != null) {
                    try {
                        instance = Activator.CreateInstance (t) as IOutletScript;

                        if (instance == null)
                            flags &= ~PluginFlags.Compiled;
                    } catch {
                        flags &= ~PluginFlags.Compiled;
                    }
                }
            }
        }

        public bool RunOutletCondition () {
            if (flags.HasFlag (PluginFlags.Compiled | PluginFlags.Outlet)) {
                try {
                    var i = instance as IOutletScript;
                    return i.OutletCondition ();
                } catch {
                    flags &= ~PluginFlags.Compiled;
                    return defaultReturn;
                }
            } else
                return defaultReturn;
        }
    }
}

