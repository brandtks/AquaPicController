using System;
using System.Collections.Generic;

namespace AquaPic.Runtime
{
    public delegate bool ConditionCheckHandler ();
    public delegate void OutputHandler ();

    public class Coil
    {
        public ConditionCheckHandler ConditionChecker;
        public OutputHandler OutputTrue;
        public OutputHandler OutputFalse;
        public bool state;

        public Coil () {
            state = false;
        }

        public void Execute () {
            if (ConditionChecker != null)       // do we have a condition check method
                state = ConditionChecker ();    // yes, lets run it to find out the new state

            if (state) {                        // if state is true
                if (OutputTrue != null)         // do we have a method to run if the state is true     
                OutputTrue ();              // yes, lets run it
            } else {
                if (OutputFalse != null)        // do we have a method to run if the state is false
                OutputFalse ();                 // yes, lets run it
            }
        }
    }
}

