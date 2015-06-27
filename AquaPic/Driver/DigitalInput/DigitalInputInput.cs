using System;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public partial class DigitalInput
    {
        private class DigitalInputInput
        {
            public bool state;
            public string name;
            public Mode mode;

            public DigitalInputInput (string name) {
                this.state = false;
                this.name = name;
                mode = Mode.Auto;
            }
        }
    }
}

