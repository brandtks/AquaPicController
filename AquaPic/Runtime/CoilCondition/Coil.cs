using System;
using System.Collections.Generic;

namespace AquaPic.CoilCondition
{
    public class Coil : Condition
    {
        //public List<Condition> Conditions;
        public ConditionScript Conditions;
        public event OutputHandler OutputTrue;
        public event OutputHandler OutputFalse;

        public Coil (string name) : base (name) {
            //Conditions = new List<Condition> ();
            Conditions = new ConditionScript ();
        }

        public void Execute () {
//            bool state = true;
//            for (int i = 0; i < Conditions.Count; ++i) {
//                state &= Conditions [i].CheckState ();
//            }

            State = Conditions.EvaluateScript ();

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

