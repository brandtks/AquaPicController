using System;
using AquaPic.Globals;

namespace AquaPic.CoilCondition
{
    public class Condition
    {
        public string Name;
        public bool State;

        public event ConditionCheckHandler CheckHandler;

        public Condition (string name) {
            this.Name = name;
            this.State = false;
            ConditionLocker.AddCondition (this);
        }

        public bool CheckState () {
            if (CheckHandler != null)
                State = CheckHandler ();

            return State;
        }
    }
}

