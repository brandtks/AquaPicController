using System;

namespace AquaPic.PluginRuntime
{
    [Flags]
    public enum ScriptFlags {
        None = 0,
        Compiled = 1,
        Cyclic = 2,
        Outlet = 4,
        Event = 8,
        Initializer = 16
    }

    public interface IOutletScript : IStartupScript
    {
        bool OutletCondition ();
    }

    public interface ICyclicScript : IStartupScript, IEventScript
    { }

    public interface IStartupScript : IScript
    {
        void Initialize ();
    }

    public interface IEventScript : IScript
    {
        void RunScript ();
    }

    public interface IScript
    { }
}