using System;
using AquaPic.Utilites;
using AquaPic.SerialBus;
using AquaPic.Alarm;

namespace AquaPic.AnalogInput
{
    public class analogInputCard
    {
        private byte _address;
        private byte _cardID;
        private int _alarmIdx;
        public analogInputChannel[] channels;
        public APBstatus commsStatus;

        public analogInputCard (byte address, byte cardID, AnalogType[] types, string[] names) {
            this._address = address;
            this._cardID = cardID;
            this._alarmIdx = alarm.subscribe ("APB communication error", "Temperature channel at address " + this._address.ToString ());
            this.channels = new analogInputChannel[4];
            for (int i = 0; i < channels.Length; ++i) {
                channels [i] = new analogInputChannel (types [i], names [i]); 
            }
        }

        public analogInputCard (byte address, byte cardID) {
            this._address = address;
            this._cardID = cardID;
            this._alarmIdx = alarm.subscribe ("APB communication error", "Temperature channel at address " + this._address.ToString ());
            this.channels = new analogInputChannel[4];
            for (int i = 0; i < channels.Length; ++i) {
                channels [i] = new analogInputChannel (AnalogType.None, null); 
            }
        }

        public void addChannel (int ch, AnalogType type, string name) {
            channels [ch].type = type;
            channels [ch].name = name;
        }

        public void getValues () {
            float[] values = new float[4];
            for (int i = 0; i < values.Length; ++i)
                values [i] = 0.0f;

            unsafe {
                fixed (float* ptr = values) {
                    commsStatus = APB.read (_address, 1, ptr, sizeof(float) * 4);
                }
            }

            if (commsStatus != APBstatus.readSuccess) {
                alarm.post (_alarmIdx, true);
                return;
            }

            for (int i = 0; i < channels.Length; ++i) {
                channels [i].value = values [i];
            }
        }

        public void getValue (byte ch) {
            byte message = ch;
            float value = 0.0f;

            unsafe {
                commsStatus = APB.readWrite (_address, 2, &message, sizeof(byte), &value, sizeof(float));
            }

            if (commsStatus != APBstatus.readWriteSuccess) {
                alarm.post (_alarmIdx, true);
                return;
            }

            channels [ch].value = value;
        }
    }
}

