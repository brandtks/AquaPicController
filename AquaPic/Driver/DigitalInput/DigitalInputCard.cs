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
            public int communicationAlarmIndex;
            public DigitalInputInput[] inputs;
            public bool updating;

            public DigitalInputCard (byte address, byte cardID, string name) {
                this.slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address, name + " (Digital Input)");
                this.slave.OnStatusUpdate += OnSlaveStatusUpdate;

                this.cardID = cardID;
                this.name = name;
                this.communicationAlarmIndex = Alarm.Subscribe (
                    this.slave.Address.ToString () + " commication fault");
                this.updating = false;

                int numberInputs = 6;
                this.inputs = new DigitalInputInput[numberInputs];
                for (int i = 0; i < numberInputs; ++i)
                    inputs [i] = new DigitalInputInput (this.name + ".i" + i.ToString ());
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((slave.Status != AquaPicBusStatus.communicationSuccess) ||
                    (slave.Status != AquaPicBusStatus.communicationStart) ||
                    (slave.Status != AquaPicBusStatus.open))
                    Alarm.Post (communicationAlarmIndex);
                else {
                    if (Alarm.CheckAlarming (communicationAlarmIndex))
                        Alarm.Clear (communicationAlarmIndex);
                }
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

