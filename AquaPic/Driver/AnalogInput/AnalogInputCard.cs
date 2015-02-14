using System;
using AquaPic.Globals;
using AquaPic.SerialBus;
using AquaPic.Alarm;

namespace AquaPic.AnalogInputDriver
{
    public partial class AnalogInput
    {
        public class analogInputCard
        {
            private AquaPicBus.Slave _apb;
            private byte _cardID;
            private int _alarmIdx;
            public analogInputChannel[] channels;

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

            public analogInputCard (byte address, byte cardID) {
                try {
                    this._apb = new AquaPicBus.Slave (AquaPicBus.Bus1, address);
                } catch (Exception ex) {
                    Console.WriteLine (ex.ToString ());
                    Console.WriteLine (ex.Message);
                }
                this._cardID = cardID;
                this._alarmIdx = alarm.subscribe ("APB communication error", "Temperature channel at address " + this._apb.address.ToString ());
                this.channels = new analogInputChannel[4];
                for (int i = 0; i < channels.Length; ++i) {
                    channels [i] = new analogInputChannel (AnalogType.None, null); 
                }
            }

            public void addChannel (int ch, AnalogType type, string name) {
                channels [ch].type = type;
                channels [ch].name = name;
            }

            protected void OnSlaveStatusUpdate (object sender) {
                if ((_apb.status != AquaPicBusStatus.communicationSuccess) || (_apb.status != AquaPicBusStatus.communicationStart))
                    alarm.post (_alarmIdx, true);
            }
                
            public void getValues () {
                unsafe {
                    _apb.Read (1, sizeof(float) * 4, getValuesCallback); 
                }
            }

            protected void getValuesCallback (CallbackArgs args) {
                float[] values = new float[4];

                unsafe {
                    fixed (float* ptr = values) {
                        args.copyBuffer (ptr, sizeof(float) * 4);
                    }
                }

                for (int i = 0; i < channels.Length; ++i) {
                    channels [i].value = values [i];
                }
            }

            public void getValue (byte ch) {
                byte message = ch;

                unsafe {
                    _apb.ReadWrite (2, &message, sizeof(byte), sizeof(float), getValueCallback);
                }
            }

            protected void getValueCallback (CallbackArgs args) {
                channelValue ch;

                unsafe {
                    args.copyBuffer (ch, sizeof(channelValue));
                }

                channels [ch.channel].value = ch.value;
            }
        }
    }
}

