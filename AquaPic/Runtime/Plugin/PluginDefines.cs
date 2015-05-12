using System;

namespace AquaPic.PluginRuntime
{
    [Flags]
    public enum PluginFlags {
        None = 0,
        Compiled = 1,
        Cyclic = 2,
        Outlet = 4,
        Initializer = 8,
        OneShot = 16
    }

    public interface IOutletScript : IPluginScript
    {
        bool OutletCondition ();
    }

    public interface IPluginScript
    {
        void Initialize ();
        void RunScript ();
    }
}