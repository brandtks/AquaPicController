using System;
using System.Reflection;

namespace AquaPic.PluginRuntime
{
    public class ValuePlugin : PluginType
    {
        public double DefualtValue;

        public ValuePlugin (string name, string path) : base (name, path) {
            this.DefualtValue = 0.0;
        }

        public double RunValueCondition () {
            if (compiled) {
                Type type = pluginAssembly.GetType ("ScriptingInterface.ValuePluginScript");
                MethodInfo method = type.GetMethod ("ValueCondition");
                object rtn = method.Invoke (null, null);
                return Convert.ToDouble (rtn);
            } else
                return DefualtValue;
        }
    }
}

