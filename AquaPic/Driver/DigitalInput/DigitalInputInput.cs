using System;

namespace AquaPic.Drivers
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

