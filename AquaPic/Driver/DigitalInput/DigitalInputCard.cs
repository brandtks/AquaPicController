using System;
using AquaPic.SerialBus;
using AquaPic.AlarmRuntime;
using AquaPic.Utilites;
using AquaPic.Globals;

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
                    this.slave.address.ToString () + "commication fault",
                    this.name + "serial communications fault");
                this.updating = false;

                int numberInputs = 6;
                this.inputs = new DigitalInputInput[numberInputs];
                for (int i = 0; i < numberInputs; ++i)
                    inputs [i] = new DigitalInputInput (i.ToString() + " input on " + this.name);
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((slave.status != AquaPicBusStatus.communicationSuccess) || (slave.status != AquaPicBusStatus.communicationStart))
                    Alarm.Post (communicationAlarmIndex);
            }

            public unsafe void GetInputs () {
                slave.Read (20, sizeof(byte), GetInputsCallback);
            }

            protected void GetInputsCallback (CallbackArgs args) {
                byte stateMask;

                unsafe {
                    args.copyBuffer (&stateMask, sizeof(byte));
                }

                for (int i = 0; i < inputs.Length; ++i)
                    inputs [i].state = Utils.mtob (stateMask, i);
            }

            public unsafe void GetInput (byte input) {
                byte message = input;
                updating = true;
                slave.ReadWrite (10, &message, sizeof(byte), sizeof(CommValueBool), GetInputCallback);
            }

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
        }
    }
}

