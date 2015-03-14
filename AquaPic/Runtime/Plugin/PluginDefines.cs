using System;

namespace ScriptingInterface
{
    public interface ScriptCoil
    {
        bool CoilCondition ();
    }

    public interface ScriptValue
    {
        double ValueCondition ();
    }
}