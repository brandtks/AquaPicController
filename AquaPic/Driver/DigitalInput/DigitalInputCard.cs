using System;
using AquaPic.SerialBus;
using AquaPic.AlarmRuntime;
using AquaPic.Utilites;

namespace AquaPic.DigitalInputDriver
{
    public partial class DigitalInput
    {
        private class DigitalInputCard
        {
            public AquaPicBus.Slave slave;
            public byte cardID;
            public string name;
            public int communicationAlarmIndex;
            public DigitalInputInput[] inputs;
            public bool updating;

            public DigitalInputCard (byte address, byte cardID, string name) {
                this.slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address);
                this.slave.OnStatusUpdate += OnSlaveStatusUpdate;

                this.cardID = cardID;
                this.name = name;
                this.communicationAlarmIndex = Alarm.Subscribe (
                    this.slave.Address.ToString () + "commication fault",
                    this.name + "serial communications fault");
                this.updating = false;

                int numberInputs = 6;
                this.inputs = new DigitalInputInput[numberInputs];
                for (int i = 0; i < numberInputs; ++i)
                    inputs [i] = new DigitalInputInput (i.ToString() + " input on " + this.name);
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((slave.Status != AquaPicBusStatus.communicationSuccess) || (slave.Status != AquaPicBusStatus.communicationStart))
                    Alarm.Post (communicationAlarmIndex);
            }

            #if SIMULATION
            public unsafe void GetInputs () {
                slave.Read (20, 1, GetInputsCallback);
            }
            #else
            public unsafe void GetInputs () {
                slave.Read (20, sizeof(byte), GetInputsCallback);
            }
            #endif

            #if SIMULATION
            protected void GetInputsCallback (CallbackArgs args) {
                byte mask = Convert.ToByte(args.readMessage[3]);
                for (int i = 0; i < inputs.Length; ++i)
                    inputs [i].state = Utils.mtob (mask, i);
            }
            #else
            protected void GetInputsCallback (CallbackArgs args) {
                byte stateMask;

                unsafe {
                    args.copyBuffer (&stateMask, sizeof(byte));
                }

                for (int i = 0; i < inputs.Length; ++i)
                    inputs [i].state = Utils.mtob (stateMask, i);
            }
            #endif

            #if SIMULATION
            public unsafe void GetInput (byte input) {
                const int messageLength = 1;
                string[] message = new string[messageLength];
                message [0] = input.ToString ();
                updating = true;
                slave.ReadWrite (10, message, messageLength, 2, GetInputCallback);
            }
            #else
            public unsafe void GetInput (byte input) {
                byte message = input;
                updating = true;
                slave.ReadWrite (10, &message, sizeof(byte), sizeof(CommValueBool), GetInputCallback);
            }
            #endif

            #if SIMULATION
            protected void GetInputCallback (CallbackArgs args) {
                int plug = Convert.ToInt32 (args.readMessage [3]);
                bool state = Convert.ToBoolean (args.readMessage [4]);

                if (inputs [plug].state != state) {
                    //do event
                }

                inputs [plug].state = state;
                updating = false;
            }
            #else
            protected void GetInputCallback (CallbackArgs args) {
                CommValueBool vg;

                unsafe {
                    args.copyBuffer (&vg, sizeof(CommValueBool));
                }

                if (inputs [vg.channel].state != vg.value) {
                    //do event
                }

                inputs [vg.channel].state = vg.value;
                updating = false;
            }
            #endif
        }
    }
}

