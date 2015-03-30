using System;

namespace ScriptingInterface
{
    public interface OutletPlugin
    {
        void OutletInitialize ();
        bool OutletCondition ();
    }

    public interface ValuePlugin
    {
        double ValueCondition ();
    }
}