using System;
using AquaPic.CoilRuntime;

namespace AquaPic.DigitalInputDriver
{
    public partial class DigitalInput
    {
        private class DigitalInputInput
        {
            public bool state;
            public string name;
            //public Condition stateCondition;

            public DigitalInputInput (string name) {
                this.state = false;
                this.name = name;
                //this.stateCondition = new Condition (name);
                //this.stateCondition.CheckHandler += delegate() {
                   // return this.state;
                //};
            }
        }
    }
}

