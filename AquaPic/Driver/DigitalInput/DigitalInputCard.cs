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
                #if UNSAFE_COMMS
                unsafe {
                    slave.Read (20, sizeof(byte), GetInputsCallback);
                }
                #else
                slave.Read (20, sizeof(byte), GetInputsCallback);
                #endif
            }

            protected void GetInputsCallback (CallbackArgs args) {
                byte stateMask;

                #if UNSAFE_COMMS
                unsafe {
                    args.CopyBuffer (&stateMask, sizeof(byte));
                }
                #else
                stateMask = args.GetDataFromReadBuffer<byte> (0);
                #endif

                for (int i = 0; i < inputs.Length; ++i)
                    inputs [i].state = Utils.mtob (stateMask, i);
            }

            public void GetInput (int ch) {
                updating = true;
                #if UNSAFE_COMMS
                byte message = ch;

                unsafe {
                    slave.ReadWrite (10, &message, sizeof(byte), sizeof(CommValueBool), GetInputCallback);
                }
                #else
                slave.ReadWrite (10, (byte)ch, 2, GetInputCallback);
                #endif
            }

            protected void GetInputCallback (CallbackArgs args) {
                #if UNSAFE_COMMS
                CommValueBool vg;

                unsafe {
                    args.CopyBuffer (&vg, sizeof(CommValueBool));
                }

                if (inputs [vg.channel].state != vg.value) {
                    //do event
                }

                inputs [vg.channel].state = vg.value;
                #else
                byte ch = args.GetDataFromReadBuffer<byte> (0);
                bool value = args.GetDataFromReadBuffer<bool> (1);

                if (inputs [ch].state != value) {
                    //do event
                }

                inputs [ch].state = value;
                #endif

                updating = false;
            }
        }
    }
}

