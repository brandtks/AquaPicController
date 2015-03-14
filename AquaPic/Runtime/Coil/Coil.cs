using System;
using System.Collections.Generic;
using AquaPic.PluginRuntime;

namespace AquaPic.CoilRuntime
{
    public delegate bool ConditionCheckHandler ();
    public delegate void OutputHandler ();

    public class Coil
    {
        public ConditionCheckHandler ConditionChecker;
        public event OutputHandler OutputTrue;
        public event OutputHandler OutputFalse;
        public bool State;

        public Coil () {
            this.State = false;
        }

        public void Execute () {
            if (ConditionChecker != null)
                State = ConditionChecker ();

            if (State) {
                if (OutputTrue != null)
                    OutputTrue ();
            } else {
                if (OutputFalse != null)
                    OutputFalse ();
            }
        }
    }
}

