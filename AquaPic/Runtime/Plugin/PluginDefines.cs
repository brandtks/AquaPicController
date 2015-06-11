using System;

namespace AquaPic.Runtime
{
    [Flags]
    public enum ScriptFlags {
        None = 0,
        Compiled = 1,
        Cyclic = 2,
        Event = 4,
        Initializer = 8
    }

    public interface ICyclicScript : IStartupScript
    { 
        void CyclicRun ();
    }

    public interface IStartupScript : IScript
    {
        void Initialize ();
    }

    public interface IEventScript : IScript
    {
        void OneShotRun ();
    }

    public interface IScript
    { }
}