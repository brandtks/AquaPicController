﻿using System;
using AquaPic.Globals;
using AquaPic.SerialBus;
using AquaPic.AlarmRuntime;

namespace AquaPic.AnalogInputDriver
{
    public partial class AnalogInput
    {
        private class AnalogInputCard
        {
            public AquaPicBus.Slave slave;
            public byte cardID;
            public string name;
            public int communicationAlarmIndex;
            public AnalogInputChannel[] channels;
            public bool updating;

            /* <FUTURE>
            public analogInputCard (byte address, byte cardID, AnalogType[] types, string[] names) {
                this._cardID = cardID;
                this._alarmIdx = alarm.subscribe ("APB communication error", "Temperature channel at address " + this._address.ToString ());
                this.channels = new analogInputChannel[4];
                for (int i = 0; i < channels.Length; ++i) {
                    channels [i] = new analogInputChannel (types [i], names [i]); 
                }
            }
            */

            public AnalogInputCard (byte address, byte cardID, string name) {
                this.slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address);
                this.slave.OnStatusUpdate += OnSlaveStatusUpdate;

                this.cardID = cardID;
                this.name = name;
                this.communicationAlarmIndex = Alarm.Subscribe (address.ToString () + " communication fault", "Analog Input card at address " + this.slave.address.ToString ());
                this.channels = new AnalogInputChannel[4];
                for (int i = 0; i < this.channels.Length; ++i) {
                    this.channels [i] = new AnalogInputChannel (); 
                }
                this.updating = false;
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
                
            public unsafe void GetValues () {
                updating = true;
                slave.Read (20, sizeof(float) * 4, GetValuesCallback); 
            }

            protected void GetValuesCallback (CallbackArgs args) {
                float[] values = new float[4];

                unsafe {
                    fixed (float* ptr = values) {
                        args.copyBuffer (ptr, sizeof(float) * 4);
                    }
                }

                for (int i = 0; i < channels.Length; ++i) {
                    channels [i].value = values [i];
                }
                updating = false;
            }

            public unsafe void GetValue (byte ch) {
                byte message = ch;
                updating = true;
                slave.ReadWrite (10, &message, sizeof(byte), sizeof(CommValueFloat), GetValueCallback);
            }

            protected void GetValueCallback (CallbackArgs args) {
                CommValueFloat vg;

                unsafe {
                    args.copyBuffer (&vg, sizeof(CommValueFloat));
                }

                channels [vg.channel].value = vg.value;
                updating = false;
            }
        }
    }
}

