using System;
using AquaPic.SerialBus;
using AquaPic.AlarmDriver;
using AquaPic.Globals;

namespace AquaPic.AnalogOutputDriver
{
    public partial class AnalogOutput
    {
        private class AnalogOutputCard
        {
            public AquaPicBus.Slave slave;
            public byte cardID;
            public int communicationAlarmIndex;
            public AnalogOutputChannel[] channels;

            public AnalogOutputCard (byte address, byte cardID) {
                try {
                    this.slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address);
                    this.slave.OnStatusUpdate += OnSlaveStatusUpdate;
                } catch (Exception ex) {
                    Console.WriteLine (ex.ToString ());
                    Console.WriteLine (ex.Message);
                }
                this.cardID = cardID;
                this.communicationAlarmIndex = Alarm.Subscribe(address.ToString () + " communication fault", "Analog output card at address " + this.slave.address.ToString ());
                this.channels = new AnalogOutputChannel[4];
                for (int i = 0; i < this.channels.Length; ++i)
                    this.channels [i] = new AnalogOutputChannel ();
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((slave.status != AquaPicBusStatus.communicationSuccess) || (slave.status != AquaPicBusStatus.communicationStart))
                    Alarm.Post (communicationAlarmIndex);
            }

            public void AddChannel (int ch, AnalogType type, string name) {
                channels [ch].type = type;
                channels [ch].name = name;

                byte[] arr = new byte[2];
                arr [0] = (byte)ch;
                arr [1] = (byte)channels [ch].type;

                unsafe {
                    fixed (byte* ptr = &arr [0]) {
                        slave.Write (2, ptr, sizeof(byte) * 2);
                    }
                }
            }

            public void SetAnalogValue (byte channelID, int value) {
                ValueSetter vs = new ValueSetter ();
                vs.channelID = channelID;
                vs.value = value;

                channels [vs.channelID].value = vs.value;

                unsafe {
                    slave.Write (31, &vs, sizeof(ValueSetter));
                }
            }

            public void SetAllAnalogValues (ref int[] values) {
                if (values.Length < 4)
                    return;

                for (int i = 0; i < 4; ++i) {
                    channels [i].value = values [i];
                }

                unsafe {
                    fixed (int* ptr = &values[0]) {
                        slave.Write (30, ptr, sizeof(int) * 4);
                    }
                }
            }

            public void GetValues () {
                unsafe {
                    slave.Read (20, sizeof(float) * 4, GetValuesCallback); 
                }
            }

            protected void GetValuesCallback (CallbackArgs args) {
                int[] values = new int[4];

                unsafe {
                    fixed (int* ptr = values) {
                        args.copyBuffer (ptr, sizeof(float) * 4);
                    }
                }

                for (int i = 0; i < channels.Length; ++i) {
                    channels [i].value = values [i];
                }
            }

            public void GetValue (byte ch) {
                byte message = ch;

                unsafe {
                    slave.ReadWrite (10, &message, sizeof(byte), sizeof(ValueGetterInt), GetValueCallback);
                }
            }

            protected void GetValueCallback (CallbackArgs args) {
                ValueGetterInt vg;

                unsafe {
                    args.copyBuffer (&vg, sizeof(ValueGetterInt));
                }

                channels [vg.channel].value = vg.value;
            }
        }
    }
}

