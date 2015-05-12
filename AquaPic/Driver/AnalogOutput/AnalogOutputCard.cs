using System;
using AquaPic.AlarmRuntime;
using AquaPic.Globals;
using AquaPic.SerialBus;

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
            public string name;

            public AnalogOutputCard (byte address, byte cardID, string name) {
                try {
                    slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address);
                    slave.OnStatusUpdate += OnSlaveStatusUpdate;
                } catch (Exception ex) {
                    Console.WriteLine (ex.ToString ());
                    Console.WriteLine (ex.Message);
                }
                this.cardID = cardID;
                this.name = name;
                communicationAlarmIndex = Alarm.Subscribe(address.ToString () + " communication fault", "Analog output card at address " + this.slave.Address.ToString ());
                channels = new AnalogOutputChannel[4];
                for (int i = 0; i < channels.Length; ++i) {
                    int chId = i;
                    this.channels [chId] = new AnalogOutputChannel (
                        (float value) => SetAnalogValue ((byte)chId, Convert.ToInt32 (value))
                    );
                }
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((slave.Status != AquaPicBusStatus.communicationSuccess) || (slave.Status != AquaPicBusStatus.communicationStart))
                    Alarm.Post (communicationAlarmIndex);
            }

            #if SIMULATION
            public void AddChannel (int ch, AnalogType type, string name) {
                channels [ch].type = type;
                channels [ch].name = name;

                const int messageLength = 2;
                string[] message = new string[messageLength];
                message [0] = ch.ToString ();
                message [1] = type.ToString ();

                slave.Write (2, message, messageLength);
            }
            #else
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
            #endif

            #if SIMULATION
            public void SetAnalogValue (byte channelID, int value) {
                if (value != channels [channelID].value) {
                    channels [channelID].value = value;

                    const int messageLength = 2;
                    string[] message = new string[messageLength];
                    message [0] = channelID.ToString ();
                    message [1] = value.ToString ();

                    slave.Write (31, message, messageLength);
                }
            }
            #else
            public void SetAnalogValue (byte channelID, int value) {
                CommValueInt vs;
                vs.channel = channelID;
                vs.value = value;

                channels [vs.channel].value = vs.value;

                unsafe {
                    slave.Write (31, &vs, sizeof(CommValueInt));
                }
            }
            #endif

            #if SIMULATION
            public void SetAllAnalogValues (int[] values) {
                if (values.Length < 4)
                    return;

                const int messageLength = 4;
                string[] message = new string[messageLength];

                for (int i = 0; i < values.Length; ++i)
                    message [i] = values [i].ToString ();

                slave.Write (30, message, messageLength);
            }
            #else
            public void SetAllAnalogValues (int[] values) {
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
            #endif

            #if SIMULATION

            #else
            public void GetValues () {
                unsafe {
                    slave.Read (20, sizeof(float) * 4, GetValuesCallback); 
                }
            }
            #endif

            #if SIMULATION

            #else
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
            #endif

            #if SIMULATION

            #else
            public void GetValue (byte ch) {
                byte message = ch;

                unsafe {
                    slave.ReadWrite (10, &message, sizeof(byte), sizeof(CommValueInt), GetValueCallback);
                }
            }
            #endif

            #if SIMULATION

            #else
            protected void GetValueCallback (CallbackArgs args) {
                CommValueInt vg;

                unsafe {
                    args.copyBuffer (&vg, sizeof(CommValueInt));
                }

                channels [vg.channel].value = vg.value;
            }
            #endif
        }
    }
}

