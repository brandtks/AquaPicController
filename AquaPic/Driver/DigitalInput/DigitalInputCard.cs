using System;
using AquaPic.SerialBus;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public partial class DigitalInput
    {
        private class DigitalInputCard
        {
            public AquaPicBus.Slave slave;
            public byte cardID;
            public string name;
            public DigitalInputInput[] inputs;
            public bool updating;

            public DigitalInputCard (byte address, byte cardID, string name) {
                this.slave = new AquaPicBus.Slave (address, name + " (Digital Input)");

                this.cardID = cardID;
                this.name = name;
                this.updating = false;

                int numberInputs = 6;
                this.inputs = new DigitalInputInput[numberInputs];
                for (int i = 0; i < numberInputs; ++i)
                    inputs [i] = new DigitalInputInput (this.name + ".i" + i.ToString ());
            }

            public void GetInputs () {
                slave.Read (20, sizeof(byte), GetInputsCallback);
            }

            protected void GetInputsCallback (CallbackArgs args) {
                byte stateMask;

                stateMask = args.GetDataFromReadBuffer<byte> (0);

                for (int i = 0; i < inputs.Length; ++i)
                    inputs [i].state = Utils.MaskToBoolean (stateMask, i);
            }

            public void GetInput (int ch) {
                updating = true;
                slave.ReadWrite (10, (byte)ch, 2, GetInputCallback);
            }

            protected void GetInputCallback (CallbackArgs args) {
                byte ch = args.GetDataFromReadBuffer<byte> (0);
                bool value = args.GetDataFromReadBuffer<bool> (1);

                if (inputs [ch].state != value) {
                    //do event
                }

                inputs [ch].state = value;

                updating = false;
            }
        }
    }
}

