using System;
using System.Reflection;

namespace AquaPic.PluginRuntime
{
    public class OutletPlugin : PluginType
    {
        public bool DefaultReturn;

        public OutletPlugin (string name, string path) : base (name, path) {
            this.DefaultReturn = false;
        }

        public bool RunOutletCondition () {
            if (compiled) {
                Type type = pluginAssembly.GetType ("ScriptingInterface.OutletPluginScript");
                MethodInfo method = type.GetMethod ("OutletCondition");
                object rtn = method.Invoke (null, null);
                return Convert.ToBoolean (rtn);
            } else
                return DefaultReturn;
        }
    }
}

