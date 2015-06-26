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

    public class AquaPicScript : Attribute
    {
        public string scriptType;

        public AquaPicScript (string scriptType) {
            this.scriptType = scriptType;
        }
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
        void OneShotRun (ref object rtnValue);
    }

    public interface IModuleScript : ICyclicScript, IEventScript
    { }

    public interface IScript
    { }
}