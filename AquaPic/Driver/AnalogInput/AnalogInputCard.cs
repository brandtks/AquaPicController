using System;
using AquaPic.Utilites;
using AquaPic.SerialBus;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
    public partial class AnalogInput
    {
        private class AnalogInputCard
        {
            public AquaPicBus.Slave slave;
            public byte cardID;
            public string name;
            public AnalogInputChannel[] channels;
            public bool updating;

            public AnalogInputCard (byte address, byte cardID, string name) {
                this.slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address, name + " (Analog Input)");

                this.cardID = cardID;
                this.name = name;
                this.channels = new AnalogInputChannel[4];
                for (int i = 0; i < this.channels.Length; ++i) {
                    this.channels [i] = new AnalogInputChannel (this.name + ".i" + i.ToString ()); 
                }
                this.updating = false;
            }

            public void AddChannel (int ch, string name) {
                channels [ch].name = name;
            }

            public void GetValues () {
                updating = true;
                #if UNSAFE_COMMS
                unsafe {
                    slave.Read (20, sizeof(Int16) * 4, GetValuesCallback);
                }
                #else
                slave.Read (20, sizeof(short) * 4, GetValuesCallback);
                #endif
            }

            protected void GetValuesCallback (CallbackArgs args) {
                short[] values = new short[4];

                #if UNSAFE_COMMS
                unsafe {
                    fixed (short* ptr = values) {
                        args.CopyBuffer (ptr, sizeof(short) * 4);
                    }
                }
                #else
                for (int i = 0; i < values.Length; ++i) {
                    values [i] = args.GetDataFromReadBuffer<short> (i * 2);
                }
                #endif

                for (int i = 0; i < channels.Length; ++i) {
                    if (channels [i].mode == Mode.Auto)
                        channels [i].value = (float)values [i];
                }

                updating = false;
            }

            public void GetValue (byte ch) {
                if (channels [ch].mode == Mode.Auto) {
                    updating = true;
                    #if UNSAFE_COMMS
                    byte message = ch;
                    unsafe {
                        slave.ReadWrite (10, &message, sizeof(byte), sizeof(CommValueInt), GetValueCallback);
                    }
                    #else
                    slave.ReadWrite (10, ch, 3, GetValuesCallback);
                    #endif
                }
            }

            protected void GetValueCallback (CallbackArgs args) {
                #if UNSAFE_COMMS
                CommValueInt vg;

                unsafe {
                    args.CopyBuffer (&vg, sizeof(CommValueInt));
                }
                   
                channels [vg.channel].value = (float)vg.value;
                #else
                byte ch = args.GetDataFromReadBuffer<byte> (0);
                short value = args.GetDataFromReadBuffer<short> (1);
                channels [ch].value = (float)value;
                #endif

                updating = false;
            }
        }
    }
}

