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

            public DigitalInputInput (string name) {
                this.state = false;
                this.name = name;
            }
        }
    }
}

