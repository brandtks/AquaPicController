using System;

namespace ScriptingInterface
{
    public interface OutletPluginScript
    {
        void Initialize ();
        bool OutletCondition ();
    }

    public interface PluginScript
    {
        void Initialize ();
        void RuntimePlugin ();
    }
}