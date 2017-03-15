using System;

namespace AquaPic.Operands
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
            state = (bool)ConditionChecker?.Invoke ();  // do we have a condition check method, if yes, lets run it to find out the new state

            if (state) {                        // if state is true
                OutputTrue?.Invoke ();          // do we have a method to run if the state is true, if yes, run it
            } else {
                OutputFalse?.Invoke ();         // do we have a method to run if the state is false, if yes, run it
            }
        }
    }
}

