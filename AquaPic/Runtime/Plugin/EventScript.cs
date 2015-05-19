using System;
using System.Reflection;

namespace AquaPic.PluginRuntime
{
    public class EventScript : BaseScript
    {
        public EventScript (string name, string path) : base (name, path) {
            flags |= ScriptFlags.Event;
        }

        protected override void CreateInstance (Assembly pluginAssembly) {
            if (flags.HasFlag (ScriptFlags.Compiled)) {
                foreach (var t in pluginAssembly.GetTypes ()) {
                    if (t.GetInterface ("IEventScript") != null) {
                        try {
                            instance = Activator.CreateInstance (t) as IEventScript;

                            if (instance == null)
                                flags &= ~ScriptFlags.Compiled;
                        } catch {
                            flags &= ~ScriptFlags.Compiled;
                        }
                    }
                }
            }
        }
    }
}

